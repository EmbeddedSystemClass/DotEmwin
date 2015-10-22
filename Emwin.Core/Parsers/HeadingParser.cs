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
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Emwin.Core.Models;

namespace Emwin.Core.Parsers
{
    /// <summary>
    /// AwipsHeader represents the header information on the first line of the product.
    /// </summary>
    [DataContract]
    public static class HeadingParser
    {

        #region Private Fields

        /// <summary>
        /// The WMO Abbreviated Header
        /// http://www.nws.noaa.gov/tg/head.php
        /// </summary>
        private static readonly Regex WmoAbbreviatedHeaderRegex = new Regex(
            @"^(?<id>[A-Z]{4}[0-9]{2})\s+(?<station>[A-Z]{4})\s+(?<time>[0-9]{6})(\s(?<indicator>[A-Z]{3}))?", 
            RegexOptions.ExplicitCapture | RegexOptions.Multiline);

        #endregion Private Fields

        #region Public Methods

        /// <summary>
        /// Creates the header from the content.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns>Header.</returns>
        /// <exception cref="System.ArgumentException">Invalid content header</exception>
        public static CommsHeader ParseProduct(WeatherProduct content)
        {
            var match = WmoAbbreviatedHeaderRegex.Match(content.GetString());
            if (!match.Success) return null;

            return new CommsHeader
            {
                Id = match.Groups["id"].Value,
                DataType = (CommsHeader.T1Code) match.Groups["id"].Value[0],
                OriginatingOffice = match.Groups["station"].Value,
                Time = TimeParser.ParseDayHourMinute(content.TimeStamp, match.Groups["time"].Value),
                Indicator = match.Groups["indicator"].Success ? match.Groups["indicator"].Value : null
            };
        }

        #endregion Public Methods

    }
}
