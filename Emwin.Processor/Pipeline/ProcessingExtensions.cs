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
using System.Linq;
using System.Security.Cryptography;

namespace Emwin.Processor.Pipeline
{
    internal static class ProcessingExtensions
    {
        #region Public Methods

        /// <summary>
        /// Combines the given content arrays.
        /// </summary>
        /// <param name="arrays">The arrays to be combined.</param>
        /// <param name="trimLast">Specifies if the final array should be null trimmed.</param>
        /// <returns>Combined Byte array.</returns>
        public static byte[] Combine(this IList<byte[]> arrays, bool trimLast = false)
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
        public static string ComputeHash(this byte[] sourceBytes)
        {
            using (var hashAlg = SHA1.Create())
                return Convert.ToBase64String(hashAlg.ComputeHash(sourceBytes));
        }

        #endregion Public Methods

    }
}
