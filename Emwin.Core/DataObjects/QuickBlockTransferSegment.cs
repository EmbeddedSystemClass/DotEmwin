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
        /// Gets the content.
        /// </summary>
        /// <value>The content.</value>
        object IEmwinContent.Content => Content;

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
        public long ReceivedAt { get; set; }

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