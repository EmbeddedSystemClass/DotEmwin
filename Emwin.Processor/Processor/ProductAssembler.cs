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
using System.Threading.Tasks.Dataflow;
using Emwin.Core.Models;
using Emwin.Processor.Instrumentation;
using Emwin.Core.Interfaces;
using Emwin.Core.Products;

namespace Emwin.Processor.Processor
{
    /// <summary>
    /// Class ProductAssembler. Assembles bundles of segments into a product by combining all the bytes from each segment.
    /// </summary>
    internal sealed class ProductAssembler
    {
        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductAssembler"/> class.
        /// </summary>
        public ProductAssembler()
        {
            Block = new TransformManyBlock<QuickBlockTransferSegment[], IEmwinContent>(x => Execute(x),
                new ExecutionDataflowBlockOptions {MaxDegreeOfParallelism = -1});
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// Gets the block.
        /// </summary>
        /// <value>The block.</value>
        public TransformManyBlock<QuickBlockTransferSegment[], IEmwinContent> Block { get; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Filters the specified bundle.
        /// </summary>
        /// <param name="bundle">The bundle.</param>
        /// <returns>System.Boolean.</returns>
        public bool Predicate(QuickBlockTransferSegment[] bundle) => bundle.All(s => s != null);

        #endregion Public Methods

        #region Private Methods

        private IEnumerable<IEmwinContent> Execute(QuickBlockTransferSegment[] segments)
        {
            var product = ProductFactory.Create(segments);

            if (product == null)
                yield break;

            ProcessorEventSource.Log.Verbose("Product", product.ToString());
            PerformanceCounters.ProductsCreatedTotal.Increment();

            yield return product;
        }

        #endregion Private Methods

    }
}