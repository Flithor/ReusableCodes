using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Flithors_ReusableCodes
{
    /// <summary>
    /// Make <see cref="IQueryable{T}"/> support or predicate in linq way
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IQueryOr<T>
    {
        IQueryOr<T> WhereOr(Expression<Func<T, bool>> predicate);
        IQueryable<T> AsQueryable();
    }
    /// <summary>
    /// The extension methods about or predicate
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class OrPredicate
    {
        /// <summary>
        /// Private or predicate builder
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private class OrPredicateBuilder<T> : IQueryOr<T>
        {
            List<Expression<Func<T, bool>>> predicates = new List<Expression<Func<T, bool>>>();
            IQueryable<T> sourceQueryable;

            #region private methods
            internal OrPredicateBuilder(IQueryable<T> sourceQueryable) => this.sourceQueryable = sourceQueryable;
            private OrPredicate(IQueryable<T> sourceQueryable, IEnumerable<Expression<Func<T, bool>>> predicates)
            {
                this.sourceQueryable = sourceQueryable;
                this.predicates.AddRange(predicates);
            }

            //===============================================
            // Code From: https://stackoverflow.com/a/50414456/6859121
            private class ExpressionReplacer : ExpressionVisitor
            {
                private readonly Func<Expression, Expression> replacer;

                public ExpressionReplacer(Func<Expression, Expression> replacer)
                {
                    this.replacer = replacer;
                }

                public override Expression Visit(Expression node)
                {
                    return base.Visit(replacer(node));
                }
            }
            private static TExpression ReplaceParameter<TExpression>(TExpression expr, ParameterExpression toReplace, ParameterExpression replacement) where TExpression : Expression
            {
                var replacer = new ExpressionReplacer(e => e == toReplace ? replacement : e);
                return (TExpression)replacer.Visit(expr);
            }
            private static Expression<Func<TEntity, TReturn>> Join<TEntity, TReturn>(Func<Expression, Expression, BinaryExpression> joiner, IReadOnlyCollection<Expression<Func<TEntity, TReturn>>> expressions)
            {
                if (!expressions.Any())
                {
                    throw new ArgumentException("No expressions were provided");
                }
                var firstExpression = expressions.First();
                if (expressions.Count == 1)
                {
                    return firstExpression;
                }
                var otherExpressions = expressions.Skip(1);
                var firstParameter = firstExpression.Parameters.Single();
                var otherExpressionsWithParameterReplaced = otherExpressions.Select(e => ReplaceParameter(e.Body, e.Parameters.Single(), firstParameter));
                var bodies = new[] { firstExpression.Body }.Concat(otherExpressionsWithParameterReplaced);
                var joinedBodies = bodies.Aggregate(joiner);
                return Expression.Lambda<Func<TEntity, TReturn>>(joinedBodies, firstParameter);
            }
            //================================================
            private Expression<Func<T, bool>> GetExpression() => Join(Expression.Or, predicates);
            #endregion

            #region public methods
            public IQueryOr<T> WhereOr(Expression<Func<T, bool>> predicate)
            {
                predicates.Add(predicate);
                return new OrPredicate<T>(sourceQueryable, predicates);
            }
            public IQueryable<T> AsQueryable()
            {
                if (predicates.Count > 0)
                    return sourceQueryable.Where(GetExpression());
                else // If not any predicates exists, returns orignal query
                    return sourceQueryable;
            }
            #endregion
        }

        /// <summary>
        /// Convert <see cref="IQueryable{T}"/> to <see cref="IQueryOr{T}"/> to make next condition append as or predicate.
        /// Call <see cref="IQueryOr{T}.AsQueryable"/> back to <see cref="IQueryable{T}"/> linq.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IQueryOr<TSource> AsWhereOr<TSource>(this IQueryable<TSource> source)
        {
            return new OrPredicateBuilder<TSource>(source);
        }
    }
}
