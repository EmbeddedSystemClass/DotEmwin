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
using Emwin.Core.Types;

namespace Emwin.Core.DataObjects
{
    /// <summary>
    /// CommsHeader represents the WMO abbreviated heading of the product.
    /// 
    /// PURPOSE of the Abbreviated Heading;
    ///
    /// The heading is to provide a means by which communication data managers recognize a bulletin for telecommunication "switching" purposes.
    /// The heading permits a uniqueness for a bulletin, which is sufficient enough to control the data for selective transmission required to meet the needs of the receiving end.
    /// The heading is for accountability in the transmission delivery process by the switching system for data management purposes.
    /// The heading is not intended for the data processing systems, as the first few lines of the text(bulletin content) further defines it for processing. (ref. WMO Codes Manual 306)
    /// 
    /// http://www.nws.noaa.gov/tg/head.php
    /// http://www.nws.noaa.gov/tg/headef.php
    /// http://www.nws.noaa.gov/tg/table.php
    /// </summary>
    [DataContract]
    public class CommsHeader
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the type of the data.
        /// </summary>
        /// <value>The type of the data.</value>
        public T1DataTypeCode DataType { get; set; }

        /// <summary>
        /// Gets the 6 character wmo identifier.
        /// </summary>
        /// <value>The wmo identifier.</value>
        [DataMember]
        public string Id { get; set; }

        /// <summary>
        /// Gets the indicator.
        /// </summary>
        /// <value>The indicator.</value>
        [DataMember]
        public string Indicator { get; set; }

        /// <summary>
        /// Gets or sets the originating station.
        /// </summary>
        /// <value>The wmo station.</value>
        [DataMember]
        public string OriginatingOffice { get; set; }

        /// <summary>
        /// Gets or sets the wmo time.
        /// </summary>
        /// <value>The wmo time.</value>
        [DataMember]
        public DateTimeOffset Time { get; set; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString() => $"DataType={DataType} Id={Id} Station={OriginatingOffice} Time={Time:g} Indicator={Indicator}";

        #endregion Public Methods

    }
}
