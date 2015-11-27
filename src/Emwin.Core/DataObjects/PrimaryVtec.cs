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
using System.Runtime.Serialization;
using Emwin.Core.Contracts;

namespace Emwin.Core.DataObjects
{
    /// <summary>
    /// Valid Time Event Code (VTEC).
    /// An event is defined as a specific combination of phenomenon and level of significance.
    /// Each VTEC event is given an Event Tracking Number, or ETN.
    /// There are two types of VTEC, the P-(Primary) VTEC, and an H-(Hydrologic) VTEC.
    /// </summary>
    [DataContract]
    public class PrimaryVtec : IPrimaryVtec
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the action code.
        /// </summary>
        /// <value>The action code.</value>
        [DataMember]
        public string ActionCode { get; set; }

        /// <summary>
        /// Gets or sets the begin time.
        /// </summary>
        /// <value>The begin time or MinValue if event in progress.</value>
        [DataMember]
        public DateTimeOffset Begin { get; set; }

        /// <summary>
        /// Gets or sets the end time.
        /// </summary>
        /// <value>The end time.</value>
        [DataMember]
        public DateTimeOffset End { get; set; }

        /// <summary>
        /// Gets or sets the event tracking number.
        /// </summary>
        /// <value>The tracking number.</value>
        [DataMember]
        public int EventNumber { get; set; }

        /// <summary>
        /// Gets or sets the office identifier.
        /// </summary>
        /// <value>The office identifier.</value>
        [DataMember]
        public string WmoId { get; set; }

        /// <summary>
        /// Gets or sets the phenomenon code.
        /// </summary>
        /// <value>The phenomenon.</value>
        [DataMember]
        public string PhenomenonCode { get; set; }

        /// <summary>
        /// Gets or sets the significance code.
        /// </summary>
        /// <value>The significance.</value>
        [DataMember]
        public char SignificanceCode { get; set; }

        /// <summary>
        /// Gets or sets the type of VTEC.
        /// </summary>
        /// <value>The type of VTEC.</value>
        [DataMember]
        public char TypeIdentifier { get; set; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// To the string.
        /// </summary>
        /// <returns>System.String.</returns>
        public override string ToString() => $"VTEC: {WmoId} #{EventNumber} Action={ActionCode} Phonomenon={PhenomenonCode} Significance={SignificanceCode} ({Begin:g} -> {End:g})";

        #endregion Public Methods

    }
}