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

namespace Emwin.Core.Models
{
    /// <summary>
    /// Class QuickBlockPacket.
    /// </summary>
    public class QuickBlockTransferSegment : AbstractContent
    {

        #region Public Properties

        /// <summary>
        /// Gets or sets the block.
        /// </summary>
        /// <value>The block.</value>
        public int BlockNumber { get; set; }

        /// <summary>
        /// Gets or sets the checksum.
        /// </summary>
        /// <value>The checksum.</value>
        public int Checksum { get; set; }

        /// <summary>
        /// Gets or sets the original (before uncompressing) length.
        /// </summary>
        /// <value>The length.</value>
        public int Length { get; set; }

        /// <summary>
        /// Gets or sets the total blocks.
        /// </summary>
        /// <value>The total blocks.</value>
        public int TotalBlocks { get; set; }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>The version.</value>
        public byte Version { get; set; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Gets the block key (filename plus time stamp).
        /// </summary>
        /// <returns>System.String.</returns>
        public string GetKey() => string.Concat(Filename, TimeStamp.ToString("s"));

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString() =>
            $"{Filename} Date: {TimeStamp:g} Block #{BlockNumber}/{TotalBlocks} V{Version} Length={Length}";

        #endregion Public Methods
    }
}