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
    internal sealed class SegmentBundler : IHandle<QuickBlockTransferSegment>
    {

        #region Public Fields

        public static TimeSpan SegmentExpireTime = TimeSpan.FromMinutes(10);

        #endregion Public Fields

        #region Private Fields

        private readonly ObjectCache _blockCache = new MemoryCache("BlockCache");

        #endregion Private Fields

        #region Public Methods

        /// <summary>
        /// This will be called every time a QuickBlockTransferSegment is published through the event aggregator
        /// </summary>
        /// <param name="blockSegment">The segment.</param>
        /// <param name="ctx">The CTX.</param>
        public void Handle(QuickBlockTransferSegment blockSegment, IEventAggregator ctx)
        {
            var key = blockSegment.GetKey();
            QuickBlockTransferSegment[] bundle = null;

            // If there is already a bundle in the cache, put the segment into it. 
            if (_blockCache.Contains(key))
            {
                bundle = (QuickBlockTransferSegment[])_blockCache.Get(key);
                if (blockSegment.BlockNumber > 0 && blockSegment.BlockNumber <= bundle.Length)
                    bundle[blockSegment.BlockNumber - 1] = blockSegment;
                ProcessorEventSource.Log.Verbose("SegmentBundler",
                    $"Added segment {blockSegment.BlockNumber} of {blockSegment.TotalBlocks} to existing bundle {blockSegment.Filename}");
            }
            else if (blockSegment.BlockNumber == 1)
            {
                // Create a new bundle array with the initial segment and add to cache.
                bundle = new QuickBlockTransferSegment[blockSegment.TotalBlocks];
                bundle[blockSegment.BlockNumber - 1] = blockSegment;
                _blockCache.Set(key, bundle, DateTimeOffset.Now.Add(SegmentExpireTime));
                ProcessorEventSource.Log.Verbose("SegmentBundler",
                    $"Added segment {blockSegment.BlockNumber} of {blockSegment.TotalBlocks} to new bundle {blockSegment.Filename}");
            }

            // If all segments are complete, publish the bundle
            if (bundle != null && bundle.All(s => s != null))
            {
                ProcessorEventSource.Log.Info("SegmentBundler", 
                    $"Completed assembling {blockSegment.TotalBlocks} blocks for bundle {blockSegment.Filename}");
                ctx.SendMessage(bundle);
            }
        }

        #endregion Public Methods

    }
}