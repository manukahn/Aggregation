using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web.OData.OData.Query.Aggregation.SamplingMethods;
using PepsAggregationLibrary.Projection;
using System.Web;
using SE.OIP.Common.Core.TimeZone;

namespace PepsAggregationLibrary.Stacking
{
    /// <summary>
    /// stack/aggregate at ‘Month’ level (omit the year)
    /// </summary>
    [SamplingMethod("stackByMonth")]
    public class StackingByMonthClasses : TimeGroupingBase
    {
        /// <summary>
        /// Do the sampling
        /// </summary>
        /// <param name="value">The timestamp</param>
        /// <param name="local">use UTC time zone</param>
        /// <param name="factor">use factor of the time unit</param>
        /// <returns>The projected timestamp</returns>
        public static DateTimeOffset DoSampling(DateTimeOffset value, bool local, int factor = 1)
        {
            var timezone = GetTimeZone();
            if (local && timezone != null)
            {
                value = TimeZoneSource.ConvertUtcToLocal(value, timezone);
            }

            int month = value.Month;
            if (factor != 1)
            {
                month = value.Month - 1;
                var reminder = month%factor;
                if (reminder != 0)
                {
                    month = month - reminder;
                }

                month++;
            }

            return new DateTimeOffset(NatualValues.Year, month, value.Day, value.Hour, value.Minute, value.Second, value.Offset);
        }
    }

    /// <summary>
    /// stack/aggregate at ‘Day’ level (omit the year and month)
    /// </summary>
    [SamplingMethod("stackByDay")]
    public class StackingByDayClasses : TimeGroupingBase
    {
        /// <summary>
        /// Do the sampling
        /// </summary>
        /// <param name="value">The timestamp</param>
        /// <param name="local">use UTC time zone</param>
        /// <param name="factor">use factor of the time unit</param>
        /// <returns>The projected timestamp</returns>
        public static DateTimeOffset DoSampling(DateTimeOffset value, bool local, int factor = 1)
        {
            var timezone = GetTimeZone();
            if (local && timezone != null)
            {
                value = TimeZoneSource.ConvertUtcToLocal(value, timezone);
            }

            int day = value.Day;
            if (factor != 1)
            {
                day = value.Day - 1;
                var reminder = day % factor;
                if (reminder != 0)
                {
                    day = day - reminder;
                }

                day++;
            }

            return new DateTimeOffset(NatualValues.Year, NatualValues.Month, day, value.Hour, value.Minute, value.Second, value.Offset);
        }
    }


    /// <summary>
    /// stack/aggregate at ‘Week’ level
    /// </summary>
    [SamplingMethod("stackByDay-MondayBasedWeek")]
    public class StackByDayMondayBasedWeek : TimeGroupingBase
    {
        /// <summary>
        /// Do the sampling
        /// </summary>
        /// <param name="value">The timestamp</param>
        /// <param name="local">use UTC time zone</param>
        /// <param name="factor">use factor of the time unit</param>
        /// <returns>The projected timestamp</returns>
        public static DateTimeOffset DoSampling(DateTimeOffset value, bool local, int factor = 1)
        {
            var timezone = GetTimeZone();
            if (local && timezone != null)
            {
                value = TimeZoneSource.ConvertUtcToLocal(value, timezone);
            }

            return new DateTimeOffset(NatualValues.YearFirstWeekMonday1970, NatualValues.MonthFirstWeekMonday1970, NatualValues.DayOfWeekMonday1970, value.Hour, value.Minute, value.Second, value.Offset);
        }
    }


