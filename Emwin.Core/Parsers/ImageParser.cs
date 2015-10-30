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

using System.Linq;
using System.Text;
using Emwin.Core.Contracts;
using Emwin.Core.Types;

namespace Emwin.Core.Parsers
{
    public static class ImageParser
    {
        #region Private Fields

        // see http://www.mikekunz.com/image_file_header.html
        private static readonly byte[] BmpFormat = Encoding.ASCII.GetBytes("BM");

        private static readonly byte[] GifFormat = Encoding.ASCII.GetBytes("GIF");

        private static readonly byte[] JPeg2Format = { 255, 216, 255, 225 };

        private static readonly byte[] JPegFormat = { 255, 216, 255, 224 };

        private static readonly byte[] PngFormat = { 137, 80, 78, 71 };

        private static readonly byte[] Tiff2Format = { 77, 77, 42 };

        private static readonly byte[] TiffFormat = { 73, 73, 42 };

        #endregion Private Fields

        #region Public Methods

        /// <summary>
        /// Gets the image format.
        /// </summary>
        /// <param name="product">The product.</param>
        /// <returns>ImageFormat.</returns>
        public static ImageFormatType GetImageFormat(IEmwinContent<byte[]> product)
        {
            var imageBytes = product.Content;

            if (GifFormat.SequenceEqual(imageBytes.Take(GifFormat.Length)))
                return ImageFormatType.Gif;

            if (PngFormat.SequenceEqual(imageBytes.Take(PngFormat.Length)))
                return ImageFormatType.Png;

            if (JPegFormat.SequenceEqual(imageBytes.Take(JPegFormat.Length)))
                return ImageFormatType.JPeg;

            if (JPeg2Format.SequenceEqual(imageBytes.Take(JPeg2Format.Length)))
                return ImageFormatType.JPeg;

            if (BmpFormat.SequenceEqual(imageBytes.Take(BmpFormat.Length)))
                return ImageFormatType.Bmp;

            if (TiffFormat.SequenceEqual(imageBytes.Take(TiffFormat.Length)))
                return ImageFormatType.Tiff;

            if (Tiff2Format.SequenceEqual(imageBytes.Take(Tiff2Format.Length)))
                return ImageFormatType.Tiff;

            return ImageFormatType.Unknown;
        }

        #endregion Public Methods
    }
}
