﻿/*
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
using System.Text;
using Emwin.Core.Contracts;
using Emwin.Core.Parsers;

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
        /// Gets the filename.
        /// </summary>
        /// <value>The filename.</value>
        public string Filename { get; set; }

        /// <summary>
        /// Gets or sets the header.
        /// </summary>
        /// <value>The header.</value>
        public ICommsHeader Header { get; set; }

        /// <summary>
        /// Gets the content.
        /// </summary>
        /// <value>The content.</value>
        object IEmwinContent.Content => Content;
        /// <summary>
        /// Gets the received at time.
        /// </summary>
        /// <value>The received at.</value>
        public DateTimeOffset ReceivedAt { get; set; }

        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        /// <value>The source.</value>
        public string Source { get; set; }

        /// <summary>
        /// Gets the content time stamp.
        /// </summary>
        /// <value>The time stamp.</value>
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
        /// <returns>Emwin.Core.Contracts.ITextProduct.</returns>
        public static ITextProduct Create(string filename, DateTimeOffset timeStamp, byte[] content, DateTimeOffset receivedAt, string source)
        {
            var count = Array.LastIndexOf(content, (byte)03); // Trim to ETX
            if (count < 0) count = content.Length;

            var product = new TextProduct
            {
                Filename = filename,
                TimeStamp = timeStamp,
                Content = Encoding.ASCII.GetString(content, 0, count),
                ReceivedAt = receivedAt,
                Source = source
            };

            product.Header = HeadingParser.ParseProduct(product);

            return product;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString() =>
            $"[TextProduct] Filename={Filename} Date={TimeStamp:g} {Header}";

        #endregion Public Methods
    }
}