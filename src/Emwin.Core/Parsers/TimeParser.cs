/*
 * Microsoft Public License (MS-PL)
 * Copyright (c) 2015 Jonathan Bradshaw <jonathan@nrgup.net>
 *     
 * This license governs use of the accompanying software. If you use the software, you
 * accept this license. If you do not accept the license, do not use the software.
 *     
 * 1. Definitions
 *     The terms "reproduce," "reproduction," "derivative works," and "distribution" have the
 *     same meaning here as under U.S. copyright law.
 *     A "contribution" is the original software, or any additions or changes to the software.
 *     A "contributor" is any person that distributes its contribution under this license.
 *     "Licensed patents" are a contributor's patent claims that read directly on its contribution.
 *     
 * 2. Grant of Rights
 *     (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
 *     (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
 *     
 * 3. Conditions and Limitations
 *     (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
 *     (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from such contributor to the software ends automatically.
 *     (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present in the software.
 *     (D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a license that complies with this license.
 *     (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement.
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
        public static DateTimeOffset ParseDateTime(string time) => time == "000000T0000Z"
            ? DateTimeOffset.MinValue
            : DateTimeOffset.ParseExact(time, "yyMMdd'T'HHmm'Z'", CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal);

        /// <summary>
        /// Parses the hour minute.
        /// </summary>
        /// <param name="reference">The reference.</param>
        /// <param name="dayHourMinute">The day hour minute.</param>
        /// <returns>System.DateTimeOffset.</returns>
        public static DateTimeOffset ParseHourMinute(DateTimeOffset reference, string dayHourMinute)
        {
            if (reference == DateTimeOffset.MinValue || reference == DateTimeOffset.MaxValue)
                reference = DateTimeOffset.UtcNow;

            int hour, minute;
            int.TryParse(dayHourMinute.Substring(0, 2), out hour);
            int.TryParse(dayHourMinute.Substring(2, 2), out minute);

            return new DateTimeOffset(reference.Year, reference.Month, reference.Day, hour, minute, 0, TimeSpan.Zero);
        }

        /// <summary>
        /// Parses the time in day hour minute format..
        /// </summary>
        /// <param name="reference">The reference.</param>
        /// <param name="dayHourMinute">The time string in day hour minute format.</param>
        /// <returns>DateTimeOffset.</returns>
        public static DateTimeOffset ParseDayHourMinute(DateTimeOffset reference, string dayHourMinute)
        {
            if (reference == DateTimeOffset.MinValue || reference == DateTimeOffset.MaxValue)
                reference = DateTimeOffset.UtcNow;

            int dayOfMonth, hour, minute;
            int.TryParse(dayHourMinute.Substring(0, 2), out dayOfMonth);
            int.TryParse(dayHourMinute.Substring(2, 2), out hour);
            int.TryParse(dayHourMinute.Substring(4, 2), out minute);

            if (dayOfMonth != reference.Day)
            {
                // Check for previous month
                if (dayOfMonth > 25 && reference.Day < 5)
                    reference = reference.AddMonths(-1);

                // Next month
                else if (dayOfMonth < 5 && reference.Day > 25)
                    reference = reference.AddMonths(1);
            }

            return new DateTimeOffset(reference.Year, reference.Month, dayOfMonth, hour, minute, 0, TimeSpan.Zero);
        }
    }
}