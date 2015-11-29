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

namespace Emwin.Core.Products
{
    public static class CapExtension
    {

        #region Public Methods

        /// <summary>
        /// Generate Common Alerting Protocol (1.2) from bulletin
        /// </summary>
        /// <param name="bulletin">The bulletin.</param>
        /// <returns>alert.</returns>
        public static alert CreateAlert(this BulletinProduct bulletin) => new alert
        {
            identifier = Guid.NewGuid().ToString("N"),
            sender = $"{bulletin.Header.WmoId}@NWS.NOAA.GOV",
            sent = bulletin.TimeStamp.DateTime,
            status = GetAlertStatus(bulletin.PrimaryVtec),
            msgType = alertMsgType.Alert,
            scope = alertScope.Public,
            note = "",
            info = new[] {GetAlertInfo(bulletin)}
        };

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Gets the alert information.
        /// </summary>
        /// <param name="bulletin">The bulletin.</param>
        /// <returns>Emwin.Core.Contracts.alertInfo.</returns>
        private static alertInfo GetAlertInfo(BulletinProduct bulletin) => new alertInfo
        {
            category = new[] { alertInfoCategory.Met },
            @event = "",
            urgency = GetUrgency(bulletin.PrimaryVtec),
            severity = GetSeverity(bulletin.PrimaryVtec),
            certainty = GetCertainty(bulletin.PrimaryVtec),
            eventCode = new[] { GetEventCode(bulletin.PrimaryVtec) },
            effectiveSpecified = true,
            effective = bulletin.PrimaryVtec.Begin.DateTime,
            expiresSpecified = true,
            expires = bulletin.PrimaryVtec.End.DateTime,
            senderName = "DotEmwin",
            headline = "",
            description = bulletin.Content,
            instruction = "",
            parameter = GetParameters(bulletin),
            area = GetAreas(bulletin)
        };

        /// <summary>
        /// Gets the urgency.
        /// </summary>
        /// <param name="primaryVtec">The primary vtec.</param>
        /// <returns>Emwin.Core.Contracts.alertInfoUrgency.</returns>
        private static alertInfoUrgency GetUrgency(PrimaryVtec primaryVtec)
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

        /// <summary>
        /// Gets the alert status.
        /// </summary>
        /// <param name="primaryVtec">The pvtec.</param>
        /// <returns>Emwin.Core.Contracts.alertStatus.</returns>
        private static alertStatus GetAlertStatus(PrimaryVtec primaryVtec)
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
        /// Gets the severity.
        /// </summary>
        /// <param name="primaryVtec">The pvtec.</param>
        /// <returns>Emwin.Core.Contracts.alertInfoSeverity.</returns>
        private static alertInfoSeverity GetSeverity(PrimaryVtec primaryVtec)
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
        /// Gets the certainty.
        /// </summary>
        /// <param name="primaryVtec">The primary vtec.</param>
        /// <returns>Emwin.Core.Contracts.alertInfoCertainty.</returns>
        private static alertInfoCertainty GetCertainty(PrimaryVtec primaryVtec)
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
        /// Gets the areas.
        /// </summary>
        /// <param name="bulletin">The bulletin.</param>
        /// <returns>Emwin.Core.Contracts.alertInfoArea[].</returns>
        private static alertInfoArea[] GetAreas(BulletinProduct bulletin) => new[]
        {
            new alertInfoArea
            {
                areaDesc = "",
                polygon = new[] {string.Join(" ", bulletin.Polygon.Select(p => p.ToString()))},
                //geocode = new[]
                //{
                //    new alertInfoAreaGeocode {valueName = "FIPS6", value = ""},
                //    new alertInfoAreaGeocode {valueName = "UGC", value = ""}
                //}
            }
        };

        /// <summary>
        /// Gets the event code.
        /// </summary>
        /// <param name="pvtec">The pvtec.</param>
        /// <returns>Emwin.Core.Contracts.alertInfoEventCode.</returns>
        private static alertInfoEventCode GetEventCode(PrimaryVtec pvtec) => new alertInfoEventCode
        {
            valueName = "SAME",
            value = string.Concat(pvtec.PhenomenonCode, pvtec.SignificanceCode)
        };

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <param name="bulletin">The bulletin.</param>
        /// <returns>Emwin.Core.Contracts.alertInfoParameter[].</returns>
        private static alertInfoParameter[] GetParameters(BulletinProduct bulletin) => new[]
        {
            //new alertInfoParameter {valueName = "WMOHEADER", value = ""},
            //new alertInfoParameter {valueName = "UGC", value = ""},
            new alertInfoParameter
            {
                valueName = "VTEC",
                value = string.Join(Environment.NewLine, bulletin.PrimaryVtec?.ToRaw(), bulletin.HydrologicVtec?.ToRaw())
            },
            new alertInfoParameter
            {
                valueName = "TIME...MOT...LOC",
                value = bulletin.TrackingLine?.ToString()
            }
        };

        #endregion Private Methods

    }
}