using System;
using System.Web.OData.OData.Query.Aggregation.SamplingMethods;
using SE.OIP.Common.Core.TimeZone;

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

            return new DateTimeOffset(value.Year, value.Month, value.Day, value.Hour, value.Minute, second, value.Offset);
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
                var reminder = minute % factor;
                if (reminder != 0)
                {
                    minute = minute - reminder;
                }
            }

            return new DateTimeOffset(value.Year, value.Month, value.Day, value.Hour, minute, NatualValues.Second, value.Offset);
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
                var reminder = hour % factor;
                if (reminder != 0)
                {
                    hour = hour - reminder;
                }
            }

            return new DateTimeOffset(value.Year, value.Month, value.Day, hour, NatualValues.Minute, NatualValues.Second, value.Offset);
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
            
            return new DateTimeOffset(value.Year, value.Month, day, NatualValues.Hour, NatualValues.Minute, NatualValues.Second, value.Offset);
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
                var reminder = month % factor;
                if (reminder != 0)
                {
                    month = month - reminder;
                }
                month++;
            }
            
            return new DateTimeOffset(value.Year, month, NatualValues.DayOfMonth, NatualValues.Hour, NatualValues.Minute, NatualValues.Second, value.Offset);
        }
    }


    /// <summary>
    /// project/aggregate at Quarter level
    /// </summary>
    [SamplingMethod("projectByQuarter")]
    public class ProjectByQuarter : TimeGroupingBase
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
            return ProjectByMonth.DoSampling(value, local, 3);
        }
    }


    /// <summary>
    /// project/aggregate at HalfYear level
    /// </summary>
    [SamplingMethod("projectByHalfYear")]
    public class ProjectByHalfYear : TimeGroupingBase
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
            return ProjectByMonth.DoSampling(value, local, 6);
        }
    }


    /// <summary>
    /// project/aggregate at ‘Week’ level
    /// </summary>
    [SamplingMethod("projectByDay-MondayBasedWeek")]
    public class ProjectByDayMondayBasedWeek : TimeGroupingBase
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
            return new DateTimeOffset(value.Year, value.Month, value.Day, NatualValues.Hour, NatualValues.Minute, NatualValues.Second, value.Offset);
        }
    }


    /// <summary>
    /// project/aggregate at ‘Week’ level
    /// </summary>
    [SamplingMethod("projectByDay-SundayBasedWeek")]
    public class ProjectByDaySundayBasedWeek : TimeGroupingBase
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
            return new DateTimeOffset(value.Year, value.Month, value.Day, NatualValues.Hour, NatualValues.Minute, NatualValues.Second, value.Offset);
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

            var year = value.Year;
            if (factor != 1)
            {
                var reminder = year%factor;
                if (reminder != 0)
                {
                    year = year - reminder;
                }
            }

            return new DateTimeOffset(year, NatualValues.Month, NatualValues.DayOfMonth, NatualValues.Hour, NatualValues.Minute, NatualValues.Second, value.Offset);
        }
    }


    /// <summary>
    /// project/aggregate at ‘Global’ level
    /// </summary>
    [SamplingMethod("projectGlobal")]
    public class ProjectGlobal : TimeGroupingBase
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
            return new DateTimeOffset(NatualValues.Year, NatualValues.Month, NatualValues.DayOfMonth, NatualValues.Hour, NatualValues.Minute, NatualValues.Second, TimeSpan.FromHours(0));
        }
    }



}
