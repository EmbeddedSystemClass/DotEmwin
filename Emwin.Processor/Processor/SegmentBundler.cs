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
using System.Threading.Tasks.Dataflow;
using Emwin.Core.Models;

namespace Emwin.Processor.Processor
{
    /// <summary>
    /// Class SegmentBundler. Uses a Memory Cache to bundle the segments of a file together.
    /// </summary>
    internal sealed class SegmentBundler
    {
        #region Public Fields

        public static TimeSpan ExpireTime = TimeSpan.FromMinutes(2);

        #endregion Public Fields

        #region Private Fields

        private readonly ObjectCache _blockCache = new MemoryCache("BlockCache");

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SegmentBundler"/> class.
        /// </summary>
        public SegmentBundler()
        {
            Block = new TransformBlock<QuickBlockTransferSegment, QuickBlockTransferSegment[]>(x => Execute(x));
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// Gets the block.
        /// </summary>
        /// <value>The block.</value>
        public TransformBlock<QuickBlockTransferSegment, QuickBlockTransferSegment[]> Block { get; }

        #endregion Public Properties

        #region Private Methods

        private QuickBlockTransferSegment[] Execute(QuickBlockTransferSegment segment)
        {
            var key = segment.GetKey();
            QuickBlockTransferSegment[] bundle;

            // If there is already a bundle in the cache, put the segment into it. 
            if (_blockCache.Contains(key))
            {
                bundle = (QuickBlockTransferSegment[])_blockCache.Get(key);
                if (segment.BlockNumber > 0 && segment.BlockNumber <= bundle.Length)
                    bundle[segment.BlockNumber - 1] = segment;
                return bundle;
            }

            // Create a new bundle array with the initial segment and add to cache.
            bundle = new QuickBlockTransferSegment[segment.TotalBlocks];
            bundle[segment.BlockNumber - 1] = segment;
            _blockCache.Set(key, bundle, DateTimeOffset.Now.Add(ExpireTime));
            return bundle;
        }

        #endregion Private Methods
    }
}