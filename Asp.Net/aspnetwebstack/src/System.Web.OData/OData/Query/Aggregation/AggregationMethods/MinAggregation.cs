using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Http;
using Microsoft.OData.Core.UriParser.Semantic;

namespace System.Web.OData.OData.Query.Aggregation.AggregationMethods
{
    /// <summary>
    /// Implementation of Min aggregation method
    /// </summary>
    [AggregationMethod("min")]
    public class MinAggregation : AggregationImplementationBase
    {
        /// <summary>
        /// Implement the Min aggregation method.
        /// </summary>
        /// <param name="elementType">The type of entities</param>
        /// <param name="query">The collection</param>
        /// <param name="transformation">The transformation clause created by the parser</param>
        /// <param name="propertyToAggregateExpression">Projection Expression that defines access to the property to aggregate</param>
        /// <param name="parameters">A list of string parameters sent to the aggregation method</param>
        /// <returns>The Sum result</returns>
        public override object DoAggregatinon(Type elementType, IQueryable collection, ApplyAggregateClause transformation, LambdaExpression propertyToAggregateExpression, params string[] parameters)
        {
            var resultType = this.GetResultType(elementType, transformation);
            var aggregatedProperyType = GetAggregatedPropertyType(elementType, transformation.AggregatableProperty);
            var projectionDelegate = GetProjectionDelegate(elementType, transformation.AggregatableProperty, propertyToAggregateExpression);
            var selectedValues = GetItemsToQuery(elementType, collection, projectionDelegate, aggregatedProperyType);

            //call: (selected.AsQueryable() as IQueryable<double>).Min();
            return ExpressionHelpers.Min(resultType, selectedValues);
        }

        /// <summary>
        /// Get the type of the aggregation result.
        /// </summary>
        /// <param name="elementType">the entity type</param>
        /// <param name="transformation">The transformation clause created by the parser</param>
        /// <returns>The type of the aggregation result.</returns>
        public override Type GetResultType(Type elementType, ApplyAggregateClause transformation)
        {
            return this.GetAggregatedPropertyType(elementType, transformation.AggregatableProperty);
        }

        /// <summary>
        /// Combine temporary results. This is useful when QUERIABLE is split due to max page size. 
        /// </summary>
        /// <param name="temporaryResults">The results to combine, as <see cref="<Tuple<object, int>"/> when item1 is the result 
        /// and item2 is the number of elements that produced this temporary result</param>
        /// <returns>The final result.</returns>
        public override object CombineTemporaryResults(List<Tuple<object, int>> temporaryResults)
        {
            if (temporaryResults.Count() == 1)
            {
                return temporaryResults[0].Item1;
            }

            var t = temporaryResults[0].Item1.GetType();
            switch (t.Name)
            {
                case "Int32": return temporaryResults.Select(pair => pair.Item1).Min(o => (int)o);
                case "Int64": return temporaryResults.Select(pair => pair.Item1).Min(o => (long)o);
                case "Int16": return temporaryResults.Select(pair => pair.Item1).Min(o => (short)o);
                case "Decimal": return temporaryResults.Select(pair => pair.Item1).Min(o => (decimal)o);
                case "Double": return temporaryResults.Select(pair => pair.Item1).Min(o => (double)o);
                case "Float": return temporaryResults.Select(pair => pair.Item1).Min(o => (float)o);
            }

            throw Error.InvalidOperation("unsupported type");
        }
    }
}
