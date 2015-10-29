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
using Emwin.Core.DataObjects;
using Emwin.Core.EventAggregator;
using Emwin.Core.Interfaces;
using Emwin.Processor.Instrumentation;
using Emwin.Processor.Processor;

namespace Emwin.Processor
{
    public class WeatherProductProcessor : IObserver<QuickBlockTransferSegment>
    {

        #region Private Fields

        private readonly EventAggregator _eventAggregator = new EventAggregator();
        private readonly ObservableListener<IImageProduct> _imageObservable = new ObservableListener<IImageProduct>();
        private readonly ObservableListener<ITextProduct> _textObservable = new ObservableListener<ITextProduct>();

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WeatherProductProcessor" /> class.
        /// </summary>
        /// <param name="observable">The observable.</param>
        public WeatherProductProcessor(IObservable<QuickBlockTransferSegment> observable = null)
        {
            _eventAggregator
                .AddListener(new SegmentBundler(_eventAggregator))
                .AddListener(new ProductAssembler(_eventAggregator))
                .AddListener(new ZipProcessor(_eventAggregator))
                .AddListener(_textObservable)
                .AddListener(_imageObservable);

            observable?.Subscribe(this);
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// Gets the pipeline event subscription manager.
        /// </summary>
        /// <value>The pipeline.</value>
        public IEventSubscriptionManager Pipeline => _eventAggregator;

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Gets the image observable.
        /// </summary>
        /// <returns>System.IObservable&lt;Emwin.Core.Interfaces.ITextProduct&gt;.</returns>
        public IObservable<IImageProduct> GetImageObservable() => _imageObservable;

        /// <summary>
        /// Gets the text observable.
        /// </summary>
        /// <returns>System.IObservable&lt;Emwin.Core.Interfaces.ITextProduct&gt;.</returns>
        public IObservable<ITextProduct> GetTextObservable() => _textObservable;

        /// <summary>
        /// Called when input is completed.
        /// </summary>
        public void OnCompleted()
        {
            ProcessorEventSource.Log.Info("WeatherProductProcessor", "Observable source indicated completion");
        }

        /// <summary>
        /// Called when error occurs.
        /// </summary>
        /// <param name="error">The error.</param>
        public void OnError(Exception error)
        {
            ProcessorEventSource.Log.Error("WeatherProductProcessor", "Observable source error: " + error);
        }

        /// <summary>
        /// Called when next block segment is available for processing.
        /// </summary>
        /// <param name="blockSegment">The value.</param>
        public void OnNext(QuickBlockTransferSegment blockSegment)
        {
            _eventAggregator.SendMessage(blockSegment);
        }

        #endregion Public Methods

    }
}
