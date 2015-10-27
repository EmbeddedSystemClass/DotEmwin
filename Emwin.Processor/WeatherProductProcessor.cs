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
using Emwin.Core.Interfaces;
using Emwin.Core.Models;
using Emwin.Processor.Pipeline;

namespace Emwin.Processor
{
    public class WeatherProductProcessor : IObserver<QuickBlockTransferSegment>
    {

        #region Private Fields

        /// <summary>
        /// The Quick Block Transfer Segment Pipeline observable
        /// </summary>
        private readonly IObserver<QuickBlockTransferSegment> _observable;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WeatherProductProcessor"/> class.
        /// </summary>
        public WeatherProductProcessor()
        {
            var pipeline = new PipelineBuilder(Filters);
            TextProductObservable = pipeline.TextProductObservable;
            ImageProductObservable = pipeline.ImageProductObservable;
            _observable = pipeline.SegmentObserver;
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// Gets the transformer filters.
        /// </summary>
        /// <value>The filters.</value>
        public PipelineFilters Filters { get; } = new PipelineFilters();

        /// <summary>
        /// Gets the image product observable.
        /// </summary>
        /// <returns>System.IObservable&lt;Emwin.Core.Interfaces.IImageProduct&gt;.</returns>
        public IObservable<IImageProduct> ImageProductObservable { get; }

        /// <summary>
        /// Gets the text product observable.
        /// </summary>
        /// <returns>System.IObservable&lt;Emwin.Core.Interfaces.ITextProduct&gt;.</returns>
        public IObservable<ITextProduct> TextProductObservable { get; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Called when input is completed.
        /// </summary>
        public void OnCompleted() => _observable.OnCompleted();

        /// <summary>
        /// Called when error occurs.
        /// </summary>
        /// <param name="error">The error.</param>
        public void OnError(Exception error) => _observable.OnError(error);

        /// <summary>
        /// Called when next block segment is available for processing.
        /// </summary>
        /// <param name="blockSegment">The value.</param>
        public void OnNext(QuickBlockTransferSegment blockSegment) => _observable.OnNext(blockSegment);

        #endregion Public Methods
    }
}
