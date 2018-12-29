// -----------------------------------------------------------------------
// <copyright file="DateTimeFunctions.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
//     All or part thereof may be subject to other licenses documented below this header and 
//     the THIRD-PARTY-LICENSE file in the repository root directory.
// </copyright>
// -----------------------------------------------------------------------

// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Lizoc.TextScript.Runtime;
using Lizoc.TextScript.Syntax;

namespace Lizoc.TextScript.Functions
{
    /// <summary>
    /// A `datetime` object represents an instant in time, expressed as a date and time of day. 
    /// 
    /// | Name             | Description
    /// |--------------    |-----------------
    /// | `.year`          | Gets the year of a date object 
    /// | `.month`         | Gets the month of a date object
    /// | `.day`           | Gets the day in the month of a date object
    /// | `.day_of_year`   | Gets the day within the year
    /// | `.hour`          | Gets the hour of the date object
    /// | `.minute`        | Gets the minute of the date object
    /// | `.second`        | Gets the second of the date object
    /// | `.millisecond`   | Gets the millisecond of the date object
    /// 
    /// [:top:](#builtins)
    ///
    /// #### Binary operations
    /// The substract operation `date1 - date2`: Substract `date2` from `date1` and return a `timespan` object (see `timespan` object below).
    /// 
    /// Other comparison operators(`==`, `!=`, `&lt;=`, `&gt;=`, `&lt;`, `&gt;`) are also working with `datetime` objects.
    /// </summary>
    /// <seealso cref="ScriptObject" />
    public class DateTimeFunctions : ScriptObject, IScriptCustomFunction
    {
        private const string FormatKey = "format";

