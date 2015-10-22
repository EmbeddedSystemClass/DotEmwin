// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;

namespace Emwin.ByteBlaster.Instrumentation
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
                ByteBlasterEventSource.Log.Verbose("Failed to decrement performance counter", ex.ToString());
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
                ByteBlasterEventSource.Log.Verbose("Failed to increment performance counter", ex.ToString());
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
                ByteBlasterEventSource.Log.Verbose("Failed to increment performance counter", ex.ToString());
            }
        }

        #endregion Public Methods
    }
}