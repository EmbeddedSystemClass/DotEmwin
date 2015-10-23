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

using System.IO;
using System.Runtime.Serialization;
using Emwin.Core.References;

namespace Emwin.Core.Models
{
    /// <summary>
    /// WeatherProduct represents a complete weather product received from the ByteBlaster server.
    /// </summary>
    [DataContract]
    public class WeatherProduct : AbstractContent
    {

        #region Public Properties

        /// <summary>
        /// Gets the description if available.
        /// </summary>
        /// <value>The description.</value>
        [IgnoreDataMember]
        public string Description
        {
            get
            {
                switch (ContentType)
                {
                    case FileContent.Image:
                        return GraphicProduct.ResourceManager.GetString(ProductName);

                    default:
                        return TextProduct.ResourceManager.GetString(ProductCode);
                }
            }
        }

        /// <summary>
        /// Gets or sets the hash of the content.
        /// </summary>
        /// <value>The hash.</value>
        [DataMember]
        public string Hash { get; set; }

        /// <summary>
        /// Gets the product code from the filename.
        /// </summary>
        /// <value>The product code.</value>
        [IgnoreDataMember]
        public string ProductCode => Filename?.Substring(0, 3);

        /// <summary>
        /// Gets the name of the product from the filename.
        /// </summary>
        /// <value>The name of the product.</value>
        [IgnoreDataMember]
        public string ProductName => Path.GetFileNameWithoutExtension(Filename);

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString() =>
            $"{Description} ({Filename}) Date: {TimeStamp.ToString("g")} Size: {Content.Length:N0} Hash: {Hash}";

        #endregion Public Methods

    }
}