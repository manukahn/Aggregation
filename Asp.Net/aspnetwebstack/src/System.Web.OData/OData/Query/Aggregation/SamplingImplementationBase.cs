using System.Reflection;

namespace System.Web.OData.OData.Query.Aggregation
{
    /// <summary>
    /// A base class for all Sampling methods implementations
    /// </summary>
    public abstract class SamplingImplementationBase : ApplyImplementationBase
    {
        /// <summary>
        /// Get the method info of sampling processing function.
        /// </summary>
        /// <param name="genericType">Generic type for the method.</param>
        /// <returns>The <see cref="MethodInfo"/> of the actual implementation method</returns>
        public abstract MethodInfo GetSamplingProcessingMethod(Type genericType);

        /// <summary>
        /// Determines the type that is returned from the sampling method. 
        /// </summary>
        /// <param name="inputType">The element type on which the sampling method operates.</param>
        /// <returns>The type that is returned from the sampling method.</returns>
        public abstract Type GetResultType(Type inputType);
    }
}
