using DotNetty.Common;

// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Emwin.ByteBlaster.Instrumentation
{
    internal sealed class AveragePerformanceCounter
    {

        #region Private Fields

        private readonly SafePerformanceCounter _baseCounter;

        private readonly SafePerformanceCounter _countCounter;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AveragePerformanceCounter"/> class.
        /// </summary>
        /// <param name="countCounter">The count counter.</param>
        /// <param name="baseCounter">The base counter.</param>
        public AveragePerformanceCounter(SafePerformanceCounter countCounter, SafePerformanceCounter baseCounter)
        {
            _countCounter = countCounter;
            _baseCounter = baseCounter;
        }

        #endregion Public Constructors

        #region Public Methods

        /// <summary>
        /// Registers the specified start timestamp.
        /// </summary>
        /// <param name="startTimestamp">The start timestamp.</param>
        public void Register(PreciseTimeSpan startTimestamp)
        {
            var elapsed = PreciseTimeSpan.FromStart - startTimestamp;
            var elapsedMs = (long) elapsed.ToTimeSpan().TotalMilliseconds;
            _countCounter.IncrementBy(elapsedMs);
            _baseCounter.Increment();
        }

        #endregion Public Methods
    }
}