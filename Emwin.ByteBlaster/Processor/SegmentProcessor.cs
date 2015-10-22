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
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using DotNetty.Transport.Channels;
using Emwin.ByteBlaster.Instrumentation;
using Emwin.ByteBlaster.Protocol;
using Emwin.Core.Models;

namespace Emwin.ByteBlaster.Processor
{
    /// <summary>
    /// Class QuickBlockPacketCombiner. Combines Quick Block Packets into complete collections.
    /// </summary>
    internal class SegmentProcessor : AsyncProcessingQueue<QuickBlockTransferSegment>
    {

        #region Public Fields

        public static TimeSpan ExpireTime = TimeSpan.FromMinutes(2);

        #endregion Public Fields

        #region Protected Fields

        /// <summary>
        /// The block cache
        /// </summary>
        protected ObjectCache BlockCache = new MemoryCache("BlockCache");

        /// <summary>
        /// The dupe cache
        /// </summary>
        protected ObjectCache DupeCache = new MemoryCache("DupeCache");

        #endregion Protected Fields

        #region Private Fields

        private readonly Filters _filters;
        private readonly Action<WeatherProduct> _output;
        private readonly object _dummyObject = new object();

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SegmentProcessor" /> class.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <param name="output">The output.</param>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public SegmentProcessor(Filters filters, Action<WeatherProduct> output)
        {
            if (filters == null) throw new ArgumentNullException(nameof(filters));
            if (output == null) throw new ArgumentNullException(nameof(output));

            _filters = filters;
            _output = output;
        }

        #endregion Public Constructors

        #region Protected Methods

        protected override Task ProcessAsync(IChannelHandlerContext context, QuickBlockTransferSegment packet)
        {
            try
            {
                var key = packet.GetKey();
                if (DupeCache.Contains(key))
                {
                    PerformanceCounters.DuplicateProductsTotal.Increment();
                    ByteBlasterEventSource.Log.Info("Skipped duplicate product", packet.ToString());
                    return Task.FromResult(false);
                }

                var blocks = PersistQuickBlockPacket(packet);
                if (blocks != null && IsComplete(blocks))
                {
                    BlockCache.Remove(key);
                    var product = ProcessSegments(blocks);
                    if (product != null)
                    {
                        _output(product);
                        DupeCache.Add(key, _dummyObject, DateTimeOffset.Now.Add(ExpireTime));
                    }
                }
            }
            catch (Exception ex)
            {
                PerformanceCounters.TransformerExceptionsTotal.Increment();
                ByteBlasterEventSource.Log.Error("Segment Processor", ex);
            }

            return Task.FromResult(true);
        }

        #endregion Protected Methods

        #region Private Methods

        /// <summary>
        /// Combines the given content arrays.
        /// </summary>
        /// <param name="arrays">The arrays to be combined.</param>
        /// <param name="trimLast">Specifies if the final array should be null trimmed.</param>
        /// <returns>Combined Byte array.</returns>
        private static byte[] CombineContent(IList<byte[]> arrays, bool trimLast)
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
        /// Determines whether the segment collection is complete.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <returns><c>true</c> if all blocks have been received; otherwise, <c>false</c>.</returns>
        private static bool IsComplete(IEnumerable<QuickBlockTransferSegment> collection) => collection.All(p => p != null);

        /// <summary>
        /// Unzips the product and returns the first contained product in the zip. 
        /// Assumes a single product is contained inside the zip file.
        /// </summary>
        /// <param name="product">The compressed product.</param>
        /// <returns>EmwinWeatherProduct.</returns>
        private static WeatherProduct UnzipProduct(WeatherProduct product)
        {
            using (var zip = new ZipArchive(product.GetStream(), ZipArchiveMode.Read))
            {
                var file = zip.Entries.First();
                using (var ms = new MemoryStream())
                using (var fileStream = file.Open())
                {
                    fileStream.CopyTo(ms);
                    return new WeatherProduct
                    {
                        Filename = file.Name.ToUpperInvariant(),
                        TimeStamp = file.LastWriteTime,
                        Content = ms.ToArray(),
                        ReceivedAt = DateTimeOffset.UtcNow
                    };
                }
            }
        }

        /// <summary>
        /// Persists the quick block packet in the dictionary.
        /// </summary>
        /// <param name="segment">The packet.</param>
        /// <returns>IReadOnlyList&lt;QuickBlockTransferSegment&gt;.</returns>
        private IReadOnlyList<QuickBlockTransferSegment> PersistQuickBlockPacket(QuickBlockTransferSegment segment)
        {
            var key = segment.GetKey();
            QuickBlockTransferSegment[] collection;

            // Check PreFilter
            if (_filters?.SegmentFilter != null && _filters.SegmentFilter(segment))
            {
                ByteBlasterEventSource.Log.Info("Segment Filtered", segment.ToString());
                PerformanceCounters.BlocksFilteredTotal.Increment();
                return null;
            }

            // If we have already started collecting blocks then add or replace in collection
            if (BlockCache.Contains(key))
            {
                collection = (QuickBlockTransferSegment[])BlockCache.Get(key);
                collection[segment.BlockNumber-1] = segment;
                return collection;
            }

            collection = new QuickBlockTransferSegment[segment.TotalBlocks];
            collection[segment.BlockNumber - 1] = segment;
            BlockCache.Set(key, collection, DateTimeOffset.Now.Add(ExpireTime));
            return collection;
        }

        /// <summary>
        /// Processes the segments into Emwin Weather Products.
        /// </summary>
        /// <param name="segments">The segments.</param>
        /// <returns>Emwin.ByteBlaster.Models.EmwinWeatherProduct.</returns>
        private WeatherProduct ProcessSegments(IReadOnlyList<QuickBlockTransferSegment> segments)
        {
            var firstSegment = segments.First(p => p != null);
            var isText = firstSegment.IsText();

            var product = new WeatherProduct
            {
                Filename = firstSegment.Filename,
                TimeStamp = firstSegment.TimeStamp,
                Content = CombineContent(segments.Select(b => b.Content).ToList(), isText),
                ReceivedAt = DateTimeOffset.UtcNow
            };

            if (product.IsCompressed())
                product = UnzipProduct(product);

            // Check PreFilter
            if (_filters?.ProductFilter != null && _filters.ProductFilter(product))
            {
                ByteBlasterEventSource.Log.Info("Product Filtered", product.ToString());
                PerformanceCounters.ProductsFilteredTotal.Increment();
                return null;
            }

            ByteBlasterEventSource.Log.Verbose("Product", product.ToString());
            PerformanceCounters.ProductsCreatedTotal.Increment();
            return product;
        }

        #endregion Private Methods

    }
}