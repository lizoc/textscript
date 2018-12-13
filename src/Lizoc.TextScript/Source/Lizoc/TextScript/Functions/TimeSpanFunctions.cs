using System;
using Lizoc.TextScript.Runtime;

namespace Lizoc.TextScript.Functions
{
    /// <summary>
    /// A timespan object represents a time interval.
    /// 
    /// | Name             | Description
    /// |--------------    |-----------------
    /// | `.days`          | Gets the number of days of this interval 
    /// | `.hours`         | Gets the number of hours of this interval
    /// | `.minutes`       | Gets the number of minutes of this interval
    /// | `.seconds`       | Gets the number of seconds of this interval
    /// | `.milliseconds`  | Gets the number of milliseconds of this interval 
    /// | `.total_days`    | Gets the total number of days in fractional part
    /// | `.total_hours`   | Gets the total number of hours in fractional part
    /// | `.total_minutes` | Gets the total number of minutes in fractional part
    /// | `.total_seconds` | Gets the total number of seconds  in fractional part
    /// | `.total_milliseconds` | Gets the total number of milliseconds  in fractional part
    /// </summary>
    /// <seealso cref="ScriptObject" />
    public class TimeSpanFunctions : ScriptObject
    {
        /// <summary>
        /// Returns a `timespan` object that represents a zero interval.
        /// </summary>
        /// <returns>A zero `timespan` object.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ (timespan.zero + timespan.from_days 5).days }}
        /// ```
        /// ```html
        /// 5
        /// ```
        /// </remarks>
        public static TimeSpan Zero => TimeSpan.Zero;

        /// <summary>
        /// Returns a `timespan` object by specifying an interval in `days`.
        /// </summary>
        /// <param name="days">A number that represents days.</param>
        /// <returns>A `timespan` object.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ (timespan.from_days 5).days }}
        /// ```
        /// ```html
        /// 5
        /// ```
        /// </remarks>
        public static TimeSpan FromDays(double days)
        {
            return TimeSpan.FromDays(days);
        }

        /// <summary>
        /// Returns a `timespan` object by specifying an interval in `hours`.
        /// </summary>
        /// <param name="hours">A number that represents hours.</param>
        /// <returns>A `timespan` object.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ (timespan.from_hours 5).hours }}
        /// ```
        /// ```html
        /// 5
        /// ```
        /// </remarks>
        public static TimeSpan FromHours(double hours)
        {
            return TimeSpan.FromHours(hours);
        }

        /// <summary>
        /// Returns a `timespan` object by specifying an interval in `minutes`.
        /// </summary>
        /// <param name="minutes">A number that represents minutes.</param>
        /// <returns>A `timespan` object.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ (timespan.from_minutes 5).minutes }}
        /// ```
        /// ```html
        /// 5
        /// ```
        /// </remarks>
        public static TimeSpan FromMinutes(double minutes)
        {
            return TimeSpan.FromMinutes(minutes);
        }

        /// <summary>
        /// Returns a `timespan` object by specifying an interval in `seconds`.
        /// </summary>
        /// <param name="seconds">A number that represents seconds.</param>
        /// <returns>A `timespan` object.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ (timespan.from_seconds 5).seconds }}
        /// ```
        /// ```html
        /// 5
        /// ```
        /// </remarks>
        public static TimeSpan FromSeconds(double seconds)
        {
            return TimeSpan.FromSeconds(seconds);
        }

        /// <summary>
        /// Returns a `timespan` object by specifying an interval in `milliseconds`.
        /// </summary>
        /// <param name="millis">A number that represents milliseconds.</param>
        /// <returns>A `timespan` object.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ (timespan.from_milliseconds 5).milliseconds }}
        /// ```
        /// ```html
        /// 5
        /// ```
        /// </remarks>
        public static TimeSpan FromMilliseconds(double millis)
        {
            return TimeSpan.FromMilliseconds(millis);
        }

        /// <summary>
        /// Parses the specified input string into a `timespan` object. 
        /// </summary>
        /// <param name="text">A string representation of a timespan.</param>
        /// <returns>A timespan object parsed from the input string.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ "6:12:14" | timespan.parse }}
        /// ```
        /// ```html
        /// 6:12:14
        /// ```
        /// </remarks>
        public static TimeSpan Parse(string text)
        {
            return TimeSpan.Parse(text);
        }
    }
}