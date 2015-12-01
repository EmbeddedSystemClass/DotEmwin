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
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using Emwin.Core.Contracts;
using Emwin.Core.DataObjects;

namespace Emwin.Core.Products
{
    /// <summary>
    /// Class TextProduct. Represents a received text file.
    /// </summary>
    [DataContract]
    public class TextProduct : IEmwinContent<TextContent>
    {

        #region Public Properties

        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        /// <value>The content.</value>
        [DataMember]
        public TextContent Content { get; set; }

        /// <summary>
        /// Gets the filename.
        /// </summary>
        /// <value>The filename.</value>
        [DataMember]
        public string Filename { get; set; }

        /// <summary>
        /// Gets the content.
        /// </summary>
        /// <value>The content.</value>
        object IEmwinContent.Content => Content;

        /// <summary>
        /// Gets the received at time.
        /// </summary>
        /// <value>The received at.</value>
        [DataMember]
        public DateTimeOffset ReceivedAt { get; set; }

        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        /// <value>The source.</value>
        [DataMember]
        public string Source { get; set; }

        /// <summary>
        /// Gets the content time stamp.
        /// </summary>
        /// <value>The time stamp.</value>
        [DataMember]
        public DateTimeOffset TimeStamp { get; set; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Creates the text product.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="timeStamp">The time stamp.</param>
        /// <param name="content">The content.</param>
        /// <param name="receivedAt">The received at.</param>
        /// <param name="source">The source.</param>
        /// <returns>Emwin.Core.Contracts.TextProduct.</returns>
        public static TextProduct Create(string filename, DateTimeOffset timeStamp, byte[] content, DateTimeOffset receivedAt, string source)
        {
            var count = Array.LastIndexOf(content, (byte)03); // Trim to ETX
            if (count < 0) count = content.Length;
            var text = Encoding.ASCII
                .GetString(content, 0, count)
                .Replace("\r\r\n", "\r\n"); // Normalize double <CR> with single <CR>

            return new TextProduct
            {
                Filename = filename,
                TimeStamp = timeStamp,
                ReceivedAt = receivedAt,
                Source = source,
                Content = new TextContent(text)
            };
        }

        /// <summary>
        /// Gets the body string reader.
        /// </summary>
        /// <returns>System.IO.StringReader.</returns>
        public StringReader GetBodyReader() => new StringReader(Content.Body);

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString() =>
            $"[{nameof(TextProduct)}] Filename={Filename} Date={TimeStamp:g} Header={Content.Header.Replace("\r\n", " ")}";

        #endregion Public Methods
    }
}