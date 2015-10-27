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

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Emwin.Core.Interfaces;
using Emwin.Core.Models;
using Emwin.Core.Types;

namespace Emwin.Core.Parsers
{
    /// <summary>
    /// Class VtecParser. Valid Time Event Code.
    /// An event is defined as a specific combination of phenomenon and level of significance.
    /// Each VTEC event is given an Event Tracking Number, or ETN.
    /// There are two types of VTEC, the P-(Primary) VTEC, and an H-(Hydrologic) VTEC.
    /// </summary>
    public static class VtecParser
    {

        #region Private Fields

        /// <summary>
        /// The Primary VTEC Pattern
        /// </summary>
        private static readonly Regex PvtecRegex = new Regex(@"^/(?<type>[TO])\.(?<action>[A-Z]{3})\.(?<office>[A-Z]{4})\.(?<phen>[A-Z]{2})\.(?<sig>[A-Z])\.(?<number>[0-9]{4})\.(?<begin>[0-9]{6}T[0-9]{4}Z)-(?<end>[0-9]{6}T[0-9]{4}Z)/\r", 
            RegexOptions.Multiline | RegexOptions.ExplicitCapture);

        #endregion Private Fields

        #region Public Methods

        /// <summary>
        /// Parses the content and returns any VTEC items contained.
        /// </summary>
        /// <param name="product">The product.</param>
        /// <returns>IEnumerable&lt;ValidTimeEventCode&gt;.</returns>
        public static IEnumerable<ValidTimeEventCode> ParseProduct(ITextProduct product)
            => PvtecRegex.Matches(product.Content).Cast<Match>().Select(Create);

        /// <summary>
        /// Parses the vtec string.
        /// </summary>
        /// <param name="vtec">The vtec string.</param>
        /// <returns>ValidTimeEventCode.</returns>
        public static ValidTimeEventCode ParseVtec(string vtec) => Create(PvtecRegex.Match(vtec));

        #endregion Public Methods

        #region Private Methods

        private static ValidTimeEventCode Create(Match pvtecMatch) => new ValidTimeEventCode
        {
            TypeIdentifier = (VtecTypeCode) pvtecMatch.Groups["type"].Value[0],
            ActionCode = pvtecMatch.Groups["action"].Value,
            OfficeId = pvtecMatch.Groups["office"].Value,
            PhenomenonCode = pvtecMatch.Groups["phen"].Value,
            Significance = (VtecSignificanceCode) pvtecMatch.Groups["sig"].Value[0],
            EventNumber = int.Parse(pvtecMatch.Groups["number"].Value),
            Begin = TimeParser.ParseDateTime(pvtecMatch.Groups["begin"].Value),
            End = TimeParser.ParseDateTime(pvtecMatch.Groups["end"].Value)
        };

        #endregion Private Methods

    }
}
