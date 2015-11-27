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
using System.Linq;
using Emwin.Core.Contracts;

namespace Emwin.Core.Products
{
    /// <summary>
    /// Class ProductFactory for creating products.
    /// </summary>
    public static class ProductBuilderExtension
    {

        #region Public Methods

        /// <summary>
        /// Converts bundle of segments to desired product type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="segments">The segments.</param>
        /// <returns>T.</returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentException">@At least one segment is not complete (is null).</exception>
        /// <exception cref="System.InvalidCastException">Unable to convert to specified type.</exception>
        public static T BuildProduct<T>(this IQuickBlockTransferSegment[] segments) where T : IEmwinContent
        {
            if (segments == null) throw new ArgumentNullException(nameof(segments));

            var lastSegment = segments[segments.Length - 1];
            var isText = typeof(T) == typeof(ITextProduct);
            var content = segments.Select(b => b.Content).ToList().Combine(isText);

            if (typeof(T) == typeof(ITextProduct))
                return (T)TextProduct.Create(lastSegment.Filename, lastSegment.TimeStamp, content, lastSegment.ReceivedAt, lastSegment.Source);

            if (typeof(T) == typeof(IImageProduct))
                return (T)ImageProduct.Create(lastSegment.Filename, lastSegment.TimeStamp, content, lastSegment.ReceivedAt, lastSegment.Source);

            if (typeof(T) == typeof(ICompressedContent))
                return (T) CompressedContent.Create(lastSegment.Filename, lastSegment.TimeStamp, content, lastSegment.ReceivedAt, lastSegment.Source);

                throw new InvalidCastException("Unable to convert to specified type");
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
