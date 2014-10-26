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

        /// <summary>
        /// RegisterExternalMethods.
        /// </summary>
        public static void Init()
        {
            var handler = new ExternalMethodsHandler();
            handler.RegisterExternalMethods();
        }
    }

    /// <summary>
    /// Mark that a class is an aggregation method implementation
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class AggregationMethodAttribute : Attribute
    {
        public AggregationMethodAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }
    }
}
