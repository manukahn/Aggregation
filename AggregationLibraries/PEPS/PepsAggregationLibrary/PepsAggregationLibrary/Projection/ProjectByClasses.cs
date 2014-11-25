using System;
using System.Web.OData.OData.Query.Aggregation.SamplingMethods;

namespace PepsAggregationLibrary.Projection
{
    /// <summary>
    /// project/aggregate at ‘Second’ level
    /// </summary>
    [SamplingMethod("projectBySecond")]
    public class ProjectBySecond : TimeGroupingBase
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
            if (utc)
            {
                value = value.Add(-value.Offset);
            }

            var second = value.Minute;
            var reminder = second % factor;
            if (reminder != 0)
            {
                second = second - reminder;
            }

            if (!utc)
            {
                return new DateTimeOffset(value.Year, value.Month, value.Day, value.Hour, value.Minute, second, value.Offset);
            }

            return new DateTimeOffset(value.Year, value.Month, value.Day, value.Hour, value.Minute, second, TimeSpan.FromHours(0));
        }
    }

    /// <summary>
    /// project/aggregate at ‘Minute’ level
    /// </summary>
    [SamplingMethod("projectByMinute")]
    public class ProjectByMinute : TimeGroupingBase
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
            if (utc)
            {
                value = value.Add(-value.Offset);
            }

            var minute = value.Minute;
            var reminder = minute % factor;
            if (reminder != 0)
            {
                minute = minute - reminder;
            }

            if (!utc)
            {
                return new DateTimeOffset(value.Year, value.Month, value.Day, value.Hour, minute, NatualValues.Second, value.Offset);
            }

            return new DateTimeOffset(value.Year, value.Month, value.Day, value.Hour, minute, NatualValues.Second, TimeSpan.FromHours(0));
        }
    }

    /// <summary>
    /// project/aggregate at ‘Hour’ level
    /// </summary>
    [SamplingMethod("projectByHour")]
    public class ProjectByHour : TimeGroupingBase
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
            if (utc)
            {
                value = value.Add(-value.Offset);
            }

            var hour = value.Hour;
            var reminder = hour % factor;
            if (reminder != 0)
            {
                hour = hour - reminder;
            }

            if (!utc)
            {
                return new DateTimeOffset(value.Year, value.Month, value.Day, hour, NatualValues.Minute, NatualValues.Second, value.Offset);
            }

            return new DateTimeOffset(value.Year, value.Month, value.Day, hour, NatualValues.Minute, NatualValues.Second, TimeSpan.FromHours(0));
        }
    }

    /// <summary>
    /// project/aggregate at ‘Day’ level
    /// </summary>
    [SamplingMethod("projectByDay")]
    public class ProjectByDay : TimeGroupingBase
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
            if (utc)
            {
                value = value.Add(-value.Offset);
            }

            var day = value.Day - 1;
            var reminder = day % factor;
            if (reminder != 0)
            {
                day = day - reminder;
            }

            day++;

            if (!utc)
            {
                return new DateTimeOffset(value.Year, value.Month, day, NatualValues.Hour, NatualValues.Minute, NatualValues.Second, value.Offset);
            }

            return new DateTimeOffset(value.Year, value.Month, day, NatualValues.Hour, NatualValues.Minute, NatualValues.Second, TimeSpan.FromHours(0));
        }
    }

    /// <summary>
    /// project/aggregate at ‘Month’ level
    /// </summary>
    [SamplingMethod("projectByMonth")]
    public class ProjectByMonth : TimeGroupingBase
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
            if (utc)
            {
                value = value.Add(-value.Offset);
            }

            var month = value.Month - 1;
            var reminder = month % factor;
            if (reminder != 0)
            {
                month = month - reminder;
            }

            month++;

            if (!utc)
            {
                return new DateTimeOffset(value.Year, month, NatualValues.DayOfMonth, NatualValues.Hour, NatualValues.Minute, NatualValues.Second, value.Offset);
            }

            return new DateTimeOffset(value.Year, value.Month, NatualValues.DayOfMonth, NatualValues.Hour, NatualValues.Minute, NatualValues.Second, TimeSpan.FromHours(0));
        }
    }


    /// <summary>
    /// project/aggregate at ‘Week’ level
    /// </summary>
    [SamplingMethod("projectByWeekMonday")]
    public class ProjectByWeekMonday : TimeGroupingBase
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
            if (utc)
            {
                value = value.Add(-value.Offset);
            }

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
                return new DateTimeOffset(value.Year, value.Month, value.Day, NatualValues.Hour, NatualValues.Minute, NatualValues.Second, value.Offset);
            }

            return new DateTimeOffset(value.Year, value.Month, value.Day, NatualValues.Hour, NatualValues.Minute, NatualValues.Second, TimeSpan.FromHours(0));
        }
    }


    /// <summary>
    /// project/aggregate at ‘Week’ level
    /// </summary>
    [SamplingMethod("projectByWeekSunday")]
    public class ProjectByWeekSunday : TimeGroupingBase
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
            if (utc)
            {
                value = value.Add(-value.Offset);
            }

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
                return new DateTimeOffset(value.Year, value.Month, value.Day, NatualValues.Hour, NatualValues.Minute, NatualValues.Second, value.Offset);
            }

            return new DateTimeOffset(value.Year, value.Month, value.Day, NatualValues.Hour, NatualValues.Minute, NatualValues.Second, TimeSpan.FromHours(0));
        }
    }

    /// <summary>
    /// project/aggregate at ‘Year’ level
    /// </summary>
    [SamplingMethod("projectByYear")]
    public class ProjectByYear : TimeGroupingBase
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
            if (utc)
            {
                value = value.Add(-value.Offset);
            }

            var year = value.Year;
            var reminder = year % factor;
            if (reminder != 0)
            {
                year = year - reminder;
            }

            if (!utc)
            {
                return new DateTimeOffset(year, NatualValues.Month, NatualValues.DayOfMonth, NatualValues.Hour, NatualValues.Minute, NatualValues.Second, value.Offset);
            }

            return new DateTimeOffset(year, NatualValues.Month, NatualValues.DayOfMonth, NatualValues.Hour, NatualValues.Minute, NatualValues.Second, TimeSpan.FromHours(0));
        }
    }



}
