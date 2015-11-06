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
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Emwin.Core.Contracts;
using Emwin.Core.Parsers;
using Emwin.Core.Types;

namespace Emwin.Core.Products
{
    /// <summary>
    /// Class ProductFactory for creating products.
    /// </summary>
    public static class ProductFactory
    {

        #region Public Methods

        /// <summary>
        /// Converts bundle of segments to desired product type contract.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="segments">The segments.</param>
        /// <returns>T.</returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentException">@At least one segment is not complete (is null).</exception>
        /// <exception cref="System.InvalidCastException">Unable to convert to specified type.</exception>
        public static T ConvertTo<T>(IQuickBlockTransferSegment[] segments) where T : IEmwinContent
        {
            if (segments == null) throw new ArgumentNullException(nameof(segments));

            var lastSegment = segments[segments.Length - 1];
            var isText = typeof(T) == typeof(ITextProduct);
            var content = segments.Select(b => b.Content).ToList().Combine(isText);

            if (typeof(T) == typeof(ITextProduct))
                return (T)CreateTextProduct(lastSegment.Filename, lastSegment.TimeStamp, content, lastSegment.ReceivedAt);

            if (typeof(T) == typeof(IImageProduct))
                return (T)CreateImageProduct(lastSegment.Filename, lastSegment.TimeStamp, content, lastSegment.ReceivedAt);

            if (typeof(T) == typeof(ICompressedContent))
                return (T)CreateCompressedContent(lastSegment.Filename, lastSegment.TimeStamp, content, lastSegment.ReceivedAt);

            throw new InvalidCastException("Unable to convert to specified type");
        }

        /// <summary>
        /// Creates the bulletin product.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="timeStamp">The time stamp.</param>
        /// <param name="content">The content.</param>
        /// <param name="receivedAt">The received at.</param>
        /// <param name="header">The header.</param>
        /// <param name="seq">The seq.</param>
        /// <returns>ITextProduct.</returns>
        public static IBulletinProduct CreateBulletinProduct(string filename, DateTimeOffset timeStamp, string content, DateTimeOffset receivedAt, ICommsHeader header, int seq)
        {
            var product = new BulletinProduct
            {
                Filename = filename,
                TimeStamp = timeStamp,
                Content = content,
                ReceivedAt = receivedAt,
                Header = header,
                SequenceNumber = seq,
            };

            product.GeoCodes = UgcParser.ParseProduct(product);
            product.PrimaryVtecCodes = VtecParser.ParseProduct(product);
            product.Polygons = SpatialParser.ParseProduct(product).Select(SpatialParser.ConvertToWellKnownText);

            return product;
        }

        /// <summary>
        /// Creates the compressed.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="timeStamp">The time stamp.</param>
        /// <param name="content">The content.</param>
        /// <param name="receivedAt">The received at.</param>
        /// <returns>ICompressedContent.</returns>
        public static ICompressedContent CreateCompressedContent(string filename, DateTimeOffset timeStamp, byte[] content,
            DateTimeOffset receivedAt)
        {
            return new CompressedContent
            {
                Filename = filename,
                TimeStamp = timeStamp,
                Content = content,
                ReceivedAt = receivedAt
            };
        }

        /// <summary>
        /// Creates the image product.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="timeStamp">The time stamp.</param>
        /// <param name="content">The content.</param>
        /// <param name="receivedAt">The received at.</param>
        /// <returns>ITextProduct.</returns>
        public static IImageProduct CreateImageProduct(string filename, DateTimeOffset timeStamp, byte[] content, DateTimeOffset receivedAt)
        {
            var image = Image.FromStream(new MemoryStream(content));

            return new ImageProduct
            {
                Filename = filename,
                TimeStamp = timeStamp,
                Content = image,
                Height = image.Height,
                Width = image.Width,
                ReceivedAt = receivedAt,
                ContentType = ContentFileType.Image
            };
        }

        /// <summary>
        /// Creates the text product.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="timeStamp">The time stamp.</param>
        /// <param name="content">The content.</param>
        /// <param name="receivedAt">The received at.</param>
        /// <returns>ITextProduct.</returns>
        public static ITextProduct CreateTextProduct(string filename, DateTimeOffset timeStamp, byte[] content, DateTimeOffset receivedAt)
        {
            var count = Array.LastIndexOf(content, (byte)03); // Trim to ETX
            if (count < 0) count = content.Length;

            var product = new TextProduct
            {
                Filename = filename,
                TimeStamp = timeStamp,
                Content = Encoding.ASCII.GetString(content, 0, count),
                ReceivedAt = receivedAt,
            };

            product.Header = HeadingParser.ParseProduct(product);

            return product;
        }

        #endregion Public Methods

        #region Private Methods

        private static byte[] Combine(this IList<byte[]> arrays, bool trimLast = false)
        {
            if (trimLast)
            {
                var last = arrays[arrays.Count - 1];
                var pos = last.Length - 1;
                while (pos > 0 && last[pos] == 0) --pos;
                if (pos < last.Length)
                {
                    Array.Resize(ref last, pos + 1);
                    arrays[arrays.Count - 1] = last;
                }
            }

            var result = new byte[arrays.Sum(a => a.Length)];
            var offset = 0;
            foreach (var array in arrays)
            {
                Buffer.BlockCopy(array, 0, result, offset, array.Length);
                offset += array.Length;
            }

            return result;
        }

        #endregion Private Methods

    }
}
