using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace System.Web.OData.OData.Query.Aggregation.SamplingMethods
{
    /// <summary>
    /// Implementation of round sampling method
    /// </summary>
    [SamplingMethod("round")]
    public class RoundSampling : SamplingImplementationBase
    {
        /// <summary>
        /// The actual Implementation method must be static.
        /// </summary>
        /// <param name="value">The data to analyze.</param>
        /// <returns>The result.</returns>
        public static int Round(double value)
        {
            return (int)Math.Round(value);
        }

        /// <summary>
        /// Get the actual implementation method. 
        /// </summary>
        /// <param name="genericType">The entity type.</param>
        /// <returns>The <see cref="MethodInfo"/> of the actual implementation method.</returns>
        public override MethodInfo GetSamplingProcessingMethod(Type genericType)
        {
            return this.GetType().GetMethods().First(mi => mi.GetParameters().First().ParameterType == genericType);
        }

        /// <summary>
        /// The type of the result.
        /// </summary>
        /// <param name="inputType">The entity type.</param>
        /// <returns>The type of the result.</returns>
        public override Type GetResultType(Type inputType)
        {
            return typeof(int);
        }
    }
}
