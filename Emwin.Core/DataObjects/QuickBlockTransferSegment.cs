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
using Emwin.Core.Interfaces;
using Emwin.Core.Parsers;
using Emwin.Core.Types;

namespace Emwin.Core.DataObjects
{
    /// <summary>
    /// Class QuickBlockTransferSegment. A method dividing messages into small pieces to allow the interruption of large, 
    /// low priority messages by messages of a more immediate nature. The use of this protocol insures timely notification 
    /// of impending severe weather events, even at a very low data rate.
    /// </summary>
    [DataContract]
    public sealed class QuickBlockTransferSegment : IEmwinContent<byte[]>
    {

        #region Public Properties

        /// <summary>
        /// Gets or sets the block number.
        /// </summary>
        /// <value>The block.</value>
        [DataMember]
        public int BlockNumber { get; set; }

        /// <summary>
        /// Gets or sets the checksum of the individual block.
        /// </summary>
        /// <value>The checksum.</value>
        [DataMember]
        public int Checksum { get; set; }

        /// <summary>
        /// Gets or sets the raw content of the block.
        /// </summary>
        /// <value>The content.</value>
        [DataMember]
        public byte[] Content { get; set; }

        /// <summary>
        /// Gets the type of the file the block is part of.
        /// </summary>
        /// <returns>WeatherProductFileType.</returns>
        [DataMember]
        public ContentFileType ContentType => ContentTypeParser.GetFileContentType(Filename);

        /// <summary>
        /// Gets or sets the filename of the file the block is part of.
        /// </summary>
        /// <value>The filename.</value>
        [DataMember]
        public string Filename { get; set; }

        /// <summary>
        /// Gets or sets the raw header.
        /// </summary>
        /// <value>The header.</value>
        public string Header { get; set; }

        /// <summary>
        /// Gets or sets the original (before uncompressing) length of the block.
        /// </summary>
        /// <value>The length.</value>
        [DataMember]
        public int Length { get; set; }

        /// <summary>
        /// Gets or sets the received at time for the block.
        /// </summary>
        /// <value>The received at.</value>
        [DataMember]
        public DateTimeOffset ReceivedAt { get; set; }

        /// <summary>
        /// Gets or sets the time stamp of the file the block is part of.
        /// </summary>
        /// <value>The time stamp.</value>
        [DataMember]
        public DateTimeOffset TimeStamp { get; set; }

        /// <summary>
        /// Gets or sets the total blocks for the file.
        /// </summary>
        /// <value>The total blocks.</value>
        [DataMember]
        public int TotalBlocks { get; set; }

        /// <summary>
        /// Gets or sets the version of the transfer protocol used.
        /// </summary>
        /// <value>The version.</value>
        [DataMember]
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
            $"[QuickBlockTransferSegment] Filename={Filename} Date={TimeStamp:g} Block#{BlockNumber}/{TotalBlocks} V{Version} Length={Length}";

        #endregion Public Methods

    }
}