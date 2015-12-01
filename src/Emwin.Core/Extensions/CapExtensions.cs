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
using System.Linq;
using Emwin.Core.Contracts;
using Emwin.Core.DataObjects;
using Emwin.Core.Parsers;
using Emwin.Core.Products;

namespace Emwin.Core.Extensions
{
    public static class CapExtension
    {

        #region Public Methods

        /// <summary>
        /// Generate Common Alerting Protocol (1.2) from bulletin
        /// </summary>
        /// <param name="segment">The bulletin.</param>
        /// <returns>alert.</returns>
        public static alert CreateAlert(this TextProductSegment segment) => new alert
        {
            identifier = Guid.NewGuid().ToString("N"),
            //sender = $"{segment.Header.WmoHeading.WmoId}@NWS.NOAA.GOV",
            sent = segment.TimeStamp.DateTime,
            status = GetAlertStatus(segment.GetPrimaryVtec().FirstOrDefault()),
            msgType = alertMsgType.Alert,
            scope = alertScope.Public,
            note = "",
            info = new[] {GetAlertInfo(segment)}
        };

        /// <summary>
        /// Gets the alert information.
        /// </summary>
        /// <param name="segment">The bulletin.</param>
        /// <returns>Emwin.Core.Contracts.alertInfo.</returns>
        public static alertInfo GetAlertInfo(this TextProductSegment segment) => new alertInfo
        {
            category = new[] { alertInfoCategory.Met },
            //@event = "",
            //urgency = segment.PrimaryVtec.GetUrgency(),
            //severity = segment.PrimaryVtec.GetSeverity(),
            //certainty = segment.PrimaryVtec.GetCertainty(),
            //eventCode = new[] { segment.PrimaryVtec.GetEventCode() },
            //effectiveSpecified = true,
            //effective = segment.PrimaryVtec.Begin.DateTime,
            //expiresSpecified = true,
            //expires = segment.PrimaryVtec.End.DateTime,
            //senderName = "DotEmwin",
            //headline = "",
            //description = segment.Content.Body,
            //instruction = "",
            //parameter = GetParameters(segment),
            //area = GetAreas(segment)
        };

        /// <summary>
        /// Gets the alert status.
        /// </summary>
        /// <param name="primaryVtec">The pvtec.</param>
        /// <returns>Emwin.Core.Contracts.alertStatus.</returns>
        public static alertStatus GetAlertStatus(this PrimaryVtec primaryVtec)
        {
            switch (primaryVtec.TypeIdentifier)
            {
                case 'O': return alertStatus.Actual;
                case 'T': return alertStatus.Test;
                case 'E': return alertStatus.Exercise;
                case 'X': return alertStatus.Draft;
            }

            return alertStatus.Test;
        }

        /// <summary>
        /// Gets the certainty.
        /// </summary>
        /// <param name="primaryVtec">The primary vtec.</param>
        /// <returns>Emwin.Core.Contracts.alertInfoCertainty.</returns>
        public static alertInfoCertainty GetCertainty(this PrimaryVtec primaryVtec)
        {
            switch (primaryVtec.SignificanceCode)
            {
                case 'N':
                case 'W': return alertInfoCertainty.Observed;

                case 'F':
                case 'A': return alertInfoCertainty.Likely;

                case 'Y': return alertInfoCertainty.Possible;
            }

            return alertInfoCertainty.Unknown;
        }

        /// <summary>
        /// Gets the event code.
        /// </summary>
        /// <param name="pvtec">The pvtec.</param>
        /// <returns>Emwin.Core.Contracts.alertInfoEventCode.</returns>
        public static alertInfoEventCode GetEventCode(this PrimaryVtec pvtec) => new alertInfoEventCode
        {
            valueName = "SAME",
            value = string.Concat(pvtec.PhenomenonCode, pvtec.SignificanceCode)
        };

        public static string GetPolygonText(this GeoPoint[] polygon)
        {
            if (polygon == null || polygon.Length == 0) return null;
            var points = polygon.Select(p => p.ToString());
            return string.Join(" ", points);
        }

        /// <summary>
        /// Gets the severity.
        /// </summary>
        /// <param name="primaryVtec">The pvtec.</param>
        /// <returns>Emwin.Core.Contracts.alertInfoSeverity.</returns>
        public static alertInfoSeverity GetSeverity(this PrimaryVtec primaryVtec)
        {
            switch (primaryVtec.SignificanceCode)
            {
                case 'W': return alertInfoSeverity.Extreme;
                case 'A': return alertInfoSeverity.Severe;
                case 'Y': return alertInfoSeverity.Moderate;
                case 'S': return alertInfoSeverity.Minor;
            }

            return alertInfoSeverity.Unknown;
        }

        /// <summary>
        /// Gets the urgency.
        /// </summary>
        /// <param name="primaryVtec">The primary vtec.</param>
        /// <returns>Emwin.Core.Contracts.alertInfoUrgency.</returns>
        public static alertInfoUrgency GetUrgency(this PrimaryVtec primaryVtec)
        {
            switch (primaryVtec.SignificanceCode)
            {
                case 'W': return alertInfoUrgency.Immediate;
                case 'A': return alertInfoUrgency.Expected;

                case 'F':
                case 'O':
                case 'S':
                case 'Y': return alertInfoUrgency.Future;

                case 'N': return alertInfoUrgency.Past;
            }

            return alertInfoUrgency.Unknown;
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Gets the areas.
        /// </summary>
        /// <param name="segment">The bulletin.</param>
        /// <returns>Emwin.Core.Contracts.alertInfoArea[].</returns>
        private static alertInfoArea[] GetAreas(TextProductSegment segment) => new[]
        {
            new alertInfoArea
            {
                //areaDesc = "",
                polygon = segment.GetPolygons().Select(x => x.GetPolygonText()).ToArray(),
                //geocode = new[]
                //{
                //    new alertInfoAreaGeocode {valueName = "FIPS6", value = ""},
                //    new alertInfoAreaGeocode {valueName = "UGC", value = ""}
                //}
            }
        };

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <param name="segment">The bulletin.</param>
        /// <returns>Emwin.Core.Contracts.alertInfoParameter[].</returns>
        private static alertInfoParameter[] GetParameters(TextProductSegment segment) => new alertInfoParameter[]
        {
            //new alertInfoParameter {valueName = "WMOHEADER", value = ""},
            //new alertInfoParameter {valueName = "UGC", value = ""},
            //new alertInfoParameter
            //{
            //    valueName = "VTEC",
            //    value = string.Join(Environment.NewLine, segment.GetPrimaryVtec().First().ToRaw(), segment.HydrologicVtec?.ToRaw())
            //},
            //segment.TrackingLine == null ? null : new alertInfoParameter
            //{
            //    valueName = "TIME...MOT...LOC",
            //    value = segment.TrackingLine?.ToString()
            //}
        };

        #endregion Private Methods
    }
}