using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.OData.Formatter;
using System.Web.OData.OData.Query.Aggregation.AggregationMethods;
using System.Web.OData.OData.Query.Aggregation.SamplingMethods;
using Microsoft.Data.Edm.Library;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Edm;
using EdmTypeKind = Microsoft.OData.Edm.EdmTypeKind;


namespace System.Web.OData.OData.Query.Aggregation
{
    /// <summary>
    /// Container of aggregation implementations
    /// Aggregation implementation is applied in a "with {aggregationName} as {alias}" statements. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AggregationImplementations<T> where T : ApplyImplementationBase
    {
        private static Dictionary<string, T> customAggregations = new Dictionary<string, T>();

        static AggregationImplementations()
        {
            AggregationMethodsImplementations.Init();
            SamplingMethodsImplementations.Init();
        }

        /// <summary>
        /// Get a particular Aggregation Implementation
        /// </summary>
        /// <param name="aggregationFunction">The name of the aggregation method</param>
        /// <returns>The implementation class of the aggregation method</returns>
        public static T GetAggregationImplementation(string aggregationFunction)
        {
            T result;
            if (customAggregations.TryGetValue(aggregationFunction.ToLower(), out result))
            {
                return result;
            }

            throw Error.NotSupported("Aggregation method not supported", aggregationFunction);
        }

        /// <summary>
        /// Register a custom implementation for a custom aggregation method
        /// </summary>
        /// <param name="aggregationFunctionName">The name of the aggregation method</param>
        /// <param name="implementation">The implementation class of the aggregation method</param>
        public static void RegisterAggregationImplementation(string aggregationFunctionName, T implementation)
        {
            if (implementation != null)
            {
                customAggregations[aggregationFunctionName] = implementation;
            }
        }
        
        /// <summary>
        /// Unregister a custom implementation for a custom aggregation method
        /// </summary>
        /// <param name="aggregationFunctionName">The name of the aggregation method</param>
        public static void UnregisterAggregationImplementation(string aggregationFunctionName)
        {
            if (customAggregations.ContainsKey(aggregationFunctionName))
            {
                customAggregations.Remove(aggregationFunctionName);
            }
        }
    }
}
