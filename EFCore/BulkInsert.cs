using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace MyReusableCodes
{
    public static class EFExtensions
    {
        #region 批量插入
        /// <summary>
        /// 高效率批量插入
        /// </summary>
        /// <typeparam name="TEntity">目标实体类型</typeparam>
        /// <typeparam name="TData">数据源类型</typeparam>
        /// <param name="db">数据上下文</param>
        /// <param name="datas">数据源</param>
        /// <param name="dbTransaction">事务,没有事务时可传递null</param>
        /// <param name="batchSize">每批次数量</param>
        /// <param name="setValueExpression">设置属性的表达式</param>
        /// <returns></returns>
        public static int BulkInsert<TEntity, TData>(
            this DbContext db,
            IEnumerable<TData> datas,
            DbTransaction dbTransaction,
            int batchSize,
            Expression<Func<TData, TEntity>> setValueExpression) where TEntity : class
        => BulkInsert(db, datas, dbTransaction, batchSize, setValueExpression, new (string propName, Func<TData, Type, object> valueSelector)[0]);
        /// <summary>
        /// 高效率批量插入
        /// </summary>
        /// <typeparam name="TEntity">目标实体类型</typeparam>
        /// <typeparam name="TData">数据源类型</typeparam>
        /// <param name="db">数据上下文</param>
        /// <param name="datas">数据源</param>
        /// <param name="dbTransaction">事务,没有事务时可传递null</param>
        /// <param name="batchSize">每批次数量</param>
        /// <param name="setValueExpression">设置属性的表达式</param>
        /// <param name="shadowPropertySetters">设置影子属性的表达式字典</param>
        /// <returns></returns>
        public static int BulkInsert<TEntity, TData>(
            this DbContext db,
            IEnumerable<TData> datas,
            DbTransaction dbTransaction,
            int batchSize,
            Expression<Func<TData, TEntity>> setValueExpression,
            Dictionary<string, Func<TData, Type, object>> shadowPropertySetters) where TEntity : class
        => BulkInsert(db, datas, dbTransaction, batchSize, setValueExpression, shadowPropertySetters.Select(kv => (kv.Key, kv.Value)).ToArray());
        /// <summary>
        /// 高效率批量插入
        /// </summary>
        /// <typeparam name="TEntity">目标实体类型</typeparam>
        /// <typeparam name="TData">数据源类型</typeparam>
        /// <param name="db">数据上下文</param>
        /// <param name="datas">数据源</param>
        /// <param name="dbTransaction">事务,没有事务时可传递null</param>
        /// <param name="batchSize">每批次数量</param>
        /// <param name="setValueExpression">设置属性的表达式</param>
        /// <param name="shadowPropertySetters">设置影子属性的表达式集合</param>
        /// <returns></returns>
        public static int BulkInsert<TEntity, TData>(
            this DbContext db,
            IEnumerable<TData> datas,
            DbTransaction dbTransaction,
            int batchSize,
            Expression<Func<TData, TEntity>> setValueExpression,
            params (string propName, Func<TData, Type, object> valueSelector)[] shadowPropertySetters) where TEntity : class
        {
            //var sw = Stopwatch.StartNew();
            #region 命令准备
            //实体类类型
            var entityType = typeof(TEntity);

            //获取数据库连接对象
            var conn = db.Database.GetDbConnection();
            if (conn.State == ConnectionState.Closed)
                conn.Open();
            //获取数据库命令对象
            var cmd = conn.CreateCommand();
            //设置事务
            cmd.Transaction = dbTransaction;
            #endregion

            #region 执行准备
            //获取实体类注册属性信息(为影子属性服务,没有设置影子属性时为空不使用)
            var modelProperties = shadowPropertySetters.Length > 0 ? db.Model.FindEntityType(entityType).GetProperties() : null;
            //构建数据库命令参数序列并获取取值信息
            //var setValueInfos = TranslateExpressionToDbParameters(cmd, modelProperties, setValueExpression, shadowPropertySetters, batchSize);
            var setValueInfo = BuildEntitySetValueInfo(cmd, batchSize, modelProperties, setValueExpression, shadowPropertySetters);
            //单批次计数
            var counting = 0;
            //获取枚举器以支持懒加载
            var dataEnumerator = datas.GetEnumerator();
            //每批次的数据
            var takes = new List<TData>();
            //退出循环标识
            var exitFlag = true;
            //设置批量插入语句
            cmd.CommandText = CreateBulkInsertSql(entityType.Name, setValueInfo.EntityPropInfos, batchSize);
            //受影响的行数
            int rowsAffected = 0;
            //准备开始循环执行
            cmd.Prepare();
            #endregion

            //Debug.WriteLine($"前期准备耗时：{sw.ElapsedMilliseconds} ms");
            //sw.Restart();
            do
            {
                //循环读取下一项
                if (dataEnumerator.MoveNext())
                {
                    //读取到下一项则计数+1,并加入序列
                    counting++;
                    takes.Add(dataEnumerator.Current);
                }
                else//到序列末尾则标记离开循环
                    exitFlag = false;
                //当序列不为空时,计数大小达到批量计数尺寸,或即将离开循环时,调用执行
                if (takes.Count > 0 && (counting >= batchSize || !exitFlag))
                {
                    if (!exitFlag)//是离开循环,但长度不足时需要重设命令参数长度信息
                    {
                        cmd.CommandText = CreateBulkInsertSql(entityType.Name, setValueInfo.EntityPropInfos, counting);
                    }
                    //设置当前批次的参数值
                    setValueInfo.SetParametersDelegate(cmd.Parameters, takes.ToArray());
                    //检查参数值是否存在空值,替换为DBNull
                    foreach (DbParameter param in cmd.Parameters)
                    {
                        if (param.Value == null)
                            param.Value = DBNull.Value;
                    }
                    //Debug.WriteLine($"准备数据耗时：{sw.ElapsedMilliseconds} ms");
                    //sw.Restart();
                    //执行SQL
                    rowsAffected += cmd.ExecuteNonQuery();
                    //Debug.WriteLine($"执行命令耗时：{sw.ElapsedMilliseconds} ms");
                    //sw.Restart();
                    //每次执行后清空序列
                    takes.Clear();
                    //重置单批次计数
                    counting = 0;
                }
            } while (exitFlag);//当退出标识为false时结束循环
            cmd.Parameters.Clear();
            return rowsAffected;//返回受影响的行数
        }
        /// <summary>
        /// 根据数量创建批量插入语句
        /// </summary>
        /// <typeparam name="TData">数据源类型</typeparam>
        /// <param name="tableName">表名</param>
        /// <param name="entityPropInfos">取值信息</param>
        /// <param name="batchSize">每批次数量</param>
        /// <returns></returns>
        private static string CreateBulkInsertSql(string tableName, IList<EntityPropInfo> entityPropInfos, int batchSize)
        {
            //参数索引号,依次递增
            var pIndex = 0;
            //"(@p1,@p2,@p3,...)"
            var valuesParts = new List<string>();
            for (int i = 0; i < batchSize; i++)
            {
                //"@p1","@p2","@p3"...
                var pMarks = new List<string>();
                for (int j = 0; j < entityPropInfos.Count; j++)
                    pMarks.Add("@p" + pIndex++);

                valuesParts.Add($"({string.Join(",", pMarks)})");
            }
            //INSERT INTO TableName(col1,col2,col3...)
            //VALUES(@p1,@p2,@p3,...),(@p....
            return $"INSERT INTO {tableName}({string.Join(",", entityPropInfos.Select(sv => sv.PropName))})" +
                "VALUES" + string.Join(",", valuesParts);
        }
        ///// <summary>
        ///// 批量设置参数值
        ///// </summary>
        ///// <typeparam name="TData">数据源类型</typeparam>
        ///// <param name="parameters">数据库参数集合对象</param>
        ///// <param name="datas">数据源</param>
        ///// <param name="setValueInfos">取值信息</param>
        //private static void BulkSetParameters<TData>(DbParameterCollection parameters, ICollection<TData> datas, IList<EntityPropInfo> entityPropInfos)
        //{
        //    var pIndex = 0;
        //    foreach (var data in datas)
        //    {
        //        foreach (var setValueInfo in entityPropInfos)
        //        {
        //            //按顺序为数据库参数赋值,使用生成的取值委托从对象取值
        //            parameters["@p" + pIndex++].Value = setValueInfo.GetDataValueDelegate?.DynamicInvoke(data, setValueInfo.SourceTypePropType);
        //        }
        //    }
        //}

        /// <summary>
        /// 翻译表达式树到数据库参数,并返回取值信息
        /// </summary>
        /// <typeparam name="TEntity">目标实体类</typeparam>
        /// <typeparam name="TData"></typeparam>
        /// <param name="cmd">数据库命令对象</param>
        /// <param name="modelProperties">模型参数,仅对影子属性生效,可能为空</param>
        /// <param name="setValueExpression">需要解析的对象初始化表达式树</param>
        /// <param name="shadowPropertySetters">设置影子属性的表达式集合</param>
        /// <param name="batchSize">每批次数量</param>
        /// <returns></returns>
        private static SetValueInfo<TData>[] TranslateExpressionToDbParameters<TEntity, TData>(
            DbCommand cmd,
            IEnumerable<IProperty> modelProperties,
            Expression<Func<TData, TEntity>> setValueExpression,
            (string entityPropName, Expression<Func<TData, Type, object>> valueSelector)[] shadowPropertySetters,
            int batchSize) where TEntity : class
        {
            //将模型参数转换为字典,仅对影子属性有效,可能为空
            var propClrTypeDic = modelProperties?.ToDictionary(p => p.Name, p => p.ClrType);
            //获取表达式树中的对象成员初始化器中的赋值表达式
            //[Prop = obj.Prop],
            var settingMembers = (setValueExpression.Body as MemberInitExpression).Bindings.Cast<MemberAssignment>();
            //将成员初始化器的赋值表达式换为取值信息
            var setValueInfos = settingMembers
                .Select(member =>
                {
                    //赋值表达式中被赋值的属性信息
                    //[Prop] = obj.Prop,
                    var prop = member.Member as PropertyInfo;
                    //赋值表达式中取值的访问属性表达式
                    //Prop = [obj.Prop],
                    var mExpression = member.Expression as MemberExpression;

                    return new SetValueInfo<TData>
                    {
                        SourceTypePropName = member.Member.Name,
                        SourceTypePropType = prop.PropertyType,
                        SourceTypePropDbType = ToDbType(prop.PropertyType),
                        //构建委托表达式并编译成委托以进行调用
                        //[(obj,type) => obj.Prop]
                        GetDataValueDelegate = Expression.Lambda(mExpression/*[obj.Prop]*/,
                            mExpression.Expression as ParameterExpression/*[obj]*/ ?? Expression.Parameter(typeof(object))/*dummy*/,
                            Expression.Parameter(typeof(Type), "type")/*[type]*/).Compile(),
                    };
                }).Concat(shadowPropertySetters.Select(t =>
                {
                    var propType = propClrTypeDic.GetValueOrDefault(t.entityPropName);
                    return new SetValueInfo<TData>
                    {
                        SourceTypePropName = t.entityPropName,
                        SourceTypePropType = propType,
                        SourceTypePropDbType = ToDbType(propType),
                        //更复杂的取值表达式,可以获取到目标影子属性的数据类型并方便值转换
                        GetDataValueDelegate = t.valueSelector.Compile()
                    };
                })).ToArray();
            int pIndex = 0;
            for (int batch = 0; batch < batchSize; batch++)
            {
                foreach (var member in setValueInfos)
                {
                    //循环添加参数
                    SetParameter(cmd, member.SourceTypePropDbType, pIndex++);
                }
            }
            return setValueInfos;
        }

        private static EntitySetValueInfo<TData> BuildEntitySetValueInfo<TEntity, TData>(DbCommand cmd,
            int batchSize,
            IEnumerable<IProperty> modelProperties,
            Expression<Func<TData, TEntity>> setValueExpression,
            (string entityPropName, Func<TData, Type, object> valueSelector)[] shadowPropertySetters) where TEntity : class
        {
            var result = new EntitySetValueInfo<TData>();

            //目标函数行格式: paras["@p" + pIndex.ToString()].Value = data.[GetValueExpression];


            //声明传入的datas参数
            var paramDatas = Expression.Parameter(typeof(TData[]), "datas");

            //构建使用构建的索引访问参数值的成员访问表达式
            #region dbParam["@p" + pIndex].Value

            #region dbParam[""].Value
            Expression<Func<DbParameterCollection, object>> expSetParaValue = dbParams => dbParams[""].Value;
            //dbParams[""].Value
            var expParaValue = expSetParaValue.Body as MemberExpression;
            //dbParams[""]
            var expGetPara = expParaValue.Expression as MethodCallExpression;
            #endregion

            #region "@p" + pIndex
            //"@p"
            var pMark = Expression.Constant("@p");
            //"var pIndex;"
            var pIndexParam = Expression.Parameter(typeof(int), "pIndex");
            //数字1
            var expNumberOne = Expression.Constant(1);
            //pIndex += 1
            var pIndexPlus1 = Expression.AddAssign(pIndexParam, expNumberOne);
            //"@p" + pIndex
            var pMarkAddpIndex = Expression.Call(
                typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) }),
                pMark,
                Expression.Call(pIndexParam, typeof(object).GetMethod("ToString")));
            #endregion

            //dbParams["@p" + pIndex]
            expGetPara = expGetPara.Update(expGetPara.Object, new[] { pMarkAddpIndex });
            //dbParams["@p" + pIndex].Value
            expParaValue = expParaValue.Update(expGetPara);
            #endregion
            //构建访问对象的
            #region dbParam["@p" + pIndex].Value = [value]
            var settingMembers = (setValueExpression.Body as MemberInitExpression).Bindings.Cast<MemberAssignment>();
            //声明一个类型为TData的对象参数,名为data
            //ParameterExpression paramData = Expression.Parameter(typeof(TData), "data");
            ParameterExpression paramData = setValueExpression.Parameters[0];
            //声明一个对象序列索引参数,名为dIndex
            var dIndexParam = Expression.Parameter(typeof(int), "dIndex");
            //声明使用索引从序列获取TData对象的表达式
            var expData = Expression.ArrayIndex(paramDatas, dIndexParam);//datas[dIndex]
            //所有设置参数值的表达式集合
            List<Expression> paramAssignmentExpressions = new List<Expression>();
            //将获取TData对象的值赋值给参数data,加入表达式集合
            paramAssignmentExpressions.Add(Expression.Assign(paramData, expData));//var data = datas[dIndex];

            foreach (var member in settingMembers)
            {
                //赋值表达式中被赋值的属性信息
                //[Prop] = obj.Prop,
                var prop = member.Member as PropertyInfo;
                result.EntityPropInfos.Add(new EntityPropInfo(prop.Name, prop.PropertyType, ToDbType(prop.PropertyType)));
                //dbParam["@p" + pIndex].Value = (object)[value]
                var assignParaValue = Expression.Assign(expParaValue, Expression.Convert(member.Expression, typeof(object)));

                paramAssignmentExpressions.Add(assignParaValue);//dbParam["@p" + pIndex].Value = data.Prop as object
                paramAssignmentExpressions.Add(pIndexPlus1);//pIndex+=1
            }

            if (shadowPropertySetters != null)
            {
                //将模型参数转换为字典,仅对影子属性有效,可能为空
                var propClrTypeDic = modelProperties?.ToDictionary(p => p.Name, p => p.ClrType);
                foreach (var setter in shadowPropertySetters)
                {
                    string propName = setter.entityPropName;
                    var propType = propClrTypeDic[propName];
                    //赋值表达式中被赋值的影子属性信息
                    result.EntityPropInfos.Add(new EntityPropInfo(propName, propType, ToDbType(propType)));
                    var selectorMethodInfo = setter.valueSelector.Method;
                    var expGetShadowPropValue = Expression.Call(Expression.Constant(setter.valueSelector.Target), setter.valueSelector.Method/*(data,type)=>[Method]*/, paramData/*data*/, Expression.Constant(propType)/*type*/);

                    //dbParam["@p" + pIndex].Value = (data,type) => [Method] as object
                    var assignParaValue = Expression.Assign(expParaValue, expGetShadowPropValue);

                    paramAssignmentExpressions.Add(assignParaValue);//dbParam["@p" + pIndex].Value = (data,type) => [Method] as object
                    paramAssignmentExpressions.Add(pIndexPlus1);//pIndex+=1
                }
            }
            var dIndexPlus1 = Expression.AddAssign(dIndexParam, expNumberOne);//dIndex+=1
            paramAssignmentExpressions.Add(dIndexPlus1);
            #endregion

            /*已构建好单个对象的表达式列表 paramAssignmentExpressions
             * var data = datas[dIndex];
             * dbParam["@p" + pIndex].Value = data.Prop1;
             * pIndex += 1
             * dbParam["@p" + pIndex].Value = data.Prop2;
             * pIndex += 1
             * dbParam["@p" + pIndex].Value = data.Prop3;
             * pIndex += 1
             * ...
             * dIndex += 1;
             */
            //接下来构建循环块

            var exitLoopLabel = Expression.Label();
            var expDatasLength = Expression.MakeMemberAccess(paramDatas, typeof(Array).GetProperty("Length"));
            var expLoop = Expression.Loop(
                Expression.IfThenElse(Expression.LessThan(dIndexParam, expDatasLength),//dIndex < datas.Length
                    Expression.Block(new[] { paramData }, paramAssignmentExpressions.ToArray()),//表达式列表
                    Expression.Break(exitLoopLabel)
                ), exitLoopLabel);
            /*已构建好循环表达式
             * do
             * {
             *   var data = datas[dIndex];
             *   dbParam["@p" + pIndex].Value = data.Prop1;
             *   pIndex += 1
             *   dbParam["@p" + pIndex].Value = data.Prop2;
             *   pIndex += 1
             *   dbParam["@p" + pIndex].Value = data.Prop3;
             *   ...
             *   dIndex += 1;
             * } while(dIndex < datas.Length);
             */

            //构建完整的函数
            /* (datas,dbParam) =>
             * {
             *   int pIndex = 0;
             *   int dIndex = 0;
             *   
             *   do
             *   {
             *     var data = datas[dIndex];
             *     dbParam["@p" + pIndex].Value = data.Prop1;
             *     pIndex += 1
             *     dbParam["@p" + pIndex].Value = data.Prop2;
             *     pIndex += 1
             *     dbParam["@p" + pIndex].Value = data.Prop3;
             *     ...
             *     dIndex += 1;
             *   } while(dIndex < datas.Length);
             * }
             */
            result.SetParametersDelegate = Expression.Lambda<Action<DbParameterCollection, TData[]>>(
                Expression.Block(new[] { pIndexParam, dIndexParam }, expLoop),
                expSetParaValue.Parameters[0], paramDatas).Compile();

            //预定义所有参数
            int pIndex = 0;
            for (int batch = 0; batch < batchSize; batch++)
            {
                foreach (var member in result.EntityPropInfos)
                {
                    //循环添加参数
                    SetParameter(cmd, member.DbType, pIndex++);
                }
            }

            return result;
        }
        /// <summary>
        /// 新增参数
        /// </summary>
        private static DbParameter SetParameter(DbCommand cmd, DbType dbType, int pIndex)
        {
            var para = cmd.CreateParameter();
            para.ParameterName = "@p" + pIndex;
            para.DbType = dbType;
            cmd.Parameters.Add(para);
            return para;
        }
        /// <summary>
        /// 将Clr类型映射为对应的数据库类型
        /// </summary>
        /// <param name="clrType"></param>
        /// <returns></returns>
        private static DbType ToDbType(Type clrType)
        {
            var GetUnderlyingType = Nullable.GetUnderlyingType(clrType);
            if (GetUnderlyingType != null)
                clrType = GetUnderlyingType;
            switch (Type.GetTypeCode(clrType))
            {
                case TypeCode.Empty:
                case TypeCode.DBNull:
                    break;
                case TypeCode.Char:
                case TypeCode.Object:
                case TypeCode.String:
                    return DbType.String;
                case TypeCode.DateTime:
                    return DbType.DateTime;
                case TypeCode.Boolean:
                    return DbType.Boolean;
                case TypeCode.SByte:
                    return DbType.SByte;
                case TypeCode.Byte:
                    return DbType.Byte;
                case TypeCode.Int16:
                    return DbType.Int16;
                case TypeCode.UInt16:
                    return DbType.UInt16;
                case TypeCode.Int32:
                    return DbType.Int32;
                case TypeCode.UInt32:
                    return DbType.UInt32;
                case TypeCode.Int64:
                    return DbType.Int64;
                case TypeCode.UInt64:
                    return DbType.UInt64;
                case TypeCode.Single:
                    return DbType.Single;
                case TypeCode.Double:
                    return DbType.Double;
                case TypeCode.Decimal:
                    return DbType.Decimal;
            }
            throw new Exception("未知错误");
        }

        /// <summary>
        /// 取值信息
        /// </summary>
        /// <typeparam name="TData">数据源类型</typeparam>
        private class SetValueInfo<TData>
        {
            /// <summary>
            /// 目标属性名称
            /// </summary>
            public string SourceTypePropName { get; set; }
            /// <summary>
            /// 目标属性类型
            /// </summary>
            public Type SourceTypePropType { get; set; }
            /// <summary>
            /// 目标属性类型对应数据库类型
            /// </summary>
            public DbType SourceTypePropDbType { get; set; }
            /// <summary>
            /// 取值委托
            /// </summary>
            public Delegate GetDataValueDelegate { get; set; }
        }
        /// <summary>
        /// 对象取值信息
        /// </summary>
        /// <typeparam name="TData">数据源类型</typeparam>
        private class EntitySetValueInfo<TData>
        {
            /// <summary>
            /// 目标属性信息
            /// </summary>
            public List<EntityPropInfo> EntityPropInfos { get; } = new List<EntityPropInfo>();
            /// <summary>
            /// 取值委托
            /// </summary>
            public Action<DbParameterCollection, TData[]> SetParametersDelegate { get; set; }
        }
        private struct EntityPropInfo
        {
            /// <summary>
            /// 实体属性名称
            /// </summary>
            public readonly string PropName;
            /// <summary>
            /// 实体属性类型
            /// </summary>
            public readonly Type PropType;
            /// <summary>
            /// 实体属性类型对应数据库类型
            /// </summary>
            public readonly DbType DbType;

            public EntityPropInfo(string propName, Type propType, DbType dbType)
            {
                PropName = propName;
                PropType = propType;
                DbType = dbType;
            }
        }
        #endregion
    }
}
