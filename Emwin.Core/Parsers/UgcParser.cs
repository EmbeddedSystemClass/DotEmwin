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
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Emwin.Core.Contracts;
using Emwin.Core.DataObjects;

namespace Emwin.Core.Parsers
{
    /// <summary>
    /// Class UgcParser. Universal Geographic Code.
    /// </summary>
    public static class UgcParser
    {
        #region Private Fields

        /// <summary>
        /// The Universal Geographic Code Pattern
        /// </summary>
        private static readonly Regex UgcRegex = new Regex(@"[A-Z]{2}[CZ][0-9AL]{3}([A-Z0-9\r\n>-]*?)[0-9]{6}-\r", RegexOptions.Singleline);

        #endregion Private Fields

        #region Public Methods

        /// <summary>
        /// Parses the content and returns any UGC codes contained.
        /// </summary>
        /// <param name="product">The product.</param>
        /// <returns>IEnumerable&lt;UniversalGeographicCode&gt;.</returns>
        public static IEnumerable<UniversalGeographicCode> ParseProduct(ITextProduct product)
        {
            var ugcMatches = UgcRegex.Matches(product.Content);
            return ugcMatches.Cast<Match>().SelectMany(ugcMatch => ParseUgc(RemoveWhitespace(ugcMatch.Value), product.TimeStamp));
        }

        /// <summary>
        /// Parses the Universal Geographic Code string.
        /// </summary>
        /// <param name="ugc">The Universal Geographic Code string.</param>
        /// <param name="referenceTime">The reference time used for dates.</param>
        /// <returns>IEnumerable&lt;UniversalGeographicCode&gt;.</returns>
        public static IEnumerable<UniversalGeographicCode> ParseUgc(string ugc, DateTimeOffset referenceTime)
        {
            // WIZ001-002-006>008-014>016-023>028-212300-
            string state = null;
            var type = char.MinValue;
            var segments = ugc.Split(new[] {'-'}, StringSplitOptions.RemoveEmptyEntries);
            var purgeTime = TimeParser.ParseDayHourMinute(referenceTime, segments[segments.Length - 1]);
            var index = 0;
            while (index < segments.Length-1)
            {
                var segment = segments[index++];
                int start, end;

                // Zone or county value (eg: 002 or ALL)
                if (state != null && type != char.MinValue && (segment == "ALL" || segment.All(char.IsDigit)))
                {
                    yield return new UniversalGeographicCode
                    {
                        State = state,
                        Type = type,
                        PurgeTime = purgeTime,
                        Id = segment.PadLeft(3, '0')
                    };
                    continue;
                }

                // State with type and location (eg: WIZ001)
                if (segment.IndexOf('>') < 0 && segment.Substring(0, 3).All(char.IsLetter))
                {
                    state = segment.Substring(0, 2);
                    type = segment[2];
                    yield return new UniversalGeographicCode
                    {
                        State = state,
                        Type = type,
                        PurgeTime = purgeTime,
                        Id = segment.Substring(3).PadLeft(3, '0')
                    };
                    continue;
                }

                // Range (eg: WIZ1>100)
                if (segment.IndexOf('>') > 4 && segment.Substring(0, 3).All(char.IsLetter))
                {
                    var split = segment.Split('>');
                    state = split[0].Substring(0, 2);
                    type = split[0][2];
                    start = int.Parse(split[0].Substring(3));
                    end = int.Parse(split[1]);
                    for (var i = start; i <= end; i++)
                        yield return new UniversalGeographicCode
                        {
                            State = state,
                            Type = type,
                            PurgeTime = purgeTime,
                            Id = i.ToString("D3")
                        };
                    continue;
                }

                // Range (eg: 001>100)
                if (segment.IndexOf('>') > 0 && char.IsDigit(segment[0]))
                {
                    var split = segment.Split('>');
                    start = int.Parse(split[0]);
                    end = int.Parse(split[1]);
                    for (var i = start; i <= end; i++)
                        yield return new UniversalGeographicCode
                        {
                            State = state,
                            Type = type,
                            PurgeTime = purgeTime,
                            Id = i.ToString("D3")
                        };
                    continue;
                }

                throw new InvalidOperationException("Unable to parse segment '" + segment + "' from " + ugc);
            }
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Removes the whitespace.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>System.String.</returns>
        private static string RemoveWhitespace(string input) => new string(input.Where(c => !char.IsWhiteSpace(c)).ToArray());

        #endregion Private Methods
    }
}
