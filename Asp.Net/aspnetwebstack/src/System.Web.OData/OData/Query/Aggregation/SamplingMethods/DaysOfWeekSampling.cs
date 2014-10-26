using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System.Web.OData.OData.Query.Aggregation.SamplingMethods
{
    /// <summary>
    /// Implementation of daysofweek sampling method
    /// </summary>
    [SamplingMethod("dayofweek")]
    public class DayOfWeekSampling : SamplingImplementationBase
    {
        /// <summary>
        /// The actual Implementation method must be static.
        /// </summary>
        /// <param name="date">The date to analyze.</param>
        /// <returns>The day in the week.</returns>
        public static string GetDayOfWeek(DateTimeOffset date)
        {
            return date.DayOfWeek.ToString();
        }

        /// <summary>
        /// Get the actual implementation method 
        /// </summary>
        /// <param name="genericType">The entity type</param>
        /// <returns>The <see cref="MethodInfo"/> of the actual implementation method</returns>
        public override MethodInfo GetSamplingProcessingMethod(Type genericType)
        {
            return this.GetType().GetMethod("GetDayOfWeek");
        }

        /// <summary>
        /// The type of the result
        /// </summary>
        /// <param name="inputType">The entity type</param>
        /// <returns>The type of the result</returns>
        public override Type GetResultType(Type inputType)
        {
            return typeof(string);
        }
    }
}
