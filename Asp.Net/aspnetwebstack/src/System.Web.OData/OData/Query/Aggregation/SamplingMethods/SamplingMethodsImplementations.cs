using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Web.OData.OData.Query.Aggregation.SamplingMethods
{
    /// <summary>
    /// Container of sampling methods implementations
    /// computation on samplings is applied in a "with {samplingMethodName} as {alias}" statements. 
    /// </summary>
    public class SamplingMethodsImplementations : AggregationImplementations<SamplingImplementationBase>
    {
        static SamplingMethodsImplementations()
        {
            AggregationImplementations<SamplingImplementationBase>.RegisterAggregationImplementation("dayofweek", new DayOfWeekSampling());
            AggregationImplementations<SamplingImplementationBase>.RegisterAggregationImplementation("round", new RoundSampling());
        }
        
        /// <summary>
        /// used to make sure the type is loaded. 
        /// </summary>
        internal static void Init()
        {
        }
    }
}
