﻿/*
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
    public static class TrackingLineParser
    {

        #region Private Fields

        private static readonly Regex TimeMotLocRegex = new Regex(@"^TIME\.\.\.MOT\.\.\.LOC\s(?<time>[0-9]{4})Z\s(?<mot>[0-9]{3})DEG\s(?<wind>[0-9]{1,2})KT(?:\s(?<points>[0-9]{4}\s[0-9]{4,5}))+", RegexOptions.ExplicitCapture | RegexOptions.Multiline | RegexOptions.Compiled);

        #endregion Private Fields

        #region Public Methods

        /// <summary>
        /// Parses the product and returns a set of latitude/longitude points.
        /// </summary>
        /// <param name="product">The product.</param>
        /// <returns>IEnumerable&lt;TrackingLine&gt;.</returns>
        public static IEnumerable<TrackingLine> GetTrackingLines(this TextProduct product)
        {
            var matches = TimeMotLocRegex.Matches(product.Content.RawBody);

            return matches.Cast<Match>().Select(match => new TrackingLine
            {
                TimeStamp = TimeParser.ParseHourMinute(product.TimeStamp, match.Groups["time"].Value),
                DirectionDeg = int.Parse(match.Groups["mot"].Value),
                WindSpeedKts = int.Parse(match.Groups["wind"].Value),
                Line = match.Groups["points"].Captures.Cast<Capture>().Select(ToLocation).ToList()
            });
        }

        #endregion Public Methods

        #region Private Methods

        private static GeoPoint ToLocation(Capture points)
        {
            var split = points.Value.Split(' ');
            return new GeoPoint(double.Parse(split[0])/100.0, -double.Parse(split[1])/100.0);
        }

        #endregion Private Methods

    }
}
