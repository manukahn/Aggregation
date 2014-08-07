// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.OData.Extensions;

namespace System.Web.OData
{
    internal class ExpressionHelperMethods
    {
        private static MethodInfo _orderByMethod = GenericMethodOf(_ => Queryable.OrderBy<int, int>(default(IQueryable<int>), default(Expression<Func<int, int>>)));
        private static MethodInfo _orderByDescendingMethod = GenericMethodOf(_ => Queryable.OrderByDescending<int, int>(default(IQueryable<int>), default(Expression<Func<int, int>>)));
        private static MethodInfo _thenByMethod = GenericMethodOf(_ => Queryable.ThenBy<int, int>(default(IOrderedQueryable<int>), default(Expression<Func<int, int>>)));
        private static MethodInfo _thenByDescendingMethod = GenericMethodOf(_ => Queryable.ThenByDescending<int, int>(default(IOrderedQueryable<int>), default(Expression<Func<int, int>>)));
        private static MethodInfo _countMethod = GenericMethodOf(_ => Queryable.LongCount<int>(default(IQueryable<int>)));
        private static MethodInfo _skipMethod = GenericMethodOf(_ => Queryable.Skip<int>(default(IQueryable<int>), default(int)));
        private static MethodInfo _whereMethod = GenericMethodOf(_ => Queryable.Where<int>(default(IQueryable<int>), default(Expression<Func<int, bool>>)));

        private static MethodInfo _queryableEmptyAnyMethod = GenericMethodOf(_ => Queryable.Any<int>(default(IQueryable<int>)));
        private static MethodInfo _queryableNonEmptyAnyMethod = GenericMethodOf(_ => Queryable.Any<int>(default(IQueryable<int>), default(Expression<Func<int, bool>>)));
        private static MethodInfo _queryableAllMethod = GenericMethodOf(_ => Queryable.All(default(IQueryable<int>), default(Expression<Func<int, bool>>)));

        private static MethodInfo _enumerableEmptyAnyMethod = GenericMethodOf(_ => Enumerable.Any<int>(default(IEnumerable<int>)));
        private static MethodInfo _enumerableNonEmptyAnyMethod = GenericMethodOf(_ => Enumerable.Any<int>(default(IEnumerable<int>), default(Func<int, bool>)));
        private static MethodInfo _enumerableAllMethod = GenericMethodOf(_ => Enumerable.All<int>(default(IEnumerable<int>), default(Func<int, bool>)));

        private static MethodInfo _enumerableOfTypeMethod = GenericMethodOf(_ => Enumerable.OfType<int>(default(IEnumerable)));
        private static MethodInfo _queryableOfTypeMethod = GenericMethodOf(_ => Queryable.OfType<int>(default(IQueryable)));

        private static MethodInfo _enumerableSelectMethod = GenericMethodOf(_ => Enumerable.Select<int, int>(default(IEnumerable<int>), i => i));
        private static MethodInfo _queryableSelectMethod = GenericMethodOf(_ => Queryable.Select<int, int>(default(IQueryable<int>), i => i));

        private static MethodInfo _queryableTakeMethod = GenericMethodOf(_ => Queryable.Take<int>(default(IQueryable<int>), default(int)));
        private static MethodInfo _enumerableTakeMethod = GenericMethodOf(_ => Enumerable.Take<int>(default(IEnumerable<int>), default(int)));


        private static MethodInfo _groupByMethod = GenericMethodOf(_ => Queryable.GroupBy<int, int>(default(IQueryable<int>), default(Expression<Func<int, int>>), default(IEqualityComparer<int>)));
        private static MethodInfo _queriableAggregateMethod = GenericMethodOf(_ => Queryable.Aggregate<double>(default(IQueryable<double>), default(Expression<Func<double, double, double>>)));

        private static MethodInfo _castMethod = GenericMethodOf(_ => Queryable.Cast<int>(default(IQueryable<int>)));
        private static MethodInfo _enumerableToListMethod = GenericMethodOf(_ => Enumerable.ToList<int>(default(IEnumerable<int>)));
        private static MethodInfo _asQueriableMethod = GenericMethodOf(_ => Queryable.AsQueryable<int>(default(IEnumerable<int>)));
        private static MethodInfo _distinctsMethod = GenericMethodOf(_ => Queryable.Distinct<int>(default(IQueryable<int>)));
        private static MethodInfo _minMethod = GenericMethodOf(_ => Queryable.Min<int>(default(IQueryable<int>)));
        private static MethodInfo _maxMethod = GenericMethodOf(_ => Queryable.Max<int>(default(IQueryable<int>)));
        private static MethodInfo _simpleCountMethod = GenericMethodOf(_ => Queryable.Count<int>(default(IQueryable<int>)));

        private static Dictionary<Type, MethodInfo> _queriableSelectAndSumMethods = new Dictionary<Type, MethodInfo>();
        private static Dictionary<Type, MethodInfo> _queriableSelectAndAverageMethods = new Dictionary<Type, MethodInfo>();

