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
using System.IO.Compression;
using Emwin.Core.Contracts;
using Emwin.Core.Parsers;
using Emwin.Core.Products;
using Emwin.Processor.EventAggregator;
using Emwin.Processor.Instrumentation;

namespace Emwin.Processor.Processor
{
    internal sealed class ZipExtractor : IHandle<ICompressedContent>
    {
        #region Public Methods

        /// <summary>
        /// This will be called every time a CompressedProduct is published through the event aggregator
        /// Unzips the product and returns the first contained product in the zip.
        /// Assumes a single product is contained inside the zip file.
        /// </summary>
        /// <param name="product">The product.</param>
        /// <param name="ctx">The CTX.</param>
        public void Handle(ICompressedContent product, IEventAggregator ctx)
        {
            try
            {
                UnZip(product, ctx);
            }
            catch (Exception ex)
            {
                ProcessorEventSource.Log.Error("ZipProcessor", ex.ToString());
            }
        }

        private static void UnZip(ICompressedContent product, IEventPublisher ctx)
        {
            using (var zip = new ZipArchive(product.GetStream(), ZipArchiveMode.Read))
            {
                foreach (var file in zip.Entries)
                {
                    using (var fileStream = file.Open())
                    {
                        var content = ReadAllBytes(fileStream);
                        var contentType = ContentTypeParser.GetFileContentType(file.Name);
                        switch (contentType)
                        {
                            case ContentFileType.Text:
                                var textProduct = TextProduct.Create(
                                    file.Name.ToUpperInvariant(), file.LastWriteTime, content, product.ReceivedAt, product.Source);
                                ctx.SendMessage(textProduct);
                                ProcessorEventSource.Log.Info("ZipProcessor", textProduct.ToString());
                                break;

                            case ContentFileType.Image:
                                var imageProduct = ImageProduct.Create(
                                    file.Name.ToUpperInvariant(), file.LastWriteTime, content, product.ReceivedAt, product.Source);
                                ctx.SendMessage(imageProduct);
                                ProcessorEventSource.Log.Info("ZipProcessor", imageProduct.ToString());
                                break;

                            default:
                                ProcessorEventSource.Log.Warning("ZipProcessor",
                                    "Unknown content file type: " + file.Name);
                                return;
                        }
                    }
                }
            }
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Reads all bytes.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>System.Byte[].</returns>
        private static byte[] ReadAllBytes(Stream stream)
        {
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }

        #endregion Private Methods
    }
}