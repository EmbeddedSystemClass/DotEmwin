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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Emwin.Core.DataObjects;
using Emwin.Core.Interfaces;
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
            var product = new CompressedProduct
            {
                Filename = filename,
                TimeStamp = timeStamp,
                Content = content,
                Hash = content.ComputeHash(),
                ReceivedAt = DateTimeOffset.UtcNow
            };

            return product;
        }

        /// <summary>
        /// Creates compressed content from a completed bundle of segments.
        /// </summary>
        /// <param name="segments">The segments.</param>
        /// <returns>ICompressedContent.</returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static ICompressedContent CreateCompressedContent(QuickBlockTransferSegment[] segments)
        {
            if (segments == null) throw new ArgumentNullException(nameof(segments));

            // Ensure all the segments have been completed.
            if (segments.Any(p => p == null))
                throw new ArgumentException("At least one segment is not complete (is null).", nameof(segments));

            var firstSegment = segments.First(p => p != null);
            var isText = firstSegment.ContentType == ContentFileType.Text;
            var content = segments.Select(b => b.Content).ToList().Combine(isText);

            return CreateCompressedContent(firstSegment.Filename, firstSegment.TimeStamp, content, DateTimeOffset.UtcNow);
        }

        /// <summary>
        /// Creates the image product from a completed bundle of segments.
        /// </summary>
        /// <param name="segments">The segments.</param>
        /// <returns>ImageProduct.</returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static IImageProduct CreateImageProduct(QuickBlockTransferSegment[] segments)
        {
            if (segments == null) throw new ArgumentNullException(nameof(segments));

            // Ensure all the segments have been completed.
            if (segments.Any(p => p == null))
                throw new ArgumentException("At least one segment is not complete (is null).", nameof(segments));

            var firstSegment = segments.First(p => p != null);
            var isText = firstSegment.ContentType == ContentFileType.Text;
            var content = segments.Select(b => b.Content).ToList().Combine(isText);

            return CreateImageProduct(firstSegment.Filename, firstSegment.TimeStamp, content, DateTimeOffset.UtcNow);
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

            var product = new ImageProduct
            {
                Filename = filename,
                TimeStamp = timeStamp,
                Content = image,
                Height = image.Height,
                Width = image.Width,
                Hash = content.ComputeHash(),
                ReceivedAt = receivedAt
            };

            return product;
        }

        /// <summary>
        /// Creates the text product from a completed bundle of segments.
        /// </summary>
        /// <returns>TextProduct.</returns>
        public static ITextProduct CreateTextProduct(QuickBlockTransferSegment[] segments)
        {
            if (segments == null) throw new ArgumentNullException(nameof(segments));

            // Ensure all the segments have been completed.
            if (segments.Any(p => p == null))
                throw new ArgumentException("At least one segment is not complete (is null).", nameof(segments));

            var firstSegment = segments.First(p => p != null);
            var isText = firstSegment.ContentType == ContentFileType.Text;
            var content = segments.Select(b => b.Content).ToList().Combine(isText);

            return CreateTextProduct(firstSegment.Filename, firstSegment.TimeStamp, content, DateTimeOffset.UtcNow);
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
            var product = new TextProduct
            {
                Filename = filename,
                TimeStamp = timeStamp,
                Content = Encoding.ASCII.GetString(content),
                Hash = content.ComputeHash(),
                ReceivedAt = receivedAt
            };

            product.Header = HeadingParser.ParseProduct(product);
            product.GeoCodes = UgcParser.ParseProduct(product);
            product.VtecCodes = VtecParser.ParseProduct(product);
            product.Polygons = SpatialParser.ParseProduct(product).Select(SpatialParser.ConvertToWellKnownText);

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

        /// <summary>
        /// Computes the hash of the source bytes and returns as Base 64 string.
        /// </summary>
        /// <param name="sourceBytes">The source bytes.</param>
        /// <returns>System.String.</returns>
        private static string ComputeHash(this byte[] sourceBytes)
        {
            using (var hashAlg = SHA1.Create())
                return Convert.ToBase64String(hashAlg.ComputeHash(sourceBytes));
        }

        #endregion Private Methods

    }
}
