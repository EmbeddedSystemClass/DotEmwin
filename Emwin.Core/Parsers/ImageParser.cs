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
