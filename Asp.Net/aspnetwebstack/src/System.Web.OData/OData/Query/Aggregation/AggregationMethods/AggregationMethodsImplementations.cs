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
            AggregationMethodsImplementations.RegisterAggregationImplementation("sumpower", new SumPowerAggregation());
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
        /// <summary>
        /// Initializes a new instance of the <see cref="AggregationMethodAttribute"/> class.
        /// </summary>
        /// <param name="name">The name of the sampling method as it will appear in the query.</param>
        public AggregationMethodAttribute(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// Gets the name of the sampling method as it will appear in the query.
        /// </summary>
        public string Name { get; private set; }
    }
}
