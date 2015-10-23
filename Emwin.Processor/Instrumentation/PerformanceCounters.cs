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
        public static readonly SafePerformanceCounter TransformerQueueCounter = Instance.GetCounter(TransformerQueueCounterName);
        public static readonly SafePerformanceCounter BlocksFilteredTotal = Instance.GetCounter(BlocksFilteredTotalCounterName);
        public static readonly SafePerformanceCounter ProductsFilteredTotal = Instance.GetCounter(ProductsFilteredTotalCounterName);
        public static readonly SafePerformanceCounter DuplicateProductsTotal = Instance.GetCounter(DuplicateProductsTotalCounterName);

        #endregion Public Performance Counters

        #region Private Fields

        private const string CategoryHelp = "EMWIN Byte Blaster Metrics";
        private const string CategoryName = "Byte Blaster Client";

        private const string BlocksReceivedTotalCounterName = "Blocks Received Total";
        private const string BlocksProcessedPerSecondCounterName = "Blocks Processed/sec";
        private const string CompressedBlocksReceivedTotalCounterName = "Compressed Blocks Received Total";
        private const string DecoderExceptionsTotalCounterName = "Decoder Exceptions Total";
        private const string ChecksumErrorsTotalCounterName = "Block Checksum Errors Total";
        private const string ServerListReceivedTotalCounterName = "Server List Received Total";
        private const string FrameSyncTotalCounterName = "Frame Re-Synchronization Total";
        private const string ProductsCreatedTotalCounterName = "Products Created Total";
        private const string TransformerExceptionsTotalCounterName = "Transformer Exceptions Total";
        private const string TransformerQueueCounterName = "Transformer Queue Count";
        private const string BlocksFilteredTotalCounterName = "Blocks Filtered Total";
        private const string ProductsFilteredTotalCounterName = "Products Filtered Total";
        private const string DuplicateProductsTotalCounterName = "Duplicate Products Total";

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
                    new CounterCreationData(BlocksReceivedTotalCounterName, "", PerformanceCounterType.NumberOfItems64),
                    new CounterCreationData(BlocksProcessedPerSecondCounterName, "", PerformanceCounterType.RateOfCountsPerSecond32),
                    new CounterCreationData(CompressedBlocksReceivedTotalCounterName, "", PerformanceCounterType.NumberOfItems64),
                    new CounterCreationData(DecoderExceptionsTotalCounterName, "", PerformanceCounterType.NumberOfItems64),
                    new CounterCreationData(ChecksumErrorsTotalCounterName, "", PerformanceCounterType.NumberOfItems64),
                    new CounterCreationData(ServerListReceivedTotalCounterName, "", PerformanceCounterType.NumberOfItems64),
                    new CounterCreationData(FrameSyncTotalCounterName, "", PerformanceCounterType.NumberOfItems64),
                    new CounterCreationData(ProductsCreatedTotalCounterName, "", PerformanceCounterType.NumberOfItems64),
                    new CounterCreationData(TransformerExceptionsTotalCounterName, "", PerformanceCounterType.NumberOfItems64),
                    new CounterCreationData(TransformerQueueCounterName, "", PerformanceCounterType.NumberOfItems64),
                    new CounterCreationData(BlocksFilteredTotalCounterName, "", PerformanceCounterType.NumberOfItems64),
                    new CounterCreationData(ProductsFilteredTotalCounterName, "", PerformanceCounterType.NumberOfItems64),
                    new CounterCreationData(DuplicateProductsTotalCounterName, "", PerformanceCounterType.NumberOfItems64)
                }
            ) { }

            #endregion Public Constructors
        }

        #endregion Private Classes
    }
}