    /// <summary>
    /// stack/aggregate at ‘Week’ level
    /// </summary>
    [SamplingMethod("stackByDay-SundayBasedWeek")]
    public class StackByDaySundayBasedWeek : TimeGroupingBase
    {
        /// <summary>
        /// Do the sampling
        /// </summary>
        /// <param name="value">The timestamp</param>
        /// <param name="local">use UTC time zone</param>
        /// <param name="factor">use factor of the time unit</param>
        /// <returns>The projected timestamp</returns>
        public static DateTimeOffset DoSampling(DateTimeOffset value, bool local, int factor = 1)
        {
            var timezone = GetTimeZone();
            if (local && timezone != null)
            {
                value = TimeZoneSource.ConvertUtcToLocal(value, timezone);
            }

            return new DateTimeOffset(NatualValues.Year, NatualValues.Month, NatualValues.DayOfWeekSunday1970, value.Hour, value.Minute, value.Second, value.Offset);

        }
    }

    /// <summary>
    /// stack/aggregate at ‘Day’ level (omit the year, month and day)
    /// </summary>
    [SamplingMethod("stackByHour")]
    public class StackingByHourClasses : TimeGroupingBase
    {
        /// <summary>
        /// Do the sampling
        /// </summary>
        /// <param name="value">The timestamp</param>
        /// <param name="local">use UTC time zone</param>
        /// <param name="factor">use factor of the time unit</param>
        /// <returns>The projected timestamp</returns>
        public static DateTimeOffset DoSampling(DateTimeOffset value, bool local, int factor = 1)
        {
            var timezone = GetTimeZone();
            if (local && timezone != null)
            {
                value = TimeZoneSource.ConvertUtcToLocal(value, timezone);
            }

            var hour = value.Hour;
            if (factor != 1)
            {
                var reminder = hour%factor;
                if (reminder != 0)
                {
                    hour = hour - reminder;
                }
            }

            return new DateTimeOffset(NatualValues.Year, NatualValues.Month, NatualValues.DayOfMonth, hour, value.Minute, value.Second, value.Offset);
        }
    }

    /// <summary>
    /// stack/aggregate at ‘Hour’ level (omit the year, month, day and hour)
    /// </summary>
    [SamplingMethod("stackByMinute")]
    public class StackingByMinuteClasses : TimeGroupingBase
    {
        /// <summary>
        /// Do the sampling
        /// </summary>
        /// <param name="value">The timestamp</param>
        /// <param name="local">use UTC time zone</param>
        /// <param name="factor">use factor of the time unit</param>
        /// <returns>The projected timestamp</returns>
        public static DateTimeOffset DoSampling(DateTimeOffset value, bool local, int factor = 1)
        {
            var timezone = GetTimeZone();
            if (local && timezone != null)
            {
                value = TimeZoneSource.ConvertUtcToLocal(value, timezone);
            }

            var minute = value.Minute;
            if (factor != 1)
            {
                var reminder = minute%factor;
                if (reminder != 0)
                {
                    minute = minute - reminder;
                }
            }

            return new DateTimeOffset(NatualValues.Year, NatualValues.Month, NatualValues.DayOfMonth, NatualValues.Hour, minute, value.Second, value.Offset);
        }
    }

    /// <summary>
    /// stack/aggregate at ‘Second’ level (omit the year, month, day, hour and minute)
    /// </summary>
    [SamplingMethod("stackBySecond")]
    public class StackingBySecondClasses : TimeGroupingBase
    {
        /// <summary>
        /// Do the sampling
        /// </summary>
        /// <param name="value">The timestamp</param>
        /// <param name="local">use UTC time zone</param>
        /// <param name="factor">use factor of the time unit</param>
        /// <returns>The projected timestamp</returns>
        public static DateTimeOffset DoSampling(DateTimeOffset value, bool local, int factor = 1)
        {
            var timezone = GetTimeZone();
            if (local && timezone != null)
            {
                value = TimeZoneSource.ConvertUtcToLocal(value, timezone);
            }

            var second = value.Second;
            if (factor != 1)
            {
                var reminder = second % factor;
                if (reminder != 0)
                {
                    second = second - reminder;
                }
            }

            return new DateTimeOffset(NatualValues.Year, NatualValues.Month, NatualValues.DayOfMonth, NatualValues.Hour, NatualValues.Minute, second, value.Offset);
        }
    }

}
