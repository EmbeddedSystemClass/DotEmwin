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
using System.Runtime.Caching;
using Emwin.Core.DataObjects;
using Emwin.Core.EventAggregator;
using Emwin.Processor.Instrumentation;

namespace Emwin.Processor.Processor
{
    /// <summary>
    /// Class SegmentBundler. Uses a Memory Cache to bundle the segments of a file together.
    /// </summary>
    internal sealed class SegmentBundler : IListener<QuickBlockTransferSegment>
    {

        #region Private Fields

        private readonly ObjectCache _blockCache = new MemoryCache("BlockCache");
        private readonly IEventPublisher _publisher;
        private readonly TimeSpan _expireTime;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SegmentBundler" /> class.
        /// </summary>
        /// <param name="publisher">The publisher.</param>
        /// <param name="expireTime">The expire time.</param>
        public SegmentBundler(IEventPublisher publisher, TimeSpan? expireTime = null)
        {
            _publisher = publisher;
            _expireTime = expireTime ?? TimeSpan.FromMinutes(10);
        }

        #endregion Public Constructors

        #region Public Methods

        /// <summary>
        /// This will be called every time a QuickBlockTransferSegment is published through the event aggregator
        /// </summary>
        /// <param name="blockSegment">The segment.</param>
        public void Handle(QuickBlockTransferSegment blockSegment)
        {
            var key = blockSegment.GetKey();
            QuickBlockTransferSegment[] bundle;

            // If there is already a bundle in the cache, put the segment into it. 
            if (_blockCache.Contains(key))
            {
                bundle = (QuickBlockTransferSegment[])_blockCache.Get(key);
                if (blockSegment.BlockNumber > 0 && blockSegment.BlockNumber <= bundle.Length)
                    bundle[blockSegment.BlockNumber - 1] = blockSegment;
                ProcessorEventSource.Log.Verbose("SegmentBundler",
                    $"Added segment {blockSegment.BlockNumber} of {blockSegment.TotalBlocks} to existing bundle {blockSegment.Filename}");
            }
            else
            {
                // Create a new bundle array with the initial segment and add to cache.
                bundle = new QuickBlockTransferSegment[blockSegment.TotalBlocks];
                bundle[blockSegment.BlockNumber - 1] = blockSegment;
                _blockCache.Set(key, bundle, DateTimeOffset.Now.Add(_expireTime));
                ProcessorEventSource.Log.Verbose("SegmentBundler",
                    $"Added segment {blockSegment.BlockNumber} of {blockSegment.TotalBlocks} to new bundle {blockSegment.Filename}");
            }

            // If all segments are complete, publish the bundle
            if (bundle.All(s => s != null))
            {
                ProcessorEventSource.Log.Info("SegmentBundler", 
                    $"Completed assembling {blockSegment.TotalBlocks} blocks for bundle {blockSegment.Filename}");
                _publisher.SendMessage(bundle);
            }
        }

        #endregion Public Methods
    }
}