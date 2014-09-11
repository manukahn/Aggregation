using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Web.Http;
using Microsoft.OData.Core.UriParser.Semantic;

namespace System.Web.OData.OData.Query.Aggregation.AggregationMethods
{
    /// <summary>
    /// Implementation of Sum aggregation method
    /// </summary>
    [AggregationMethod("sum")]
    public class SumAggregation : AggregationImplementationBase
    {
        /// <summary>
        /// Implement the Sum aggregation method
        /// </summary>
        /// <param name="elementType">The type of entities</param>
        /// <param name="query">The collection</param>
        /// <param name="transformation">The transformation clause created by the parser</param>
        /// <param name="propertyToAggregateExpression">Projection Expression that defines access to the property to aggregate</param>
        /// <returns>The Sum result</returns>
        public override object DoAggregatinon(Type elementType, IQueryable query, ApplyAggregateClause transformation, LambdaExpression propertyToAggregateExpression)
        {
            var resultType = GetResultType(elementType, transformation);
            return ExpressionHelpers.SelectAndSum(query, elementType, resultType, propertyToAggregateExpression);
        }

        /// <summary>
        /// Get the type of the aggregation result
        /// </summary>
        /// <param name="elementType">the entity type</param>
        /// <param name="transformation">The transformation clause created by the parser</param>
        /// <returns>The type of the aggregation result</returns>
        public override Type GetResultType(Type elementType, ApplyAggregateClause transformation)
        {
            return GetAggregatedPropertyType(elementType, transformation.AggregatableProperty);
        }

        /// <summary>
        /// Combine temporary results. This is useful when queryable is split due to max page size. 
        /// </summary>
        /// <param name="temporaryResults">The results to combine, as <see cref="<Tuple<object, int>"/> when item1 is the result 
        /// and item2 is the number of elements that produced this temporary result</param>
        /// <returns>The final result</returns>
        public override object CombineTemporaryResults(List<Tuple<object, int>> temporaryResults)
        {
            if (temporaryResults.Count() == 1)
                return temporaryResults[0].Item1;
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
