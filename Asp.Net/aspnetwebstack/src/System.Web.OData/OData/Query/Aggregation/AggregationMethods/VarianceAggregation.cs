using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Web.OData.OData.Query.Aggregation.AggregationMethods
{
    /// <summary>
    /// Implementation of Variance aggregation method
    /// </summary>
    [AggregationMethod("variance")]
    public class VarianceAggregation : AggregationImplementationBase
    {
        public static double Variance(IList<double> values, double mean, int count)
        {
            double variance = 0;
            for (int i = 0; i < count; i++)
            {
                variance += Math.Pow((values[i] - mean), 2);
            }

            return variance / (count);
        }

        /// <summary>
        /// Implement the Variance aggregation method
        /// </summary>
        /// <param name="elementType">The type of entities</param>
        /// <param name="query">The collection</param>
        /// <param name="transformation">The transformation clause created by the parser</param>
        /// <param name="propertyToAggregateExpression">Projection Expression that defines access to the property to aggregate</param>
        /// <param name="paramaters">A list of string parameters sent to the aggregation method</param>
        /// <returns>The Variance result</returns>
        public override object DoAggregatinon(Type elementType, IQueryable collection, Microsoft.OData.Core.UriParser.Semantic.ApplyAggregateClause transformation, Linq.Expressions.LambdaExpression propertyToAggregateExpression, params string[] parameters)
        {
            var selectedValues = GetSelectedValues(elementType, collection, transformation, propertyToAggregateExpression).AllElements<double>();
            return Variance(selectedValues, selectedValues.Average(), selectedValues.Count);
        }

        /// <summary>
        /// The type of the result of Variance aggregation method is: double
        /// </summary>
        /// <param name="elementType">>The type of entities</param>
        /// <param name="transformation">The transformation clause created by the parser</param>
        /// <returns>result type</returns>
        public override Type GetResultType(Type elementType, Microsoft.OData.Core.UriParser.Semantic.ApplyAggregateClause transformation)
        {
            return typeof(double);
        }

        /// <summary>
        /// CombineTemporaryResults is unsupported. Use the aggregationWindowSize parameter to make sure all values are aggregated in one batch.
        /// </summary>
        /// <param name="temporaryResults"></param>
        /// <returns></returns>
        public override object CombineTemporaryResults(List<Tuple<object, int>> temporaryResults)
        {
            if (temporaryResults.Count() == 1)
            {
                return temporaryResults[0].Item1;
            }

            return new NotSupportedException("Cannot combine multiple Variance results");
        }
    }
}
