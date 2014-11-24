using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.OData.OData.Query.Aggregation.SamplingMethods;
using PepsAggregationLibrary.Projection;

namespace PepsAggregationLibrary.Stacking
{
    /// <summary>
    /// stack/aggregate at ‘Year’ level
    /// </summary>
    [SamplingMethod("stackByYear")]
    public class StackingByYearClasses : TimeGroupingBase
    {
        /// <summary>
        /// Do the sampling
        /// </summary>
        /// <param name="value">The timestamp</param>
        /// <param name="utc">use UTC time zone</param>
        /// <param name="factor">use factor of the time unit</param>
        /// <returns>The projected timestamp</returns>
        public static DateTimeOffset DoSampling(DateTimeOffset value, bool utc, int factor = 1)
        {
            var month = value.Month - 1;
            var reminder = month % factor;
            if (reminder != 0)
            {
                month = month - reminder;
            }

            month++;

            if (!utc)
            {
                return new DateTimeOffset(NatualValues.Year, month, value.Day, value.Hour, value.Minute, value.Second, value.Offset);
            }

            return new DateTimeOffset(NatualValues.Year, month, value.Day, value.Hour, value.Minute, value.Second, TimeSpan.FromHours(0));
        }
    }

    /// <summary>
    /// stack/aggregate at ‘Month’ level
    /// </summary>
    [SamplingMethod("stackByMonth")]
    public class StackingByMonthClasses : TimeGroupingBase
    {
        /// <summary>
        /// Do the sampling
        /// </summary>
        /// <param name="value">The timestamp</param>
        /// <param name="utc">use UTC time zone</param>
        /// <param name="factor">use factor of the time unit</param>
        /// <returns>The projected timestamp</returns>
        public static DateTimeOffset DoSampling(DateTimeOffset value, bool utc, int factor = 1)
        {
            var day = value.Day - 1;
            var reminder = day % factor;
            if (reminder != 0)
            {
                day = day - reminder;
            }

            day++;

            if (!utc)
            {
                return new DateTimeOffset(NatualValues.Year, NatualValues.Month, day, value.Hour, value.Minute, value.Second, value.Offset);
            }

            return new DateTimeOffset(NatualValues.Year, NatualValues.Month, day, value.Hour, value.Minute, value.Second, TimeSpan.FromHours(0));
        }
    }


    /// <summary>
    /// stack/aggregate at ‘Week’ level
    /// </summary>
    [SamplingMethod("stackByWeekMonday")]
    public class StackByWeekMonday : TimeGroupingBase
    {
        /// <summary>
        /// Do the sampling
        /// </summary>
        /// <param name="value">The timestamp</param>
        /// <param name="utc">use UTC time zone</param>
        /// <param name="factor">use factor of the time unit</param>
        /// <returns>The projected timestamp</returns>
        public static DateTimeOffset DoSampling(DateTimeOffset value, bool utc, int factor = 1)
        {
            double offset = 0;
            var dayOfweek = value.DayOfWeek;
            switch (dayOfweek)
            {
                case DayOfWeek.Sunday:
                    offset = -(6 + ((factor - 1) * 7));
                    break;
                case DayOfWeek.Monday:
                    offset = -(0 + ((factor - 1) * 7));
                    break;
                case DayOfWeek.Tuesday:
                    offset = -(1 + ((factor - 1) * 7));
                    break;
                case DayOfWeek.Wednesday:
                    offset = -(2 + ((factor - 1) * 7));
                    break;
                case DayOfWeek.Thursday:
                    offset = -(3 + ((factor - 1) * 7));
                    break;
                case DayOfWeek.Friday:
                    offset = -(4 + ((factor - 1) * 7));
                    break;
                case DayOfWeek.Saturday:
                    offset = -(5 + ((factor - 1) * 7));
                    break;
            }

            value = value.AddDays(offset);

            if (!utc)
            {
                return new DateTimeOffset(NatualValues.Year, NatualValues.Month, value.Day, value.Hour, value.Minute, value.Second, value.Offset);
            }

            return new DateTimeOffset(NatualValues.Year, NatualValues.Month, value.Day, value.Hour, value.Minute, value.Second, TimeSpan.FromHours(0));
        }
    }


