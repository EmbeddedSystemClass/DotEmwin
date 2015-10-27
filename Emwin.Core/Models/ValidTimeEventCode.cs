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
using Emwin.Core.References;
using Emwin.Core.Types;

namespace Emwin.Core.Models
{
    /// <summary>
    /// Valid Time Event Code (VTEC).
    /// An event is defined as a specific combination of phenomenon and level of significance.
    /// Each VTEC event is given an Event Tracking Number, or ETN.
    /// There are two types of VTEC, the P-(Primary) VTEC, and an H-(Hydrologic) VTEC.
    /// </summary>
    [DataContract]
    public class ValidTimeEventCode
    {
        #region Public Properties

        /// <summary>
        /// Gets the action.
        /// </summary>
        /// <value>The action.</value>
        [IgnoreDataMember]
        public string Action => VtecAction.ResourceManager.GetString(ActionCode);

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
        public string OfficeId { get; set; }

        /// <summary>
        /// Gets the phenomenon.
        /// </summary>
        /// <value>The phenomenon.</value>
        [IgnoreDataMember]
        public string Phenomenon => VtecPhenomenon.ResourceManager.GetString(PhenomenonCode);

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
        public VtecSignificanceCode Significance { get; set; }

        /// <summary>
        /// Gets or sets the type of VTEC.
        /// </summary>
        /// <value>The type of VTEC.</value>
        [DataMember]
        public VtecTypeCode TypeIdentifier { get; set; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Determines whether this event is currently active based on current UTC time and action field.
        /// </summary>
        /// <returns><c>true</c> if this instance is active; otherwise, <c>false</c>.</returns>
        public bool IsActive() => DateTimeOffset.UtcNow >= Begin && 
                                  DateTimeOffset.UtcNow <= End && Action != "CAN" && Action != "EXP";

        /// <summary>
        /// To the string.
        /// </summary>
        /// <returns>System.String.</returns>
        public override string ToString() => $"VTEC: {OfficeId} #{EventNumber} {Action} ({ActionCode}) {Phenomenon} ({PhenomenonCode}) {Significance} ({Begin:g} -> {End:g})";

        #endregion Public Methods

    }
}