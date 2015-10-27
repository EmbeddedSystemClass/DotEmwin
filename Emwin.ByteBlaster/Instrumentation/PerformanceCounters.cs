// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace Emwin.ByteBlaster.Instrumentation
{
    /// <summary>
    /// Class PerformanceCounters.
    /// </summary>
    internal static class PerformanceCounters
    {
        private static readonly Manager Instance = new Manager();

        #region Public Performance Counters

        public static readonly SafePerformanceCounter BlocksReceivedTotal = Instance.GetCounter(BlocksReceivedTotalCounterName);
        public static readonly SafePerformanceCounter BlocksProcessedPerSecond = Instance.GetCounter(BlocksProcessedPerSecondCounterName);
        public static readonly SafePerformanceCounter CompressedBlocksReceivedTotal = Instance.GetCounter(CompressedBlocksReceivedTotalCounterName);
        public static readonly SafePerformanceCounter DecoderExceptionsTotal = Instance.GetCounter(DecoderExceptionsTotalCounterName);
        public static readonly SafePerformanceCounter ChecksumErrorsTotal = Instance.GetCounter(ChecksumErrorsTotalCounterName);
        public static readonly SafePerformanceCounter ServerListReceivedTotal = Instance.GetCounter(ServerListReceivedTotalCounterName);
        public static readonly SafePerformanceCounter FrameSyncTotal = Instance.GetCounter(FrameSyncTotalCounterName);

        #endregion Public Performance Counters

        #region Private Fields

        private const string CategoryHelp = "EMWIN Byte Blaster Client Metrics";
        private const string CategoryName = "EMWIN Byte Blaster Client";

        private const string BlocksReceivedTotalCounterName = "Blocks Received Total";
        private const string BlocksProcessedPerSecondCounterName = "Blocks Processed/sec";
        private const string CompressedBlocksReceivedTotalCounterName = "Compressed Blocks Received Total";
        private const string DecoderExceptionsTotalCounterName = "Decoder Exceptions Total";
        private const string ChecksumErrorsTotalCounterName = "Block Checksum Errors Total";
        private const string ServerListReceivedTotalCounterName = "Server List Received Total";
        private const string FrameSyncTotalCounterName = "Frame Re-Synchronization Total";

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
                }
            ) { }

            #endregion Public Constructors
        }

        #endregion Private Classes
    }
}