        // Code from DotLiquid https://github.com/dotliquid/dotliquid/blob/master/src/DotLiquid/Util/StrFTime.cs
        // Apache License, Version 2.0
        private static readonly Dictionary<char, Func<DateTime, CultureInfo, string>> Formats = new Dictionary<char, Func<DateTime, CultureInfo, string>>
        {
            // Thu
            { 'a', (dateTime, cultureInfo) => dateTime.ToString("ddd", cultureInfo) },
            // Thursday
            { 'A', (dateTime, cultureInfo) => dateTime.ToString("dddd", cultureInfo) },
            // Sep
            { 'b', (dateTime, cultureInfo) => dateTime.ToString("MMM", cultureInfo) },
            // September
            { 'B', (dateTime, cultureInfo) => dateTime.ToString("MMMM", cultureInfo) },
            // Full datetime => Sun Dec 09 14:34:20 2018
            { 'c', (dateTime, cultureInfo) => dateTime.ToString("ddd MMM dd HH:mm:ss yyyy", cultureInfo) },
            // Century => 21
            { 'C', (dateTime, cultureInfo) => (dateTime.Year / 100).ToString("D2", cultureInfo) },
            // Day of month => 03
            { 'd', (dateTime, cultureInfo) => dateTime.ToString("dd", cultureInfo) },
            // month/day/year => 09/03/18
            { 'D', (dateTime, cultureInfo) => dateTime.ToString("MM/dd/yy", cultureInfo) },
            // Day of month no pad => 3
            { 'e', (dateTime, cultureInfo) => dateTime.ToString("%d", cultureInfo).PadLeft(2, ' ') },
            // iso8601 yyyy-mm-dd => 2018-09-12
            { 'F', (dateTime, cultureInfo) => dateTime.ToString("yyyy-MM-dd", cultureInfo) },
            // Same as %b
            { 'h', (dateTime, cultureInfo) => dateTime.ToString("MMM", cultureInfo) },
            // 24 hour of day => 22
            { 'H', (dateTime, cultureInfo) => dateTime.ToString("HH", cultureInfo) },
            // 12 hour of day => 10
            { 'I', (dateTime, cultureInfo) => dateTime.ToString("hh", cultureInfo) },
            // day of year => 033
            { 'j', (dateTime, cultureInfo) => dateTime.DayOfYear.ToString("D3", cultureInfo) },
            // 24 hour of day no pad => 02
            { 'k', (dateTime, cultureInfo) => dateTime.ToString("%H", cultureInfo).PadLeft(2, ' ') },
            // 12 hour of day no pad => 04
            { 'l', (dateTime, cultureInfo) => dateTime.ToString("%h", cultureInfo).PadLeft(2, ' ') },
            // millisecond of second => 032
            { 'L', (dateTime, cultureInfo) => dateTime.ToString("FFF", cultureInfo) },
            // month => 09
            { 'm', (dateTime, cultureInfo) => dateTime.ToString("MM", cultureInfo) },
            // minute of hour => 49
            { 'M', (dateTime, cultureInfo) => dateTime.ToString("mm", cultureInfo) },
            // newline char
            { 'n', (dateTime, cultureInfo) => "\n" },
            // nanosecond of second (9 digits) => 000000000
            { 'N', (dateTime, cultureInfo) => dateTime.ToString("fffffff00", cultureInfo) },
            // am/pm in caps => AM
            { 'p', (dateTime, cultureInfo) => dateTime.ToString("tt", cultureInfo) },
            // am/pm in lower case => am
            { 'P', (dateTime, cultureInfo) => dateTime.ToString("tt", cultureInfo).ToLowerInvariant() },
            // long time format 12 hours => 10:49:27 PM
            { 'r', (dateTime, cultureInfo) => dateTime.ToString("hh:mm:ss tt", cultureInfo) },
            // short time format 24 hours => 22:49
            { 'R', (dateTime, cultureInfo) => dateTime.ToString("HH:mm", cultureInfo) },
            // Number of seconds since 1970-01-01 00:00:00 +0000
            { 's', (dateTime, cultureInfo) => ((dateTime.ToUniversalTime().Ticks - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks) / TimeSpan.TicksPerSecond).ToString(cultureInfo) },
            // seconds
            { 'S', (dateTime, cultureInfo) => dateTime.ToString("ss", cultureInfo) },
            // tab char
            { 't', (dateTime, cultureInfo) => "\t" },
            // long time format 24 hours => 22:49:27
            { 'T', (dateTime, cultureInfo) => dateTime.ToString("HH:mm:ss", cultureInfo) },
            // Day of week (1 for mon, 7 for sun) => 4
            { 'u', (dateTime, cultureInfo) => ((dateTime.DayOfWeek == DayOfWeek.Sunday) ? 7 : (int) dateTime.DayOfWeek).ToString(cultureInfo) },
            // week of year (first sun of year as week 0: 00-53) => 36
            { 'U', (dateTime, cultureInfo) => cultureInfo.Calendar.GetWeekOfYear(dateTime, cultureInfo.DateTimeFormat.CalendarWeekRule, DayOfWeek.Sunday).ToString("D2", cultureInfo) },
            // VMS date culture invariant => 12-SEP-2013
            { 'v', (dateTime, cultureInfo) => string.Format(CultureInfo.InvariantCulture, "{0,2}-{1}-{2:D4}", dateTime.Day, dateTime.ToString("MMM", CultureInfo.InvariantCulture).ToUpperInvariant(), dateTime.Year) },
            // week of year iso8601 (01-53) => 37
            { 'V', (dateTime, cultureInfo) => cultureInfo.Calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday).ToString("D2", cultureInfo) },
            // week of year (first mon of year as week 0: 00-53) => 36
            { 'W', (dateTime, cultureInfo) => cultureInfo.Calendar.GetWeekOfYear(dateTime, cultureInfo.DateTimeFormat.CalendarWeekRule, DayOfWeek.Monday).ToString("D2", cultureInfo) },
            // day of week (0 for sun and 6 for sat) => 4
            { 'w', (dateTime, cultureInfo) => ((int)dateTime.DayOfWeek).ToString(cultureInfo) },
            // default date (no time)
            { 'x', (dateTime, cultureInfo) => dateTime.ToString("d", cultureInfo) },
            // default time (no date)
            { 'X', (dateTime, cultureInfo) => dateTime.ToString("T", cultureInfo) },
            // year 2 digit => 18
            { 'y', (dateTime, cultureInfo) => dateTime.ToString("yy", cultureInfo) },
            // year => 2018
            { 'Y', (dateTime, cultureInfo) => dateTime.ToString("yyyy", cultureInfo) },
            // time zone => IST
            { 'Z', (dateTime, cultureInfo) => dateTime.ToString("zzz", cultureInfo) },
            // Output % as-is
            { '%', (dateTime, cultureInfo) => "%" }
        };

