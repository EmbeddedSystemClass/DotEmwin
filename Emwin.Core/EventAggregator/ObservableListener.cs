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

namespace Emwin.Core.EventAggregator
{
    public class ObservableListener<T> : IHandle<T>, IObservable<T>
    {
        #region Private Fields

        private readonly List<IObserver<T>> _observers = new List<IObserver<T>>();

        #endregion Private Fields

        #region Public Methods

        /// <summary>
        /// This will be called every time a TMessage is published through the event aggregator
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="ctx">The Event Aggregator context.</param>
        public void Handle(T message, IEventAggregator ctx)
        {
            try
            {
                lock (_observers)
                {
                    _observers.ForEach(o => o.OnNext(message));
                }
            }
            catch
            {
                // ignored
            }
        }

        /// <summary>
        /// Subscribes the specified observer.
        /// </summary>
        /// <param name="observer">The observer.</param>
        /// <returns>System.IDisposable.</returns>
        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (observer == null)
                throw new ArgumentNullException(nameof(observer));

            lock (_observers)
                _observers.Add(observer);

            return new DisposeAction(() =>
            {
                lock (_observers) 
                    _observers.Remove(observer);
            });
        }

        #endregion Public Methods

        #region Private Classes

        private class DisposeAction : IDisposable
        {

            #region Private Fields

            private readonly Action _action;

            #endregion Private Fields

            #region Public Constructors

            /// <summary>
            /// Initializes a new instance of the <see cref="DisposeAction" /> class.
            /// </summary>
            /// <param name="action">The action.</param>
            public DisposeAction(Action action)
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
