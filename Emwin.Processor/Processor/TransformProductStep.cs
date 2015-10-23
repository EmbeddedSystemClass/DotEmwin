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
using Emwin.Core.Models;
using Emwin.Processor.Instrumentation;
using Emwin.Processor.Pipeline;

namespace Emwin.Processor.Processor
{
    internal sealed class TransformProductStep
    {
        public static TimeSpan ExpireTime = TimeSpan.FromMinutes(2);

        private readonly object _dummyObject = new object();

        private readonly ObjectCache _dupeFilter = new MemoryCache("_dupeFilter");

        public IEnumerable<WeatherProduct> Execute(QuickBlockTransferSegment[] segments)
        {
            if (!IsComplete(segments)) yield break;

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

            ProcessorEventSource.Log.Verbose("Product", product.ToString());
            PerformanceCounters.ProductsCreatedTotal.Increment();

            var productKey = string.Concat(product.Filename, product.Hash);
            if (_dupeFilter.Contains(productKey))
            {
                PerformanceCounters.DuplicateProductsTotal.Increment();
                ProcessorEventSource.Log.Info("Skipped duplicate product", product.ToString());
                yield break;
            }

            _dupeFilter.Add(productKey, _dummyObject, DateTimeOffset.Now.Add(ExpireTime));
            yield return product;
        }

        /// <summary>
        /// Determines whether the segment collection is complete.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <returns><c>true</c> if all blocks have been received; otherwise, <c>false</c>.</returns>
        private static bool IsComplete(IEnumerable<QuickBlockTransferSegment> collection) => collection.All(p => p != null);
    }
}