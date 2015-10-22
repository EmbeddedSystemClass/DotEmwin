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

        protected ObjectCache BlockCache = new MemoryCache("BlockCache");

        protected ObjectCache DupeFilter = new MemoryCache("DupeFilter");

        #endregion Protected Fields

        #region Private Fields

        private readonly object _dummyObject = new object();

        private readonly Filters _filters;

        private readonly Action<WeatherProduct> _output;

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
                var segments = PersistSegment(packet);
                if (segments != null && segments.IsComplete())
                {
                    var product = ProcessSegments(segments);
                    BlockCache.Remove(packet.GetKey());

                    if (product != null)
                    {
                        var productKey = string.Concat(product.Filename, product.Hash);
                        if (DupeFilter.Contains(productKey))
                        {
                            PerformanceCounters.DuplicateProductsTotal.Increment();
                            ByteBlasterEventSource.Log.Info("Skipped duplicate product", product.ToString());
                        }
                        else
                        {
                            _output(product);
                            DupeFilter.Add(productKey, _dummyObject, DateTimeOffset.Now.Add(ExpireTime));
                        }
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
        /// Persists the quick block packet in the dictionary.
        /// </summary>
        /// <param name="segment">The packet.</param>
        /// <returns>IReadOnlyList&lt;QuickBlockTransferSegment&gt;.</returns>
        private IReadOnlyList<QuickBlockTransferSegment> PersistSegment(QuickBlockTransferSegment segment)
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

            // Create a new collection array and add to cache with initial block populated
            collection = new QuickBlockTransferSegment[segment.TotalBlocks];
            collection[segment.BlockNumber - 1] = segment;
            BlockCache.Set(key, collection, DateTimeOffset.Now.Add(ExpireTime));
            return collection;
        }

        /// <summary>
        /// Processes the segments into Weather Products.
        /// </summary>
        /// <param name="segments">The segments.</param>
        /// <returns>WeatherProduct.</returns>
        private WeatherProduct ProcessSegments(IReadOnlyList<QuickBlockTransferSegment> segments)
        {
            var firstSegment = segments.First(p => p != null);
            var isText = firstSegment.IsText();
            var content = segments.Select(b => b.Content).ToList().Combine(isText);
            var hash = content.ComputeHash();

            var product = new WeatherProduct
            {
                Filename = firstSegment.Filename,
                TimeStamp = firstSegment.TimeStamp,
                Content = content,
                Hash = hash,
                ReceivedAt = DateTimeOffset.UtcNow
            };

            if (product.IsCompressed())
                product = product.Unzip();

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