        /// <summary>
        /// The default format for `datetime` objects.
        /// </summary>
        /// <remarks>
        /// This is exposed as well as default_format
        /// </remarks>
        public const string DefaultFormat = "%d %b %Y";

        [ScriptMemberIgnore]
        public static readonly ScriptVariable DateVariable = new ScriptVariableGlobal("date");

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeFunctions"/> class.
        /// </summary>
        public DateTimeFunctions()
        {
            Format = DefaultFormat;
            CreateImportFunctions();
        }

        /// <summary>
        /// Gets or sets the format used to format all `datetime` object.
        /// </summary>
        public string Format
        {
            get => GetSafeValue<string>(FormatKey) ?? DefaultFormat;
            set => SetValue(FormatKey, value, false);
        }

        /// <summary>
        /// Returns a `datetime` object of the current time, including the hour, minutes, seconds and milliseconds.
        /// </summary>
        /// <remarks>
        /// ```template-text
        /// {{ date.now.year }}
        /// ```
        /// ```html
        /// 2018
        /// ```
        /// </remarks>
        public static DateTime Now() => DateTime.Now;

        /// <summary>
        /// Adds the specified number of days to the input `datetime`. 
        /// </summary>
        /// <param name="date">The input `datetime` object.</param>
        /// <param name="days">The days.</param>
        /// <returns>A new `datetime` object.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ date.parse '2016/01/05' | date.add_days 1 }}
        /// ```
        /// ```html
        /// 06 Jan 2016
        /// ```
        /// </remarks>
        public static DateTime AddDays(DateTime date, double days)
        {
            return date.AddDays(days);
        }

        /// <summary>
        /// Adds the specified number of months to the input `datetime` object. 
        /// </summary>
        /// <param name="date">The input `datetime` object.</param>
        /// <param name="months">The months.</param>
        /// <returns>A new `datetime` object.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ date.parse '2016/01/05' | date.add_months 1 }}
        /// ```
        /// ```html
        /// 05 Feb 2016
        /// ```
        /// </remarks>
        public static DateTime AddMonths(DateTime date, int months)
        {
            return date.AddMonths(months);
        }

        /// <summary>
        /// Adds the specified number of years to the input `datetime` object. 
        /// </summary>
        /// <param name="date">The input `datetime` object.</param>
        /// <param name="years">The years.</param>
        /// <returns>A new `datetime` object.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ date.parse '2016/01/05' | date.add_years 1 }}
        /// ```
        /// ```html
        /// 05 Jan 2017
        /// ```
        /// </remarks>
        public static DateTime AddYears(DateTime date, int years)
        {
            return date.AddYears(years);
        }

        /// <summary>
        /// Adds the specified number of hours to the input `datetime` object. 
        /// </summary>
        /// <param name="date">The input `datetime` object.</param>
        /// <param name="hours">The hours.</param>
        /// <returns>A new `datetime` object.</returns>
        public static DateTime AddHours(DateTime date, double hours)
        {
            return date.AddHours(hours);
        }

        /// <summary>
        /// Adds the specified number of minutes to the input `datetime` object. 
        /// </summary>
        /// <param name="date">The input `datetime` object.</param>
        /// <param name="minutes">The minutes.</param>
        /// <returns>A new `datetime` object.</returns>
        public static DateTime AddMinutes(DateTime date, double minutes)
        {
            return date.AddMinutes(minutes);
        }

        /// <summary>
        /// Adds the specified number of seconds to the input `datetime` object. 
        /// </summary>
        /// <param name="date">The input `datetime` object.</param>
        /// <param name="seconds">The seconds.</param>
        /// <returns>A new `datetime` object.</returns>
        public static DateTime AddSeconds(DateTime date, double seconds)
        {
            return date.AddSeconds(seconds);
        }

