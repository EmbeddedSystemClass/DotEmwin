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

namespace Emwin.Core.Models
{
    /// <summary>
    /// WmoHeader represents the WMO abbreviated heading of the product.
    /// http://www.nws.noaa.gov/tg/head.php
    /// http://www.nws.noaa.gov/tg/table.php
    /// </summary>
    [DataContract]
    public class WmoHeading
    {
        #region Public Enums

        public enum T1Code
        {
            Analyses = 'A',
            AddressedMessage = 'B',
            ClimaticData = 'C',
            GridPointInformationD = 'D',
            SatelliteImagery = 'E',
            Forecast = 'F',
            GridPointInformationG = 'G',
            GridPointInformationH = 'H',
            ObservationalData = 'I',
            ForecastData = 'J',
            Crex = 'K',
            AviationXmlData = 'L',
            Notices = 'N',
            OceanographicInformation = 'O',
            PictorialInformation = 'P',
            RegionalPictorialInformation = 'Q',
            SurfaceData = 'S',
            SatelliteData = 'T',
            UpperAirData = 'U',
            NationalData = 'V',
            Warnings = 'W',
            CommonAlertProtocolMessages = 'X',
            GribRegionalUse = 'Y'
        }

        #endregion Public Enums

        #region Public Properties

        /// <summary>
        /// Gets or sets the type of the data.
        /// </summary>
        /// <value>The type of the data.</value>
        public T1Code DataType { get; set; }

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
        /// Gets or sets the wmo station.
        /// </summary>
        /// <value>The wmo station.</value>
        [DataMember]
        public string Station { get; set; }

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
        public override string ToString() => $"T1: {DataType} Id: {Id} Station: {Station} Time: {Time:g} Indicator: {Indicator ?? "-"}";

        #endregion Public Methods

    }
}
