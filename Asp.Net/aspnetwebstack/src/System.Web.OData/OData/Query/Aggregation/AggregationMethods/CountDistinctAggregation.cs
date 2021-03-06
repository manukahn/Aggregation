﻿using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Http;
using Microsoft.OData.Core.UriParser.Semantic;

namespace System.Web.OData.OData.Query.Aggregation.AggregationMethods
{
    /// <summary>
    /// countdistinct aggregation method implementation.
    /// </summary>
    [AggregationMethod("countdistinct")]
    public class CountDistinctAggregation : AggregationImplementationBase
    {
        /// <summary>
        /// Implement the Count-Distinct aggregation method
        /// </summary>
        /// <param name="elementType">The type of entities</param>
        /// <param name="query">The collection</param>
        /// <param name="transformation">The transformation clause created by the parser</param>
        /// <param name="propertyToAggregateExpression">Projection Expression that defines access to the property to aggregate</param>
        /// <param name="parameters">A list of string parameters sent to the aggregation method</param>
        /// <returns>The Sum result</returns>
        public override object DoAggregatinon(Type elementType, IQueryable collection, ApplyAggregateClause transformation, LambdaExpression propertyToAggregateExpression, params string[] parameters)
        {
            var propertyType = GetAggregatedPropertyType(elementType, transformation.AggregatableProperty);
            var selectedValues = GetSelectedValues(elementType, collection, transformation, propertyToAggregateExpression);

            //call: (selected.AsQueryable() as IQueryable<double>).Ditinct();
            var distinct = ExpressionHelpers.Distinct(propertyType, selectedValues);

            try
            {
                //call: (distinct.AsQueryable() as IQueryable<double>).Count();
                return ExpressionHelpers.Count(propertyType, distinct);
            }
            catch (TargetInvocationException)
            {
                //salve a problem in mongo that throw the error "No further operators may follow Distinct in a LINQ query." when trying to construct the expression tree.
                distinct = ExpressionHelpers.Cast(propertyType, distinct.AllElements().AsQueryable());
                return ExpressionHelpers.Count(propertyType, distinct);
            }
        }

        /// <summary>
        /// The type of the result of Count-Distinct aggregation method is: int
        /// </summary>
        /// <param name="elementType">>The type of entities</param>
        /// <param name="transformation">The transformation clause created by the parser</param>
        /// <returns>result type</returns>
        public override Type GetResultType(Type elementType, ApplyAggregateClause transformation)
        {
            return typeof(int);
        }

        /// <summary>
        /// Combine temporary results. This is useful when queryable is split due to max page size. 
        /// </summary>
        /// <param name="temporaryResults">The results to combine, as <see cref="{Tuple<object, int}"/> when item1 is the result 
        /// and item2 is the number of elements that produced this temporary result</param>
        /// <returns>The final result</returns>
        public override object CombineTemporaryResults(List<Tuple<object, int>> temporaryResults)
        {
            if (temporaryResults.Count() == 1)
            {
                return temporaryResults[0].Item1;
            }

            var t = temporaryResults[0].Item1.GetType();
            switch (t.Name)
            {
                case "Int32": return temporaryResults.Select(pair => pair.Item1).Sum(o => (int)o);
                case "Int64": return temporaryResults.Select(pair => pair.Item1).Sum(o => (long)o);
                case "Int16": return temporaryResults.Select(pair => pair.Item1).Sum(o => (short)o);
                case "Decimal": return temporaryResults.Select(pair => pair.Item1).Sum(o => (decimal)o);
                case "Double": return temporaryResults.Select(pair => pair.Item1).Sum(o => (double)o);
                case "Float": return temporaryResults.Select(pair => pair.Item1).Sum(o => (float)o);
            }

            throw Error.InvalidOperation("unsupported type");
        }
    }
}
