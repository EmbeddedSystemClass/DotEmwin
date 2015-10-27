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
using System.Runtime.Caching;
using System.Threading.Tasks.Dataflow;
using Emwin.Core.Interfaces;
using Emwin.Processor.Instrumentation;
using Emwin.Processor.Pipeline;

namespace Emwin.Processor.Processor
{
    internal sealed class ProductFilter
    {
        #region Private Fields

        private readonly PipelineFilters _filters;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductFilter"/> class.
        /// </summary>
        /// <param name="filters">The filters.</param>
        public ProductFilter(PipelineFilters filters)
        {
            _filters = filters;
            Block = new TransformManyBlock<IEmwinContent, IEmwinContent>(x => Execute(x));
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// Gets the block.
        /// </summary>
        /// <value>The block.</value>
        public TransformManyBlock<IEmwinContent, IEmwinContent> Block { get; }

        #endregion Public Properties

        #region Private Methods

        private IEnumerable<IEmwinContent> Execute(IEmwinContent product)
        {
            try
            {
                if (_filters.ContentFilter != null && _filters.ContentFilter(product))
                {
                    ProcessorEventSource.Log.Info("Product Filtered", product.ToString());
                    PerformanceCounters.ProductsFilteredTotal.Increment();
                    yield break;
                }
            }
            catch (Exception ex)
            {
                ProcessorEventSource.Log.Error("Product Filter", ex);
            }

            yield return product;
        }

        #endregion Private Methods
    }
}