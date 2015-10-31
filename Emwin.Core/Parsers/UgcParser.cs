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
        private static readonly Regex UgcRegex = new Regex(@"[\r\n]*[A-Z]{2}[CZ][0-9AL]{3}([A-Z0-9\r\n>-]*?)[0-9]{6}-[\r\n]+", RegexOptions.Singleline);

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
            return ugcMatches.Cast<Match>().SelectMany(ugcMatch => ParseUgc(ugcMatch.Value, product.TimeStamp));
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
            var segments = RemoveWhitespace(ugc).Split(new[] {'-'}, StringSplitOptions.RemoveEmptyEntries);
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
