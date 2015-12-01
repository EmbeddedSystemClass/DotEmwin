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

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Emwin.Core.DataObjects;
using Emwin.Core.Products;

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

        private static readonly Regex HvtecRegex = new Regex(@"^/(?<nwsli>[0-9A-Z]{5})\.(?<severity>[N0123U])\.(?<cause>[A-Z]{2})\.(?<begin>[0-9]{6}T[0-9]{4}Z)\.(?<crest>[0-9]{6}T[0-9]{4}Z)\.(?<end>[0-9]{6}T[0-9]{4}Z)\.(?<floodrecord>[A-Z]{2})/\r",
                    RegexOptions.Multiline | RegexOptions.ExplicitCapture | RegexOptions.Compiled);

        /// <summary>
        /// The Primary VTEC Pattern
        /// </summary>
        private static readonly Regex PvtecRegex = new Regex(@"^/(?<type>[TO])\.(?<action>[A-Z]{3})\.(?<office>[A-Z]{4})\.(?<phen>[A-Z]{2})\.(?<sig>[A-Z])\.(?<number>[0-9]{4})\.(?<begin>[0-9]{6}T[0-9]{4}Z)-(?<end>[0-9]{6}T[0-9]{4}Z)/\r", 
            RegexOptions.Multiline | RegexOptions.ExplicitCapture | RegexOptions.Compiled);

        #endregion Private Fields

        #region Public Methods

        /// <summary>
        /// Parses the content and returns any Hydrologic VTEC items contained.
        /// </summary>
        /// <param name="product">The product.</param>
        /// <returns>System.Collections.Generic.IEnumerable&lt;Emwin.Core.Contracts.HydrologicVtec&gt;.</returns>
        public static IEnumerable<HydrologicVtec> GetHydrologicVtec(this TextProduct product)
                    => HvtecRegex.Matches(product.Content.Body).Cast<Match>().Select(CreateHydrologicVtec);

        /// <summary>
        /// Parses the hydrologic vtec.
        /// </summary>
        /// <param name="hvtec">The hvtec.</param>
        /// <returns>Emwin.Core.Contracts.HydrologicVtec.</returns>
        public static HydrologicVtec ParseHydrologicVtec(string hvtec) => CreateHydrologicVtec(HvtecRegex.Match(hvtec));

        /// <summary>
        /// Parses the content and returns any Primary VTEC items contained.
        /// </summary>
        /// <param name="product">The product.</param>
        /// <returns>IEnumerable&lt;ValidTimeEventCode&gt;.</returns>
        public static IEnumerable<PrimaryVtec> GetPrimaryVtec(this TextProduct product)
            => PvtecRegex.Matches(product.Content.Body).Cast<Match>().Select(CreatePrimaryVtec);
        /// <summary>
        /// Parses the VTEC string.
        /// </summary>
        /// <param name="pvtec">The VTEC.</param>
        /// <returns>ValidTimeEventCode.</returns>
        public static PrimaryVtec ParsePrimaryVtec(string pvtec) => CreatePrimaryVtec(PvtecRegex.Match(pvtec));

        #endregion Public Methods

        #region Private Methods

        private static HydrologicVtec CreateHydrologicVtec(Match hvtecMatch) => new HydrologicVtec
        {
            LocationIdentifier = hvtecMatch.Groups["nwsli"].Value,
            SeverityCode = hvtecMatch.Groups["severity"].Value[0],
            ImmediateCauseCode = hvtecMatch.Groups["cause"].Value,
            Begin = TimeParser.ParseDateTime(hvtecMatch.Groups["begin"].Value),
            Crest = TimeParser.ParseDateTime(hvtecMatch.Groups["crest"].Value),
            End = TimeParser.ParseDateTime(hvtecMatch.Groups["end"].Value),
            FloodRecordStatusCode = hvtecMatch.Groups["floodrecord"].Value
        };

        private static PrimaryVtec CreatePrimaryVtec(Match pvtecMatch) => new PrimaryVtec
        {
            TypeIdentifier = pvtecMatch.Groups["type"].Value[0],
            ActionCode = pvtecMatch.Groups["action"].Value,
            WmoId = pvtecMatch.Groups["office"].Value,
            PhenomenonCode = pvtecMatch.Groups["phen"].Value,
            SignificanceCode = pvtecMatch.Groups["sig"].Value[0],
            EventNumber = int.Parse(pvtecMatch.Groups["number"].Value),
            Begin = TimeParser.ParseDateTime(pvtecMatch.Groups["begin"].Value),
            End = TimeParser.ParseDateTime(pvtecMatch.Groups["end"].Value)
        };

        #endregion Private Methods
    }
}
