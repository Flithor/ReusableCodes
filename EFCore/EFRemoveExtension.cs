using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Text;

using Microsoft.EntityFrameworkCore.Infrastructure;

using Microsoft.EntityFrameworkCore;

namespace Flithor_ReusableCodes
{
    /// <summary>
    /// Make you remove data in ef more easily
    /// </summary>
    public static class EFRemoveExtension
    {
        /// <summary>Query primary key only by condition</summary>
        /// <param name="predicate">Query condition</param>
        public static IQueryable<TEntity> SelectKeyOnly<TEntity>(this DbContext db, Expression<Func<TEntity, bool>> predicate) where TEntity : class
        {
            var entityModel = db.Model.FindEntityType(typeof(TEntity));
            var keys = entityModel.FindPrimaryKey();
            var paramE = predicate.Parameters[0];
            var init = Expression.MemberInit(Expression.New(typeof(TEntity)), keys.Properties.Select(p => Expression.Bind(p.PropertyInfo, Expression.MakeMemberAccess(paramE, p.PropertyInfo))));
            var lambda = Expression.Lambda<Func<TEntity, TEntity>>(init, paramE);
            return db.Set<TEntity>().Where(predicate).Select(lambda);
        }
        /// <summary>Query primary key only by condition</summary>
        /// <param name="predicate">Query condition</param>
        public static IQueryable<TEntity> SelectKeyOnly<TEntity>(this DbSet<TEntity> set, Expression<Func<TEntity, bool>> predicate) where TEntity : class =>
            SelectKeyOnly(set.GetService<ICurrentDbContext>().Context, predicate);

        /// <summary>Delete data by condition</summary>
        /// <param name="predicate">Query condition</param>
        public static void RemoveWhere<TEntity>(this DbContext db, Expression<Func<TEntity, bool>> predicate) where TEntity : class
        {
            db.Set<TEntity>().RemoveRange(SelectKeyOnly(db, predicate));
        }
        /// <summary>Delete data by condition</summary>
        /// <param name="predicate">Query condition</param>
        public static void RemoveWhere<TEntity>(this DbSet<TEntity> set, Expression<Func<TEntity, bool>> predicate) where TEntity : class =>
            RemoveWhere(set.GetService<ICurrentDbContext>().Context, predicate);

        /// <summary>Delete data by condition and return deleted entities</summary>
        /// <param name="predicate">Query condition</param>
        public static List<TEntity> RemoveWhereAndTake<TEntity>(this DbContext db, Expression<Func<TEntity, bool>> predicate) where TEntity : class
        {
            var entities = db.Set<TEntity>().Where(predicate).ToList();
            db.RemoveRange(entities);
            return entities;
        }
        /// <summary>Delete data by condition and return deleted entities</summary>
        /// <param name="predicate">Query condition</param>
        public static List<TEntity> RemoveWhereAndTake<TEntity>(this DbSet<TEntity> set, Expression<Func<TEntity, bool>> predicate) where TEntity : class =>
            RemoveWhereAndTake(set.GetService<ICurrentDbContext>().Context, predicate);

        /// <summary>Delete data by condition and return primary keys of deleted entities</summary>
        /// <param name="predicate">Query condition</param>
        public static List<TEntity> RemoveWhereAndTakeKeys<TEntity>(this DbContext db, Expression<Func<TEntity, bool>> predicate) where TEntity : class
        {
            var keys = SelectKeyOnly(db, predicate).ToList();
            db.RemoveRange(keys);
            return keys;
        }
        /// <summary>Delete data by condition and return primary keys of deleted entities</summary>
        /// <param name="predicate">Query condition</param>
        public static List<TEntity> RemoveWhereAndTakeKeys<TEntity>(this DbSet<TEntity> set, Expression<Func<TEntity, bool>> predicate) where TEntity : class =>
            RemoveWhereAndTakeKeys(set.GetService<ICurrentDbContext>().Context, predicate);
    }
}
