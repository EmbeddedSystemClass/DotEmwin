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
    /// Universal Geographic Code.
    /// </summary>
    [DataContract]
    public class UniversalGeographicCode
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the zone or county element.
        /// </summary>
        /// <value>The element.</value>
        [DataMember]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the purge time.
        /// </summary>
        /// <value>The purge time.</value>
        [DataMember]
        public DateTimeOffset PurgeTime { get; set; }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>The state.</value>
        [DataMember]
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>The type.</value>
        [DataMember]
        public char Type { get; set; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString() => $"Type: {Type} State: {State} Id: {Id} PurgeTime: {PurgeTime:g}";

        #endregion Public Methods
    }
}