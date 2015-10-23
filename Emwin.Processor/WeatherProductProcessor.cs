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
using Emwin.Core.Models;
using Emwin.Processor.Pipeline;
using Emwin.Processor.Processor;

namespace Emwin.Processor
{
    public class WeatherProductProcessor : IObserver<QuickBlockTransferSegment>, IObservable<WeatherProduct>
    {

        #region Private Fields

        private readonly List<IObserver<WeatherProduct>> _observers = new List<IObserver<WeatherProduct>>();

        private readonly ProcessingPipeline _pipeline;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WeatherProductProcessor"/> class.
        /// </summary>
        public WeatherProductProcessor()
        {
            _pipeline = new ProcessingPipeline(Filters, product => _observers.ForEach(x => x.OnNext(product)));
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// Gets the transformer filters.
        /// </summary>
        /// <value>The filters.</value>
        public Filters Filters { get; } = new Filters();

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Called when input is completed.
        /// </summary>
        public void OnCompleted()
        {
            _pipeline.Complete();
        }

        /// <summary>
        /// Called when error occurs.
        /// </summary>
        /// <param name="error">The error.</param>
        public void OnError(Exception error)
        {
            _pipeline.Fault(error);
        }

        /// <summary>
        /// Called when next block segment is available for processing.
        /// </summary>
        /// <param name="blockSegment">The value.</param>
        public void OnNext(QuickBlockTransferSegment blockSegment)
        {
            _pipeline.Post(blockSegment);
        }

        public IDisposable Subscribe(IObserver<WeatherProduct> observer)
        {
            _observers.Add(observer);
            return new Unsubscriber(() => _observers.Remove(observer));
        }

        #endregion Public Methods

        #region Private Classes

        private class Unsubscriber : IDisposable
        {

            #region Private Fields

            private readonly Action _action;

            #endregion Private Fields

            #region Public Constructors

            /// <summary>
            /// Initializes a new instance of the <see cref="Unsubscriber" /> class.
            /// </summary>
            /// <param name="action">The action.</param>
            public Unsubscriber(Action action)
            {
                _action = action;
            }

            #endregion Public Constructors

            #region Public Methods

            /// <summary>
            /// Disposes this instance.
            /// </summary>
            public void Dispose()
            {
                _action();
            }

            #endregion Public Methods

        }

        #endregion Private Classes

    }
}
