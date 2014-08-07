using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace System.Web.OData.OData.Query.Aggregation.SamplingMethods
{
    public class RoundSampling : SamplingImplementationBase
    {
        /// <summary>
        /// Implementation method must be static
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int Round(double value)
        {
            return (int)value;
        }

        public override MethodInfo GetSamplingProcessingMethod(Type genericType)
        {
            return this.GetType().GetMethods().First(mi => mi.GetParameters().First().ParameterType == genericType);
        }

        public override Type GetResultType(Type inputType)
        {
            return typeof(int);
        }
    }
}