        /// <summary>
        /// Adds the specified number of milliseconds to the input `datetime` object. 
        /// </summary>
        /// <param name="date">The input `datetime` object.</param>
        /// <param name="millis">The milliseconds.</param>
        /// <returns>A new `datetime` object.</returns>
        public static DateTime AddMilliseconds(DateTime date, double millis)
        {
            return date.AddMilliseconds(millis);
        }

        /// <summary>
        /// Parses the specified input string to a `datetime` object. 
        /// </summary>
        /// <param name="context">The template context.</param>
        /// <param name="text">A text representing a date.</param>
        /// <returns>A `datetime` object.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ date.parse '2016/01/05' }}
        /// ```
        /// ```html
        /// 05 Jan 2016
        /// ```
        /// </remarks>
        public static DateTime? Parse(TemplateContext context, string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            DateTime result;
            if (DateTime.TryParse(text, context.CurrentCulture, DateTimeStyles.None, out result))
                return result;

            return new DateTime();
        }

        /// <summary>
        /// Clones a `datetime` object.
        /// </summary>
        /// <param name="deep">Performs a deep clone.</param>
        /// <returns>An <see cref="IScriptObject"/> object.</returns>
        public override IScriptObject Clone(bool deep)
        {
            DateTimeFunctions dateFunctions = (DateTimeFunctions)base.Clone(deep);
            // This is important to call the CreateImportFunctions as it is instance specific (using DefaultFormat from `date` object)
            dateFunctions.CreateImportFunctions();
            return dateFunctions;
        }

