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
using System.Diagnostics;

namespace Emwin.Processor.Instrumentation
{
    internal sealed class SafePerformanceCounter
    {
        #region Private Fields

        private readonly PerformanceCounter _counter;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SafePerformanceCounter"/> class.
        /// </summary>
        /// <param name="counter">The counter.</param>
        public SafePerformanceCounter(PerformanceCounter counter)
        {
            _counter = counter;
            Clear();
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// Gets or sets the raw value.
        /// </summary>
        /// <value>The raw value.</value>
        public long RawValue
        {
            get { return _counter.RawValue; }
            set { _counter.RawValue = value; }
        }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Clears this instance to zero.
        /// </summary>
        public void Clear()
        {
            _counter.RawValue = 0;
        }

        /// <summary>
        /// Decrements this instance.
        /// </summary>
        public void Decrement()
        {
            try
            {
                _counter.Decrement();
            }
            catch (Exception ex)
            {
                ProcessorEventSource.Log.Verbose("Failed to decrement performance counter", ex.ToString());
            }
        }

        /// <summary>
        /// Increments the counter.
        /// </summary>
        public void Increment()
        {
            try
            {
                _counter.Increment();
            }
            catch (Exception ex)
            {
                ProcessorEventSource.Log.Verbose("Failed to increment performance counter", ex.ToString());
            }
        }

        /// <summary>
        /// Increments the counter by the given value.
        /// </summary>
        /// <param name="value">The value.</param>
        public void IncrementBy(long value)
        {
            try
            {
                _counter.IncrementBy(value);
            }
            catch (Exception ex)
            {
                ProcessorEventSource.Log.Verbose("Failed to increment performance counter", ex.ToString());
            }
        }

        #endregion Public Methods
    }
}