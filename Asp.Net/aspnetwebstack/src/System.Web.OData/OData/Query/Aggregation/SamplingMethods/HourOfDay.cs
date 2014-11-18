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
    [SamplingMethod("hourofday")]
    public class HourOfDaySampling : SamplingImplementationBase
    {
        private static Dictionary<string, TimeZoneInfo> timezones;

        static HourOfDaySampling()
        {
            timezones = new Dictionary<string, TimeZoneInfo>();
            foreach (var systemTimeZone in TimeZoneInfo.GetSystemTimeZones())
            {
                var segments = systemTimeZone.Id.Split(' ');
                StringBuilder id = new StringBuilder();
                foreach (var segment in segments)
                {
                    id.Append(segment);
                }
                timezones[id.ToString()] = TimeZoneInfo.FindSystemTimeZoneById(systemTimeZone.Id);
            }
        }

        /// <summary>
        /// The actual Implementation method must be static.
        /// </summary>
        /// <param name="date">The date to analyze.</param>
        /// <returns>The day in the week.</returns>
        public static int GetHourInDay(DateTimeOffset date, string timezoneStr)
        {
            var timezone = HourOfDaySampling.FindTimeZone(timezoneStr);
            var converted = TimeZoneInfo.ConvertTime(date, timezone);
            return converted.TimeOfDay.Hours;
        }

        /// <summary>
        /// Get the actual implementation method 
        /// </summary>
        /// <param name="genericType">The entity type</param>
        /// <returns>The <see cref="MethodInfo"/> of the actual implementation method</returns>
        public override MethodInfo GetSamplingProcessingMethod(Type genericType)
        {
            return this.GetType().GetMethod("GetHourInDay");
        }

        /// <summary>
        /// The type of the result
        /// </summary>
        /// <param name="inputType">The entity type</param>
        /// <returns>The type of the result</returns>
        public override Type GetResultType(Type inputType)
        {
            return typeof(int);
        }

        private static TimeZoneInfo FindTimeZone(string timezoneString)
        {
            var res = TimeZoneInfo.Utc;
            if (!string.IsNullOrEmpty(timezoneString) && !string.IsNullOrWhiteSpace(timezoneString))
            {
                if (timezones.ContainsKey(timezoneString))
                {
                    res = timezones[timezoneString];
                }
            }

            return res;
        }
    }
}
