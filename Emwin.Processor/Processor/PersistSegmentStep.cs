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
using System.Runtime.Caching;
using Emwin.Core.Models;

namespace Emwin.Processor.Processor
{
    internal sealed class PersistSegmentStep
    {
        public static TimeSpan ExpireTime = TimeSpan.FromMinutes(2);

        private readonly ObjectCache _blockCache = new MemoryCache("_blockCache");

        public QuickBlockTransferSegment[] Execute(QuickBlockTransferSegment segment)
        {
            var key = segment.GetKey();
            QuickBlockTransferSegment[] collection;

            if (_blockCache.Contains(key))
            {
                collection = (QuickBlockTransferSegment[])_blockCache.Get(key);
                collection[segment.BlockNumber - 1] = segment;
                return collection;
            }

            // Create a new collection array and add to cache with initial block populated
            collection = new QuickBlockTransferSegment[segment.TotalBlocks];
            collection[segment.BlockNumber - 1] = segment;
            _blockCache.Set(key, collection, DateTimeOffset.Now.Add(ExpireTime));
            return collection;
        }
    }
}