// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetty.Common.Concurrency;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using Emwin.ByteBlaster.Instrumentation;

namespace Emwin.ByteBlaster.Processor
{
    /// <summary>
    /// Class PacketAsyncProcessor.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class AsyncProcessingQueue<T>
    {
        private readonly Queue<T> _backlogQueue;
        private readonly TaskCompletionSource _completionSource;
        private State _state;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncProcessingQueue{T}"/> class.
        /// </summary>
        protected AsyncProcessingQueue()
        {
            _backlogQueue = new Queue<T>();
            _completionSource = new TaskCompletionSource();
        }

        /// <summary>
        /// Gets the completion task.
        /// </summary>
        /// <value>The completion task.</value>
        public Task Completion => _completionSource.Task;

        /// <summary>
        /// Gets the size of the backlog.
        /// </summary>
        /// <value>The size of the backlog.</value>
        public int BacklogSize => _backlogQueue.Count;

        /// <summary>
        /// Posts the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="packet">The packet.</param>
        public void Post(IChannelHandlerContext context, T packet)
        {
            switch (_state)
            {
                case State.Idle:
                    _backlogQueue.Enqueue(packet);
                    _state = State.Processing;
                    StartQueueProcessingAsync(context);
                    break;
                case State.Processing:
                case State.FinalProcessing:
                    _backlogQueue.Enqueue(packet);
                    break;
                case State.Aborted:
                    ReferenceCountUtil.Release(packet);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Completes this instance.
        /// </summary>
        public void Complete()
        {
            switch (_state)
            {
                case State.Idle:
                    _completionSource.TryComplete();
                    break;
                case State.Processing:
                    _state = State.FinalProcessing;
                    break;
                case State.FinalProcessing:
                case State.Aborted:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Aborts this instance.
        /// </summary>
        public void Abort()
        {
            switch (_state)
            {
                case State.Idle:
                case State.Processing:
                case State.FinalProcessing:
                    _state = State.Aborted;

                    var queue = _backlogQueue;
                    while (queue.Count > 0)
                    {
                        var packet = queue.Dequeue();
                        ReferenceCountUtil.Release(packet);
                    }
                    break;
                case State.Aborted:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Starts the queue processing asynchronous.
        /// </summary>
        /// <param name="context">The context.</param>
        private async void StartQueueProcessingAsync(IChannelHandlerContext context)
        {
            try
            {
                var queue = _backlogQueue;
                while (queue.Count > 0 && _state != State.Aborted)
                {
                    PerformanceCounters.TransformerQueueCounter.RawValue = queue.Count;
                    var message = queue.Dequeue();
                    await ProcessAsync(context, message);
                }

                switch (_state)
                {
                    case State.Processing:
                        _state = State.Idle;
                        break;
                    case State.FinalProcessing:
                    case State.Aborted:
                        _completionSource.TryComplete();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception ex)
            {
                Abort();
                _completionSource.TrySetException(ex);
            }
        }

        /// <summary>
        /// Processes the asynchronous packet.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="packet">The packet.</param>
        /// <returns>System.Threading.Tasks.Task.</returns>
        protected abstract Task ProcessAsync(IChannelHandlerContext context, T packet);

        /// <summary>
        /// Enum State for Packet Processing
        /// </summary>
        private enum State
        {
            Idle,
            Processing,
            FinalProcessing,
            Aborted
        }
    }
}