        /// <summary>
        /// Converts a `datetime` object to a textual representation using the specified format string.
        /// 
        /// By default, if you are using a date, it will use the format specified by `date.format` which defaults to 
        /// `date.default_format` (readonly) which default to `%d %b %Y`.
        /// 
        /// You can override the format used for formatting all dates by assigning the a new format: 
        /// `date.format = '%a %b %e %T %Y';`.
        /// 
        /// You can recover the default format by using `date.format = date.default_format;`.
        /// 
        /// By default, the `to_string` format is using the **current culture**, but you can switch to an invariant 
        /// culture by using the modifier `%g`.
        /// 
        /// For example, using `%g %d %b %Y` will output the date using an invariant culture.
        /// 
        /// If you are using `%g` alone, it will output the date with `date.format` using an invariant culture.
        /// 
        /// Suppose that `date.now` would return the date `2013-09-12 22:49:27 +0530`, the following table explains the 
        /// format modifiers:
        /// 
        /// | Format | Result         | Description
        /// |--------|----------------|--------------------------------------------
        /// | `%a`   | `Thu`          | Day of week in short form
        /// | `%A`   | `Thursday`     | Day of week in full form
        /// | `%b`   | `Sep`          | Month in short form
        /// | `%B`   | `September`    | Month in full form
        /// | `%c`   | `Sun Dec 09 14:34:20 2018` | Date and time (%a %b %e %T %Y)
        /// | `%C`   | `20`           | Century in 2 digits
        /// | `%d`   | `03`           | Day of the month (01-31)
        /// | `%D`   | `09/12/13`     | Date (month-year-date)
        /// | `%e`   | `3`            | Day of the month (without padding, 1-31)
        /// | `%F`   | `2013-09-12`   | ISO 8601 date (year-month-date)
        /// | `%h`   | `Sep`          | Same as `%b`
        /// | `%H`   | `22`           | Hour in 24 hour format (00-24)
        /// | `%I`   | `10`           | Hour in 12 hour format (00-12)
        /// | `%j`   | `255`          | Day of year (001..366)
        /// | `%k`   | `22`           | Hour in 24 hour format (without padding, 0-23)
        /// | `%l`   | `10`           | Hour in 12 hour format (without padding 0-12)
        /// | `%L`   | `000`          | Millisecond (000-999)
        /// | `%m`   | `09`           | Month (01-12)
        /// | `%M`   | `49`           | Minute (00-59)
        /// | `%n`   |                | Newline character `\n`
        /// | `%N`   | `000000000`    | Nanoseconds (000000000-999999999)
        /// | `%p`   | `PM`           | AM/PM in upper case
        /// | `%P`   | `pm`           | AM/PM in lower case
        /// | `%r`   | `10:49:27 PM`  | Time in 12 hour long format
        /// | `%R`   | `22:49`        | Time in 24 hour short format
        /// | `%s`   |                | Number of seconds since epoch time (1970-01-01 00:00:00 +0000)
        /// | `%S`   | `27`           | Seconds (00-59)
        /// | `%t`   |                | Tab character `\t`
        /// | `%T`   | `22:49:27`     | Time in 24 hour long format
        /// | `%u`   | `4`            | Day of week (Mon to Sun, 1-7)
        /// | `%U`   | `36`           | Week of year (first Sunday of year as first week, 00-53)
        /// | `%v`   | `12-SEP-2013`  | VMS date (culture invariant)
        /// | `%V`   | `37`           | Week of year according to ISO 8601 (01-53)
        /// | `%W`   | `36`           | Week of year (first Monday of year as first week, 00-53)
        /// | `%w`   | `4`            | Day of week of the time (from 0 for Sunday to 6 for Saturday)
        /// | `%x`   |                | Default date (no time)
        /// | `%X`   |                | Default time (no date)
        /// | `%y`   | `13`           | Year in 2 digits
        /// | `%Y`   | `2013`         | Year in 4 digits
        /// | `%Z`   | `IST`          | Time zone
        /// | `%%`   | `%`            | Percent character `%`
        /// 
        /// Note that the format is using a good part of the [ruby format](http://apidock.com/ruby/DateTime/strftime).
        /// 
        /// ```template-text
        /// {{ date.parse '2016/01/05' | date.to_string `%d %b %Y` }}
        /// ```
        /// ```html
        /// 05 Jan 2016
        /// ```
        /// </summary>
        /// <param name="datetime">The input datetime to format</param>
        /// <param name="pattern">The date format pattern.</param>
        /// <param name="culture">The culture used to format the datetime</param>
        /// <returns>
        /// A <see cref="String"/> that represents this instance.
        /// </returns>
        public virtual string ToString(DateTime? datetime, string pattern, CultureInfo culture)
        {
            if (pattern == null)
                throw new ArgumentNullException(nameof(pattern));

            if (!datetime.HasValue)
                return null;

            // If pattern is %g only, use the default date
            if (pattern == "%g")
                pattern = "%g " + Format;

            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < pattern.Length; i++)
            {
                char c = pattern[i];
                if (c == '%' && (i + 1) < pattern.Length)
                {
                    i++;
                    Func<DateTime, CultureInfo, string> formatter;
                    char format = pattern[i];

                    // Switch to invariant culture
                    if (format == 'g')
                    {
                        culture = CultureInfo.InvariantCulture;
                        continue;
                    }

                    if (Formats.TryGetValue(format, out formatter))
                    {
                        builder.Append(formatter.Invoke(datetime.Value, culture));
                    }
                    else
                    {
                        builder.Append('%');
                        builder.Append(format);
                    }
                }
                else
                {
                    builder.Append(c);
                }
            }

            return builder.ToString();

        }

        public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
        {
            // If we access `date` without any parameter, it calls by default the "parse" function
            // otherwise it is the 'date' object itself
            switch (arguments.Count)
            {
                case 0:
                    return this;
                case 1:
                    return Parse(context, context.ToString(callerContext.Span, arguments[0]));
                default:
                    throw new ScriptRuntimeException(callerContext.Span, string.Format(RS.BadFunctionInvokeArgCountRange, "date", arguments.Count, 0, 1));
            }
        }

        private void CreateImportFunctions()
        {
            // This function is very specific, as it is calling a member function of this instance
            // in order to retrieve the `date.format`
            this.Import("to_string", new Func<TemplateContext, DateTime?, string, string>((context, date, pattern) => ToString(date, pattern, context.CurrentCulture)));
        }
    }
}