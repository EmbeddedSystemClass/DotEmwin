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
using System.Collections.Generic;
using Emwin.Core.Interfaces;
using Emwin.Core.Models;
using Emwin.Core.Types;

namespace Emwin.Core.Products
{
    /// <summary>
    /// Class TextProduct. Represents a received text file.
    /// </summary>
    public class TextProduct : ITextProduct
    {

        #region Public Properties

        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        /// <value>The content.</value>
        public string Content { get; set; }

        /// <summary>
        /// Gets the type of the content.
        /// </summary>
        /// <value>The type of the content.</value>
        public ContentFileType ContentType { get; set; }

        /// <summary>
        /// Gets the filename.
        /// </summary>
        /// <value>The filename.</value>
        public string Filename { get; set; }

        /// <summary>
        /// Gets or sets the geo codes.
        /// </summary>
        /// <value>The geo codes.</value>
        public IEnumerable<UniversalGeographicCode> GeoCodes { get; set; } 

        /// <summary>
        /// Gets or sets the hash.
        /// </summary>
        /// <value>The hash.</value>
        public string Hash { get; set; }

        /// <summary>
        /// Gets or sets the header.
        /// </summary>
        /// <value>The header.</value>
        public CommsHeader Header { get; set; }

        /// <summary>
        /// Gets the received at time.
        /// </summary>
        /// <value>The received at.</value>
        public DateTimeOffset ReceivedAt { get; set; }

        /// <summary>
        /// Gets the content time stamp.
        /// </summary>
        /// <value>The time stamp.</value>
        public DateTimeOffset TimeStamp { get; set; }

        /// <summary>
        /// Gets or sets the polygons.
        /// </summary>
        /// <value>The polygons.</value>
        public IEnumerable<string> Polygons { get; set; } 

        /// <summary>
        /// Gets any vtec codes.
        /// </summary>
        /// <value>The vtec codes.</value>
        public IEnumerable<ValidTimeEventCode> VtecCodes { get; set; } 

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString() =>
            $"[TextProduct] Filename={Filename} Date={TimeStamp:g} {Header}";

        #endregion Public Methods
    }
}