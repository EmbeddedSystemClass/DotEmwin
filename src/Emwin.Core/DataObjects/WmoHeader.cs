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

namespace Emwin.Core.DataObjects
{
    /// <summary>
    /// </summary>
    [DataContract]
    public class WmoHeader
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the type of the data or form.
        /// </summary>
        /// <value>The type of the data.</value>
        [DataMember]
        public string DataType { get; set; }

        /// <summary>
        /// Gets or sets the geographical distribution location.
        /// </summary>
        /// <value>The geographical distribution location.</value>
        [DataMember]
        public string Distribution { get; set; }

        /// <summary>
        /// Gets the indicator.
        /// </summary>
        /// <value>The indicator.</value>
        [DataMember]
        public string Designator { get; set; }

        /// <summary>
        /// Gets or sets the 4 letter originating office.
        /// </summary>
        /// <value>The wmo station.</value>
        [DataMember]
        public string WmoId { get; set; }

        /// <summary>
        /// Gets or sets the wmo time.
        /// </summary>
        /// <value>The wmo time.</value>
        [DataMember]
        public DateTimeOffset IssuedAt { get; set; }

        /// <summary>
        /// Gets or sets the location identifier (up to 3 characters).
        /// </summary>
        /// <value>The location identifier.</value>
        [DataMember]
        public string LocationIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the 3 character product category.
        /// </summary>
        /// <value>The product category.</value>
        [DataMember]
        public string ProductCategory { get; set; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString() => $"{DataType} {WmoId} {IssuedAt:ddHHmm} {Designator}\r\n{ProductCategory}{LocationIdentifier}";

        /// <summary>
        /// Determines whether this product is a correction.
        /// </summary>
        /// <returns>System.Boolean.</returns>
        public bool IsCorrection() => Designator != null && (Designator.StartsWith("CC") || Designator.StartsWith("AA"));

        /// <summary>
        /// Determines whether this product is an re-issuance.
        /// </summary>
        /// <returns>System.Boolean.</returns>
        public bool IsReissuance() => Designator != null && Designator.StartsWith("RR");

        #endregion Public Methods

    }
}
