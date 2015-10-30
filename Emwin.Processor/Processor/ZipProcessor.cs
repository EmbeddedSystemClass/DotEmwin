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
using System.IO;
using System.IO.Compression;
using System.Linq;
using Emwin.Core.EventAggregator;
using Emwin.Core.Parsers;
using Emwin.Core.Products;
using Emwin.Core.Types;
using Emwin.Processor.Instrumentation;

namespace Emwin.Processor.Processor
{
    internal sealed class ZipProcessor : IHandle<CompressedProduct>
    {
        #region Public Methods

        /// <summary>
        /// This will be called every time a CompressedProduct is published through the event aggregator
        /// Unzips the product and returns the first contained product in the zip.
        /// Assumes a single product is contained inside the zip file.
        /// </summary>
        /// <param name="product">The product.</param>
        /// <param name="ctx">The CTX.</param>
        public void Handle(CompressedProduct product, IEventAggregator ctx)
        {
            try
            {
                UnZip(product, ctx);
                ProcessorEventSource.Log.Info("ZipProcessor", "Completed unzipping " + product.Filename);
            }
            catch (Exception ex)
            {
                ProcessorEventSource.Log.Error("ZipProcessor", ex.ToString());
            }
        }

        private static void UnZip(CompressedProduct product, IEventPublisher ctx)
        {
            using (var zip = new ZipArchive(product.GetStream(), ZipArchiveMode.Read))
            {
                var file = zip.Entries.First();
                using (var fileStream = file.Open())
                {
                    var content = ReadAllBytes(fileStream);
                    var contentType = ContentTypeParser.GetFileContentType(file.Name);
                    switch (contentType)
                    {
                        case ContentFileType.Text:
                            var textProduct = ProductFactory.CreateTextProduct(
                                file.Name.ToUpperInvariant(),file.LastWriteTime, content, DateTimeOffset.UtcNow);
                            ctx.SendMessage(textProduct);
                            ProcessorEventSource.Log.Info("ZipProcessor", textProduct.ToString());
                            break;

                        case ContentFileType.Image:
                            var imageProduct = ProductFactory.CreateImageProduct(
                                file.Name.ToUpperInvariant(), file.LastWriteTime, content, DateTimeOffset.UtcNow);
                            ctx.SendMessage(imageProduct);
                            ProcessorEventSource.Log.Info("ZipProcessor", imageProduct.ToString());
                            break;

                        default:
                            ProcessorEventSource.Log.Warning("ZipProcessor", "Unknown content file type: " + file.Name);
                            return;
                    }
                }
            }
        }

        #endregion Public Methods

        #region Private Methods

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