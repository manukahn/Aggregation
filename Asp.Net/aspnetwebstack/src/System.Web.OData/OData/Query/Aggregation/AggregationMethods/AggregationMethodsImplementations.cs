using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Web.OData.OData.Query.Aggregation.AggregationMethods
{
    /// <summary>
    /// Container of aggregation methods implementations
    /// Aggregation implementation is applied in a "with {aggregationName} as {alias}" statements. 
    /// </summary>
    public class AggregationMethodsImplementations : AggregationImplementations<AggregationImplementationBase>
    {
        static AggregationMethodsImplementations()
        {
            AggregationMethodsImplementations.RegisterAggregationImplementation("sum", new SumAggregation());
            AggregationMethodsImplementations.RegisterAggregationImplementation("average", new AverageAggregation());
            AggregationMethodsImplementations.RegisterAggregationImplementation("min", new MinAggregation());
            AggregationMethodsImplementations.RegisterAggregationImplementation("max", new MaxAggregation());
            AggregationMethodsImplementations.RegisterAggregationImplementation("countdistinct", new CountDistinctAggregation());
        }

        public static void Init()
        {
            
        }
    }
}
