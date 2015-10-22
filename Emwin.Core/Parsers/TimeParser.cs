/*
 * The MIT License (MIT)
 * 
 * Copyright (c) 2015 Jonathan Bradshaw <jonathan@nrgup.net>
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */
using System;
using System.Globalization;

namespace Emwin.Core.Parsers
{
    internal static class TimeParser
    {
        /// <summary>
        /// Parses the date time string.
        /// </summary>
        /// <param name="time">The time.</param>
        /// <returns>DateTimeOffset.</returns>
        public static DateTimeOffset ParseDateTime(string time)
        {
            return time == "000000T0000Z"
                ? DateTimeOffset.MinValue
                : DateTimeOffset.ParseExact(time, "yyMMdd'T'HHmm'Z'", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
        }

        /// <summary>
        /// Parses the time in day hour minute format..
        /// </summary>
        /// <param name="reference">The reference.</param>
        /// <param name="dayHourMinute">The time string in day hour minute format.</param>
        /// <returns>DateTimeOffset.</returns>
        public static DateTimeOffset ParseDayHourMinute(DateTimeOffset reference, string dayHourMinute)
        {
            int dayOfMonth, hour, minute;
            int.TryParse(dayHourMinute.Substring(0, 2), out dayOfMonth);
            int.TryParse(dayHourMinute.Substring(2, 2), out hour);
            int.TryParse(dayHourMinute.Substring(4, 2), out minute);

            // Check for case when day of month rolls over to the next month
            if (dayOfMonth == 1 && reference.Day > 1)
                reference = reference.AddMonths(1);

            // Check for case when day of month was at the end of previous month
            var daysLastMonth = DateTime.DaysInMonth(reference.Year, reference.AddMonths(-1).Month);
            if (dayOfMonth == daysLastMonth && reference.Day == 1)
                reference = reference.AddMonths(-1);

            return new DateTimeOffset(reference.Year, reference.Month, dayOfMonth, hour, minute, 0, TimeSpan.Zero);
        }
    }
}