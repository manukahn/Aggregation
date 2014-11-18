using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.OData.Core.UriParser.Semantic;

namespace System.Web.OData.OData.Query.Aggregation.AggregationMethods
{
    /// <summary>
    /// Implementation of Average aggregation method
    /// </summary>
    [AggregationMethod("sumpower")]
    public class SumPowerAggregation : AggregationImplementationBase
    {

        public static double TotalSqrt(IQueryable input, double pwr)
        {
            return input.AllElements().Cast<double>().Sum(i => Math.Pow(i, pwr));
        }

        /// <summary>
        /// Implement the Average aggregation method
        /// </summary>
        /// <param name="elementType">The type of entities</param>
        /// <param name="query">The collection</param>
        /// <param name="transformation">The transformation clause created by the parser</param>
        /// <param name="propertyToAggregateExpression">Projection Expression that defines access to the property to aggregate</param>
        /// <param name="parematers">A list of string parameters sent to the aggregation method</param>
        /// <returns>The Sum result</returns>
        public override object DoAggregatinon(Type elementType, IQueryable collection, ApplyAggregateClause transformation, LambdaExpression propertyToAggregateExpression, params string[] parameters)
        {
            var pwr = double.Parse(parameters.First());
            var aggregatedProperyType = GetAggregatedPropertyType(elementType, transformation.AggregatableProperty);
            var selectedValues = GetItemsToQuery(elementType, collection, propertyToAggregateExpression, aggregatedProperyType);

            return TotalSqrt(selectedValues, pwr);
        }

        /// <summary>
        /// The type of the result of Average aggregation method is: double
        /// </summary>
        /// <param name="elementType">>The type of entities</param>
        /// <param name="transformation">The transformation clause created by the parser</param>
        /// <returns>result type</returns>
        public override Type GetResultType(Type elementType, ApplyAggregateClause transformation)
        {
            return typeof(double);
        }

        /// <summary>
        /// Combine temporary results. This is useful when QUERIABLE is split due to max page size. 
        /// </summary>
        /// <param name="temporaryResults">The results to combine, as <see cref="Tuple{object, int}"/> when item1 is the result 
        /// and item2 is the number of elements that produced this temporary result</param>
        /// <returns>The final result</returns>
        public override object CombineTemporaryResults(List<Tuple<object, int>> temporaryResults)
        {
            if (temporaryResults.Count() == 1)
            {
                return temporaryResults[0].Item1;
            }

            var t = temporaryResults[0].Item1.GetType();
            var numberOfElemets = temporaryResults.Sum(o => o.Item2);

            switch (t.Name)
            {
                case "Int32": return temporaryResults.Sum(o => (int)o.Item1 * o.Item2) / numberOfElemets;
                case "Int64": return temporaryResults.Sum(o => (long)o.Item1 * o.Item2) / numberOfElemets;
                case "Int16": return temporaryResults.Sum(o => (short)o.Item1 * o.Item2) / numberOfElemets;
                case "Decimal": return temporaryResults.Sum(o => (decimal)o.Item1 * o.Item2) / numberOfElemets;
                case "Double": return temporaryResults.Sum(o => (double)o.Item1 * o.Item2) / numberOfElemets;
                case "Float": return temporaryResults.Sum(o => (float)o.Item1 * o.Item2) / numberOfElemets;
            }

            throw Error.InvalidOperation("unsupported type");
        }
    }
}
