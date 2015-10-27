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
using System.Threading.Tasks.Dataflow;
using Emwin.Core.Interfaces;
using Emwin.Core.Models;
using Emwin.Processor.Processor;

namespace Emwin.Processor.Pipeline
{
    /// <summary>
    /// Class ProcessingPipeline.
    /// </summary>
    internal sealed class PipelineBuilder
    {
        #region Private Fields

        /// <summary>
        /// The Data Flow Link Options.
        /// </summary>
        private static readonly DataflowLinkOptions LinkOptions = new DataflowLinkOptions { PropagateCompletion = true };

        /// <summary>
        /// Generic null block for terminating a pipeline path properly.
        /// </summary>
        private static readonly ITargetBlock<object> Terminator = DataflowBlock.NullTarget<object>();

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PipelineBuilder" /> class.
        /// </summary>
        /// <param name="filters">The filters.</param>
        public PipelineBuilder(PipelineFilters filters)
        {
            var segmentBundler = new SegmentBundler();
            var productAssembler = new ProductAssembler();
            var unzipProduct = new ZipProcessor();
            var productFilter = new ProductFilter(filters);
            var textOutput = new TransformBlock<IEmwinContent, ITextProduct>(p => (ITextProduct)p);
            var imageOutput = new TransformBlock<IEmwinContent, IImageProduct>(p => (IImageProduct)p);

            // Create external observer to receive segments
            SegmentObserver = segmentBundler.Block.AsObserver();

            // Link to the product assembler when the bundle is complete
            segmentBundler.Block.LinkTo(productAssembler.Block, LinkOptions, productAssembler.Predicate);

            // If the bundle is not complete yet, end the pipeline     
            segmentBundler.Block.LinkTo(Terminator, LinkOptions);

            // Link to unzip compressed products if product is compressed
            productAssembler.Block.LinkTo(unzipProduct.Block, LinkOptions, unzipProduct.Predicate);

            // Also link the zip processor to the product filter
            unzipProduct.Block.LinkTo(productFilter.Block, LinkOptions);

            // If the product is not compressed, send to the product filter
            productAssembler.Block.LinkTo(productFilter.Block, LinkOptions);

            // Link the product filter to the appropriate output based on type
            productFilter.Block.LinkTo(textOutput, LinkOptions, o => o is ITextProduct);
            productFilter.Block.LinkTo(imageOutput, LinkOptions, o => o is IImageProduct);
            productFilter.Block.LinkTo(Terminator, LinkOptions);

            // Create observable output
            TextProductObservable = textOutput.AsObservable();
            ImageProductObservable = imageOutput.AsObservable();
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// Gets the image product observable.
        /// </summary>
        /// <value>The image product observable.</value>
        public IObservable<IImageProduct> ImageProductObservable { get; }

        /// <summary>
        /// Gets the observer.
        /// </summary>
        /// <value>The observer.</value>
        public IObserver<QuickBlockTransferSegment> SegmentObserver { get; }

        /// <summary>
        /// Gets the text product observable.
        /// </summary>
        /// <value>The observable.</value>
        public IObservable<ITextProduct> TextProductObservable { get; }

        #endregion Public Properties
    }
}
