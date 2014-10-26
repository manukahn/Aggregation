using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Http;
using Microsoft.OData.Core.UriParser.Semantic;

namespace System.Web.OData.OData.Query.Aggregation.AggregationMethods
{
    /// <summary>
    /// Implementation of Max aggregation method
    /// </summary>
    [AggregationMethod("max")]
    public class MaxAggregation : AggregationImplementationBase
    {
        /// <summary>
        /// Implement the Max aggregation method
        /// </summary>
        /// <param name="elementType">The type of entities</param>
        /// <param name="query">The collection</param>
        /// <param name="transformation">The transformation clause created by the parser</param>
        /// <param name="propertyToAggregateExpression">Projection Expression that defines access to the property to aggregate</param>
        /// <returns>The Max result</returns>
        public override object DoAggregatinon(Type elementType, IQueryable query, ApplyAggregateClause transformation, LambdaExpression propertyToAggregateExpression)
        {
            var resultType = this.GetResultType(elementType, transformation);

            var selected = (ExpressionHelpers.QueryableSelect(query, elementType, resultType, propertyToAggregateExpression)).AsQueryable();
            ///call: (selected.AsQueryable() as IQueryable<double>).Max();
            return ExpressionHelpers.Max(resultType, selected);
        }

        /// <summary>
        /// Get the type of the aggregation result
        /// </summary>
        /// <param name="elementType">the entity type</param>
        /// <param name="transformation">The transformation clause created by the parser</param>
        /// <returns>The type of the aggregation result.</returns>
        public override Type GetResultType(Type elementType, ApplyAggregateClause transformation)
        {
            return GetAggregatedPropertyType(elementType, transformation.AggregatableProperty);
        }


        /// <summary>
        /// Combine temporary results. This is useful when queryable is split due to max page size. 
        /// </summary>
        /// <param name="temporaryResults">The results to combine, as <see cref="Tuple{object, int}"/> when item1 is the result 
        /// and item2 is the number of elements that produced this temporary result</param>
        /// <returns>The final result.</returns>
        public override object CombineTemporaryResults(List<Tuple<object, int>> temporaryResults)
        {
            if (temporaryResults.Count() == 1)
                return temporaryResults[0].Item1;
            var t = temporaryResults[0].Item1.GetType();
            switch (t.Name)
            {
                case "Int32": return temporaryResults.Select(pair => pair.Item1).Max(o => (int)o);
                case "Int64": return temporaryResults.Select(pair => pair.Item1).Max(o => (long)o);
                case "Int16": return temporaryResults.Select(pair => pair.Item1).Max(o => (short)o);
                case "Decimal": return temporaryResults.Select(pair => pair.Item1).Max(o => (decimal)o);
                case "Double": return temporaryResults.Select(pair => pair.Item1).Max(o => (double)o);
                case "Float": return temporaryResults.Select(pair => pair.Item1).Max(o => (float)o);
            }

            throw Error.InvalidOperation("unsupported type");
        }
    }
}
