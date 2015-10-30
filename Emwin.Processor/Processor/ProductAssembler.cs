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
using System.Linq;
using Emwin.Core.DataObjects;
using Emwin.Core.EventAggregator;
using Emwin.Processor.Instrumentation;
using Emwin.Core.Products;
using Emwin.Core.Types;

namespace Emwin.Processor.Processor
{
    /// <summary>
    /// Class ProductAssembler. Assembles bundles of segments into a product by combining all the bytes from each segment.
    /// </summary>
    internal sealed class ProductAssembler : IHandle<QuickBlockTransferSegment[]>
    {
        /// <summary>
        /// This will be called every time a QuickBlockTransferSegment[] bundle is published through the event aggregator
        /// </summary>
        /// <param name="bundle">The bundle.</param>
        /// <param name="ctx">The CTX.</param>
        public void Handle(QuickBlockTransferSegment[] bundle, IEventAggregator ctx)
        {
            try
            {
                var contentType = bundle.First(x => x != null).ContentType;
                switch (contentType)
                {
                    case ContentFileType.Text:
                        var textProduct = ProductFactory.CreateTextProduct(bundle);
                        ctx.SendMessage(textProduct);
                        ProcessorEventSource.Log.Info("ProductAssembler", textProduct.ToString());
                        break;

                    case ContentFileType.Image:
                        var imageProduct = ProductFactory.CreateImageProduct(bundle);
                        ctx.SendMessage(imageProduct);
                        ProcessorEventSource.Log.Info("ProductAssembler", imageProduct.ToString());
                        break;

                    case ContentFileType.Compressed:
                        var compressedProduct = ProductFactory.CreateCompressedContent(bundle);
                        ctx.SendMessage(compressedProduct);
                        ProcessorEventSource.Log.Info("ProductAssembler", compressedProduct.ToString());
                        break;

                    default:
                        ProcessorEventSource.Log.Warning("ProductAssembler", "Unknown content file type: " + contentType);
                        return;
                };

                PerformanceCounters.ProductsCreatedTotal.Increment();
            }
            catch (Exception ex)
            {
                ProcessorEventSource.Log.Error("ProductAssembler", ex.ToString());
            }
        }
    }
}