    /// <summary>
    /// stack/aggregate at ‘Week’ level
    /// </summary>
    [SamplingMethod("stackByWeekSunday")]
    public class StackByWeekSunday : TimeGroupingBase
    {
        /// <summary>
        /// Do the sampling
        /// </summary>
        /// <param name="value">The timestamp</param>
        /// <param name="utc">use UTC time zone</param>
        /// <param name="factor">use factor of the time unit</param>
        /// <returns>The projected timestamp</returns>
        public static DateTimeOffset DoSampling(DateTimeOffset value, bool utc, int factor = 1)
        {
            double offset = 0;
            var dayOfweek = value.DayOfWeek;
            switch (dayOfweek)
            {
                case DayOfWeek.Sunday:
                    offset = -(0 + ((factor - 1) * 7));
                    break;
                case DayOfWeek.Monday:
                    offset = -(1 + ((factor - 1) * 7));
                    break;
                case DayOfWeek.Tuesday:
                    offset = -(2 + ((factor - 1) * 7));
                    break;
                case DayOfWeek.Wednesday:
                    offset = -(3 + ((factor - 1) * 7));
                    break;
                case DayOfWeek.Thursday:
                    offset = -(4 + ((factor - 1) * 7));
                    break;
                case DayOfWeek.Friday:
                    offset = -(5 + ((factor - 1) * 7));
                    break;
                case DayOfWeek.Saturday:
                    offset = -(6 + ((factor - 1) * 7));
                    break;
            }

            value = value.AddDays(offset);

            if (!utc)
            {
                return new DateTimeOffset(NatualValues.Year, NatualValues.Month, value.Day, value.Hour, value.Minute, value.Second, value.Offset);
            }

            return new DateTimeOffset(NatualValues.Year, NatualValues.Month, value.Day, value.Hour, value.Minute, value.Second, TimeSpan.FromHours(0));
        }
    }

    /// <summary>
    /// stack/aggregate at ‘Day’ level
    /// </summary>
    [SamplingMethod("stackByDay")]
    public class StackingByDayClasses : TimeGroupingBase
    {
        /// <summary>
        /// Do the sampling
        /// </summary>
        /// <param name="value">The timestamp</param>
        /// <param name="utc">use UTC time zone</param>
        /// <param name="factor">use factor of the time unit</param>
        /// <returns>The projected timestamp</returns>
        public static DateTimeOffset DoSampling(DateTimeOffset value, bool utc, int factor = 1)
        {
            var hour = value.Hour;
            var reminder = hour % factor;
            if (reminder != 0)
            {
                hour = hour - reminder;
            }

            if (!utc)
            {
                return new DateTimeOffset(NatualValues.Year, NatualValues.Month, NatualValues.DayOfMonth, hour, value.Minute, value.Second, value.Offset);
            }

            return new DateTimeOffset(NatualValues.Year, NatualValues.Month, NatualValues.DayOfMonth, hour, value.Minute, value.Second, TimeSpan.FromHours(0));
        }
    }

    /// <summary>
    /// stack/aggregate at ‘Hour’ level
    /// </summary>
    [SamplingMethod("stackByHour")]
    public class StackingByHourClasses : TimeGroupingBase
    {
        /// <summary>
        /// Do the sampling
        /// </summary>
        /// <param name="value">The timestamp</param>
        /// <param name="utc">use UTC time zone</param>
        /// <param name="factor">use factor of the time unit</param>
        /// <returns>The projected timestamp</returns>
        public static DateTimeOffset DoSampling(DateTimeOffset value, bool utc, int factor = 1)
        {
            var minute = value.Minute;
            var reminder = minute % factor;
            if (reminder != 0)
            {
                minute = minute - reminder;
            }

            if (!utc)
            {
                return new DateTimeOffset(NatualValues.Year, NatualValues.Month, NatualValues.DayOfMonth, NatualValues.Hour, minute, value.Second, value.Offset);
            }

            return new DateTimeOffset(NatualValues.Year, NatualValues.Month, NatualValues.DayOfMonth, NatualValues.Hour, minute, value.Second, TimeSpan.FromHours(0));
        }
    }

    /// <summary>
    /// stack/aggregate at ‘Minute’ level
    /// </summary>
    [SamplingMethod("stackByMinute")]
    public class StackingByMinuteClasses : TimeGroupingBase
    {
        /// <summary>
        /// Do the sampling
        /// </summary>
        /// <param name="value">The timestamp</param>
        /// <param name="utc">use UTC time zone</param>
        /// <param name="factor">use factor of the time unit</param>
        /// <returns>The projected timestamp</returns>
        public static DateTimeOffset DoSampling(DateTimeOffset value, bool utc, int factor = 1)
        {
            var second = value.Minute;
            var reminder = second % factor;
            if (reminder != 0)
            {
                second = second - reminder;
            }

            if (!utc)
            {
                return new DateTimeOffset(NatualValues.Year, NatualValues.Month, NatualValues.DayOfMonth, NatualValues.Hour, NatualValues.Minute, second, value.Offset);
            }

            return new DateTimeOffset(NatualValues.Year, NatualValues.Month, NatualValues.DayOfMonth, NatualValues.Hour, NatualValues.Minute, second, TimeSpan.FromHours(0));
        }
    }

}
