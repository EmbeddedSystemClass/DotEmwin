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

using System.Diagnostics;

namespace Emwin.Processor.Instrumentation
{
    /// <summary>
    /// Class PerformanceCounters.
    /// </summary>
    internal static class PerformanceCounters
    {
        private static readonly Manager Instance = new Manager();

        #region Public Performance Counters

        public static readonly SafePerformanceCounter TransformerExceptionsTotal = Instance.GetCounter(TransformerExceptionsTotalCounterName);
        public static readonly SafePerformanceCounter ProductsCreatedTotal = Instance.GetCounter(ProductsCreatedTotalCounterName);

        #endregion Public Performance Counters

        #region Private Fields

        private const string CategoryHelp = "EMWIN Processor Metrics";
        private const string CategoryName = "EMWIN Processor";

        private const string ProductsCreatedTotalCounterName = "Products Created Total";
        private const string TransformerExceptionsTotalCounterName = "Transformer Exceptions Total";

        #endregion Private Fields

        #region Private Classes

        private class Manager : PerformanceCounterManager
        {
            #region Public Constructors

            /// <summary>
            /// Initializes a new instance of the <see cref="Manager"/> class.
            /// </summary>
            public Manager() : base(
                new PerformanceCounterCategoryInfo(CategoryName, PerformanceCounterCategoryType.SingleInstance, CategoryHelp),
                new[]
                {
                    new CounterCreationData(ProductsCreatedTotalCounterName, "", PerformanceCounterType.NumberOfItems64),
                    new CounterCreationData(TransformerExceptionsTotalCounterName, "", PerformanceCounterType.NumberOfItems64),
                }
            ) { }

            #endregion Public Constructors
        }

        #endregion Private Classes
    }
}