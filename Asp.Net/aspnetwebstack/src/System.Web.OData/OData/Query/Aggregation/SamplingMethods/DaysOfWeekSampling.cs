using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System.Web.OData.OData.Query.Aggregation.SamplingMethods
{
    [SamplingMethod("dayofweek")]
    public class DayOfWeekSampling : SamplingImplementationBase
    {
        /// <summary>
        /// Implementation method must be static
        /// </summary>
        public static string GetDayOfWeek(DateTimeOffset date)
        {
            return date.DayOfWeek.ToString();
        }

        public override MethodInfo GetSamplingProcessingMethod(Type genericType)
        {
            return this.GetType().GetMethod("GetDayOfWeek");
        }

        public override Type GetResultType(Type inputType)
        {
            return typeof(string);
        }
    }
}
