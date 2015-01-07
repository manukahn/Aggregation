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
                var dt = TimeZoneSource.ConvertUtcToLocal(value, timezone);
                value = new DateTimeOffset(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, default(TimeSpan));
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
                var dt = TimeZoneSource.ConvertUtcToLocal(value, timezone);
                value = new DateTimeOffset(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, default(TimeSpan));
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
                var dt = TimeZoneSource.ConvertUtcToLocal(value, timezone);
                value = new DateTimeOffset(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, default(TimeSpan));
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
                var dt = TimeZoneSource.ConvertUtcToLocal(value, timezone);
                value = new DateTimeOffset(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, default(TimeSpan));
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
                var dt = TimeZoneSource.ConvertUtcToLocal(value, timezone);
                value = new DateTimeOffset(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, default(TimeSpan));
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
    [SamplingMethod("projectByDayOfWeek")]
    public class ProjectByDayOfWeek : TimeGroupingBase
    {
        /// <summary>
        /// Do the sampling
        /// </summary>
        /// <param name="value">The timestamp</param>
        /// <param name="local">use UTC time zone</param>
        /// <param name="firstDay">The first day of the week</param>
        /// <param name="factor">use factor of the time unit</param>
        /// <returns>The projected timestamp</returns>
        public static DateTimeOffset DoSampling(DateTimeOffset value, bool local, string firstDay, int factor = 1)
        {
            var timezone = GetTimeZone();
            if (local && timezone != null)
            {
                var dt = TimeZoneSource.ConvertUtcToLocal(value, timezone);
                value = new DateTimeOffset(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, default(TimeSpan));
            }

            var offset = ProjectByDayOfWeek.GetOffset(value.DayOfWeek, firstDay, factor);
            value = value.AddDays(offset);
            
            return new DateTimeOffset(value.Year, value.Month, value.Day, NatualValues.Hour, NatualValues.Minute, NatualValues.Second, value.Offset);
        }

        private static double GetOffset(DayOfWeek dayOfweek, string firstDay, int factor)
        {
            double offset = 0;

            if (firstDay.ToLower() == "monday")
            {
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
            }
            else if (firstDay.ToLower() == "saturday")
            {
                switch (dayOfweek)
                {
                    case DayOfWeek.Sunday:
                        offset = -(1 + ((factor - 1) * 7));
                        break;
                    case DayOfWeek.Monday:
                        offset = -(2 + ((factor - 1) * 7));
                        break;
                    case DayOfWeek.Tuesday:
                        offset = -(3 + ((factor - 1) * 7));
                        break;
                    case DayOfWeek.Wednesday:
                        offset = -(4 + ((factor - 1) * 7));
                        break;
                    case DayOfWeek.Thursday:
                        offset = -(5 + ((factor - 1) * 7));
                        break;
                    case DayOfWeek.Friday:
                        offset = -(6 + ((factor - 1) * 7));
                        break;
                    case DayOfWeek.Saturday:
                        offset = -(0 + ((factor - 1) * 7));
                        break;
                }
            }
            else 
            {
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
            }

            return offset;
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
                var dt = TimeZoneSource.ConvertUtcToLocal(value, timezone);
                value = new DateTimeOffset(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, default(TimeSpan));
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
        /// <returns>The projected timestamp</returns>
        public static DateTimeOffset DoSampling(DateTimeOffset value, bool local)
        {
            return new DateTimeOffset(NatualValues.Year, NatualValues.Month, NatualValues.DayOfMonth, NatualValues.Hour, NatualValues.Minute, NatualValues.Second, TimeSpan.FromHours(0));
        }
    }



}
