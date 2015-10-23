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
using Emwin.Core.Models;
using Emwin.Processor.Processor;

namespace Emwin.Processor.Pipeline
{
    /// <summary>
    /// Class ProcessingPipeline.
    /// </summary>
    internal sealed class ProcessingPipeline
    {

        #region Private Fields

        /// <summary>
        /// Gets the target.
        /// </summary>
        /// <value>The target.</value>
        private readonly ITargetBlock<QuickBlockTransferSegment> _rootBlock;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingPipeline"/> class.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <param name="outAction">The out action.</param>
        public ProcessingPipeline(Filters filters, Action<WeatherProduct> outAction)
        {
            var filterSegmentStep = new SegmentFilter(filters);
            var persistSegmentStep = new PersistSegmentStep();
            var transformProductStep = new TransformProductStep();
            var filterProductStep = new ProductFilter(filters);
            var unzipProductStep = new ZipProcessor();

            var filterSegmentBlock = new TransformManyBlock<QuickBlockTransferSegment, QuickBlockTransferSegment>(x => filterSegmentStep.Execute(x));
            var persistSegmentBlock = new TransformBlock<QuickBlockTransferSegment, QuickBlockTransferSegment[]>(x => persistSegmentStep.Execute(x));
            var transformProductBlock = new TransformManyBlock<QuickBlockTransferSegment[], WeatherProduct>(x => transformProductStep.Execute(x));
            var filterProductBlock = new TransformManyBlock<WeatherProduct, WeatherProduct>(x => filterProductStep.Execute(x));
            var unzipProductBlock = new TransformBlock<WeatherProduct, WeatherProduct>(
                x => unzipProductStep.Execute(x),
                new ExecutionDataflowBlockOptions {MaxDegreeOfParallelism = Environment.ProcessorCount});
            var outputProductBlock = new ActionBlock<WeatherProduct>(outAction);

            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };
            filterSegmentBlock.LinkTo(persistSegmentBlock, linkOptions);
            persistSegmentBlock.LinkTo(transformProductBlock, linkOptions);
            transformProductBlock.LinkTo(filterProductBlock, linkOptions);
            filterProductBlock.LinkTo(unzipProductBlock, linkOptions);
            unzipProductBlock.LinkTo(outputProductBlock, linkOptions);

            _rootBlock = filterSegmentBlock;
        }

        #endregion Public Constructors

        #region Public Methods

        /// <summary>
        /// Completes this instance.
        /// </summary>
        public void Complete() => _rootBlock.Complete();

        /// <summary>
        /// Faults the specified ex.
        /// </summary>
        /// <param name="ex">The ex.</param>
        public void Fault(Exception ex) => _rootBlock.Fault(ex);

        /// <summary>
        /// Posts the specified item to the pipeline.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>System.Boolean.</returns>
        public bool Post(QuickBlockTransferSegment item) => _rootBlock.Post(item);

        #endregion Public Methods

    }
}