        static ExpressionHelperMethods()
        {
            var types = new Type[]
            {
                typeof(int), typeof(int?), typeof(long), typeof(long?), typeof(float), typeof(float?),
                typeof(double), typeof(double?), typeof(decimal), typeof(decimal?)
            };
            foreach (var t in types)
            {
                _queriableSelectAndSumMethods.Add(t, GetQueryableSpecificMethodInfo("Sum", t).GetGenericMethodDefinition());
                _queriableSelectAndAverageMethods.Add(t, GetQueryableSpecificMethodInfo("Average", t).GetGenericMethodDefinition());
            }
        }

        public static MethodInfo QueryableSimpleCountGeneric
        {
            get { return _simpleCountMethod; }
        }

        public static MethodInfo QueryableMinGeneric
        {
            get { return _minMethod; }
        }

        public static MethodInfo QueryableMaxGeneric
        {
            get { return _maxMethod; }
        }

        public static MethodInfo QueryableDistinctGeneric
        {
            get { return _distinctsMethod; }
        }

        public static MethodInfo EnumerabltAsQueriableGeneric
        {
            get { return _asQueriableMethod; }
        }

        public static MethodInfo EnumerabltToListGeneric
        {
            get { return _enumerableToListMethod; }
        }


        public static MethodInfo QueryableCastByGeneric
        {
            get { return _castMethod; }
        }

        public static MethodInfo QueryableGroupByGeneric
        {
            get { return _groupByMethod; }
        }

        public static MethodInfo QueryableAggregateGeneric
        {
            get { return _queriableAggregateMethod; }
        }

        public static MethodInfo QueryableOrderByGeneric
        {
            get { return _orderByMethod; }
        }

        public static MethodInfo QueryableOrderByDescendingGeneric
        {
            get { return _orderByDescendingMethod; }
        }

        public static MethodInfo QueryableThenByGeneric
        {
            get { return _thenByMethod; }
        }

        public static MethodInfo QueryableThenByDescendingGeneric
        {
            get { return _thenByDescendingMethod; }
        }

        public static MethodInfo QueryableCountGeneric
        {
            get { return _countMethod; }
        }

        public static MethodInfo QueryableTakeGeneric
        {
            get { return _queryableTakeMethod; }
        }

        public static MethodInfo EnumerableTakeGeneric
        {
            get { return _enumerableTakeMethod; }
        }

        public static MethodInfo QueryableSkipGeneric
        {
            get { return _skipMethod; }
        }

        public static MethodInfo QueryableWhereGeneric
        {
            get { return _whereMethod; }
        }

        public static MethodInfo QueryableSelectGeneric
        {
            get { return _queryableSelectMethod; }
        }

        public static MethodInfo EnumerableSelectGeneric
        {
            get { return _enumerableSelectMethod; }
        }

        public static MethodInfo QueryableEmptyAnyGeneric
        {
            get { return _queryableEmptyAnyMethod; }
        }

        public static MethodInfo QueryableNonEmptyAnyGeneric
        {
            get { return _queryableNonEmptyAnyMethod; }
        }

        public static MethodInfo QueryableAllGeneric
        {
            get { return _queryableAllMethod; }
        }

        public static MethodInfo EnumerableEmptyAnyGeneric
        {
            get { return _enumerableEmptyAnyMethod; }
        }

        public static MethodInfo EnumerableNonEmptyAnyGeneric
        {
            get { return _enumerableNonEmptyAnyMethod; }
        }

        public static MethodInfo EnumerableAllGeneric
        {
            get { return _enumerableAllMethod; }
        }

        public static MethodInfo EnumerableOfType
        {
            get { return _enumerableOfTypeMethod; }
        }

        public static MethodInfo QueryableOfType
        {
            get { return _queryableOfTypeMethod; }
        }

        public static MethodInfo QueryableSelectAndSumGeneric(Type resultType)
        {
            return _queriableSelectAndSumMethods[resultType];
        }

        public static MethodInfo QueryableSelectAndAverageGeneric(Type resultType)
        {
            return _queriableSelectAndAverageMethods[resultType];
        }


        private static MethodInfo GenericMethodOf<TReturn>(Expression<Func<object, TReturn>> expression)
        {
            return GenericMethodOf(expression as Expression);
        }

        private static MethodInfo GenericMethodOf(Expression expression)
        {
            LambdaExpression lambdaExpression = expression as LambdaExpression;

            Contract.Assert(expression.NodeType == ExpressionType.Lambda);
            Contract.Assert(lambdaExpression != null);
            Contract.Assert(lambdaExpression.Body.NodeType == ExpressionType.Call);

            return (lambdaExpression.Body as MethodCallExpression).Method.GetGenericMethodDefinition();
        }

        /// <summary>
        /// Get the method info of methods such as: public static double Sum<TSource>(this IQueryable<TSource> source, Expression<Func<TSource,double>> selector)
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="resultType"></param>
        public static MethodInfo GetQueryableSpecificMethodInfo(string methodName, Type resultType)
        {
            var q = typeof(Queryable).GetMethods()
                .Where(mi => mi.IsGenericMethod && mi.Name == methodName && mi.GetParameters().Count() == 2);

            return q.First(mi => mi.GetParameters()
                            .Second()
                            .ParameterType.GetGenericArguments()
                            .First()
                            .GetGenericArguments()
                            .Second()
                            .FullName == resultType.FullName);
        }
    }
}
