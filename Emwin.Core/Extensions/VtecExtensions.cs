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
using Emwin.Core.DataObjects;
using Emwin.Core.Types;

namespace Emwin.Core.Extensions
{
    public static class VtecExtensions
    {
        /// <summary>
        /// Determines whether the specified vtec is active.
        /// </summary>
        /// <param name="vtec">The vtec.</param>
        /// <returns><c>true</c> if the specified vtec is active; otherwise, <c>false</c>.</returns>
        public static bool IsActive(this ValidTimeEventCode vtec)
        {
            return DateTimeOffset.UtcNow >= vtec.Begin &&
                   DateTimeOffset.UtcNow <= vtec.End && vtec.Action != "CAN" && vtec.Action != "EXP";
        }

        /// <summary>
        /// Gets the significance level.
        /// </summary>
        /// <param name="vtec">The vtec.</param>
        /// <returns>VtecSignificanceLevel.</returns>
        public static VtecSignificanceLevel GetSignificanceLevel(this ValidTimeEventCode vtec)
        {
            switch (vtec.SignificanceCode)
            {
                case 'Y': return VtecSignificanceLevel.Advisory;
                case 'F': return VtecSignificanceLevel.Forecast;
                case 'O': return VtecSignificanceLevel.Outlook;
                case 'S': return VtecSignificanceLevel.Statement;
                case 'N': return VtecSignificanceLevel.Synopsis;
                case 'W': return VtecSignificanceLevel.Warning;
                case 'A': return VtecSignificanceLevel.Watch;
            }

            return VtecSignificanceLevel.None;
        }

        /// <summary>
        /// Determines whether the specified vtec is operational.
        /// </summary>
        /// <param name="vtec">The vtec.</param>
        /// <returns>System.Boolean.</returns>
        public static bool IsOperational(this ValidTimeEventCode vtec)
                    => vtec.TypeIdentifier == (char)VtecTypeCode.Operational;

    }
}
