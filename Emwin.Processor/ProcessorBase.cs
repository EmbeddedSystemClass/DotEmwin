/*
 * Microsoft Public License (MS-PL)
 * Copyright (c) 2015 Jonathan Bradshaw <jonathan@nrgup.net>
 *     
 * This license governs use of the accompanying software. If you use the software, you
 * accept this license. If you do not accept the license, do not use the software.
 *     
 * 1. Definitions
 *     The terms "reproduce," "reproduction," "derivative works," and "distribution" have the
 *     same meaning here as under U.S. copyright law.
 *     A "contribution" is the original software, or any additions or changes to the software.
 *     A "contributor" is any person that distributes its contribution under this license.
 *     "Licensed patents" are a contributor's patent claims that read directly on its contribution.
 *     
 * 2. Grant of Rights
 *     (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
 *     (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
 *     
 * 3. Conditions and Limitations
 *     (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
 *     (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from such contributor to the software ends automatically.
 *     (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present in the software.
 *     (D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a license that complies with this license.
 *     (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement.
 */
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Emwin.Processor.EventAggregator;
using Emwin.Processor.Instrumentation;

namespace Emwin.Processor
{
    public abstract class ProcessorBase<T> : IDisposable
    {
        #region Public Fields

        public const int DefaultWorkers = 2;

        #endregion Public Fields

        #region Protected Fields

        protected readonly BlockingCollection<T> SegmentQueue = new BlockingCollection<T>();
        protected readonly List<Worker> Workers = new List<Worker>();

        #endregion Protected Fields

        #region Public Methods

        /// <summary>
        /// Disposes this instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// How many workers are currently running
        /// </summary>
        public int NumberOfWorkers => Workers.Count;

        /// <summary>
        /// Sets the number of workers.
        /// </summary>
        /// <param name="value">The value.</param>
        public void SetNumberOfWorkers(int value)
        {
            lock (Workers)
            {
                while (Workers.Count > value)
                {
                    Workers[0].Stop();
                    Workers.RemoveAt(0);
                }

                while (Workers.Count < value)
                    Workers.Add(new Worker(GetAggregator(), SegmentQueue));
            }
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        /// <param name="numberOfWorkers">The number of workers.</param>
        public void Start(int numberOfWorkers = DefaultWorkers)
        {
            SetNumberOfWorkers(numberOfWorkers);
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public void Stop()
        {
            SetNumberOfWorkers(0);
        }

        #endregion Public Methods

        #region Protected Methods

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            SegmentQueue.Dispose();
            lock (Workers)
                Workers.ForEach(w => w.Dispose());
        }

        protected abstract IEventAggregator GetAggregator();

        #endregion Protected Methods

        #region Protected Classes

        protected class Worker : IDisposable
        {

            #region Private Fields

            private readonly CancellationTokenSource _cancelTokenSource = new CancellationTokenSource();

            private readonly Task _task;

            private static int _seq;
            public int Id { get; }

            #endregion Private Fields

            #region Public Constructors

            public Worker(
                IEventPublisher publisher,
                BlockingCollection<T> collection)
            {
                Id = Interlocked.Increment(ref _seq);

                _task = Task.Run(() =>
                {
                    ProcessorEventSource.Log.Info("Worker Started", Id.ToString());
                    try
                    {
                        foreach (var blockSegment in collection.GetConsumingEnumerable(_cancelTokenSource.Token))
                            publisher.SendMessage(blockSegment);
                    }
                    catch (OperationCanceledException)
                    {
                        // Ignore Cancel
                    }

                    ProcessorEventSource.Log.Info("Worker Stopped", Id.ToString());
                }, _cancelTokenSource.Token);
            }

            #endregion Public Constructors

            #region Public Methods

            public void Dispose()
            {
                _cancelTokenSource.Dispose();
                _task.Dispose();
            }

            public void Stop()
            {
                _cancelTokenSource.Cancel();
                _task.Wait();
            }

            #endregion Public Methods

        }

        #endregion Protected Classes
    }
}