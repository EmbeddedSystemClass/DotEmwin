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
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Emwin.ByteBlaster.Instrumentation;
using Emwin.ByteBlaster.Protocol;
using Emwin.Core.Models;

namespace Emwin.ByteBlaster
{
    /// <summary>
    /// Class ByteBlasterClient implements a persistent connection to a Byte Blaster Server
    /// and provides an observable provider of QuickBlockTransferSegment objects for processing.
    /// </summary>
    public class ByteBlasterClient : IObservable<QuickBlockTransferSegment>
    {

        #region Private Fields

        private static readonly IEventLoopGroup ExecutorGroup = new MultithreadEventLoopGroup();
        private readonly Bootstrap _channelBootstrap;
        private readonly List<IObserver<QuickBlockTransferSegment>> _observers = new List<IObserver<QuickBlockTransferSegment>>();
        private CancellationTokenSource _cancelSource;
        private IChannel _channel;
        private Task _task;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ByteBlasterClient" /> class.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="observer">The observer to subscribe.</param>
        public ByteBlasterClient(string email, IObserver<QuickBlockTransferSegment> observer = null)
        {
            _channelBootstrap = new Bootstrap()
                .Group(ExecutorGroup)
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.ConnectTimeout, TimeSpan.FromSeconds(20))
                .Option(ChannelOption.SoKeepalive, true)
                .Handler(new ActionChannelInitializer<ISocketChannel>(ch => ch.Pipeline.AddLast(
                    new ByteBlasterProtocolDecoder(),
                    new ByteBlasterLogonHandler(email),
                    new ByteBlasterWatchdogHandler(),
                    new ChannelEventHandler<QuickBlockTransferSegment>((ctx, segment) =>
                    {
                        lock (_observers) _observers.ForEach(o => o.OnNext(segment));
                    }),
                    new ChannelEventHandler<ByteBlasterServerList>((ctx, serverList) => ServerList = serverList)
                )));

            if (observer != null)
                Subscribe(observer);
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// Gets if the channel is open and active.
        /// </summary>
        /// <value>The channel is open and active.</value>
        public bool IsActive => _channel?.Active ?? false;

        /// <summary>
        /// Gets or sets the remote address.
        /// </summary>
        /// <value>The remote address.</value>
        public EndPoint RemoteAddress => _channel?.RemoteAddress;

        /// <summary>
        /// Gets or sets the Byte Blaster server list.
        /// </summary>
        /// <value>The Byte Blaster server list.</value>
        public ByteBlasterServerList ServerList { get; set; } = new ByteBlasterServerList();

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Shutdowns the executor threads gracefully.
        /// </summary>
        /// <returns>System.Threading.Tasks.Task.</returns>
        public static Task ShutdownGracefullyAsync() => ExecutorGroup.ShutdownGracefullyAsync();

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
        {
            if (_task != null && _task.IsCompleted == false) return;

            _cancelSource = new CancellationTokenSource();
            _task = Task.Factory.StartNew(
                function: ExecuteAsync,
                cancellationToken: _cancelSource.Token,
                creationOptions: TaskCreationOptions.LongRunning,
                scheduler: TaskScheduler.Default).Unwrap();
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        /// <param name="timeout">The timeout (or infinite if not specified).</param>
        public void Stop(TimeSpan? timeout = null)
        {
            _cancelSource?.Cancel();
            if (_task == null || _task.IsCompleted) return;

            try
            {
                _task.Wait(timeout ?? Timeout.InfiniteTimeSpan);
            }
            catch (AggregateException)
            {
            }
        }

        /// <summary>
        /// Subscribes the specified observer.
        /// </summary>
        /// <param name="observer">The observer.</param>
        /// <returns>System.IDisposable.</returns>
        public IDisposable Subscribe(IObserver<QuickBlockTransferSegment> observer)
        {
            if (observer == null) throw new ArgumentNullException(nameof(observer));

            lock (_observers)
                _observers.Add(observer);

            return new Unsubscriber(() => { lock (_observers) _observers.Remove(observer); });
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Connects to a round-robin list of servers. If connection is closed or does not succeed, the next server is tried.
        /// </summary>
        /// <returns>System.Threading.Tasks.Task.</returns>
        private async Task ExecuteAsync()
        {
            var index = 0;
            while (!_cancelSource.IsCancellationRequested)
            {
                if (index > ServerList.Servers.Count) index = 0;
                var serverAddress = ServerList.Servers[index++];
                ByteBlasterEventSource.Log.Info("Attempting connection to ByteBlaster server", serverAddress.ToString());

                try
                {
                    _channel = await _channelBootstrap.ConnectAsync(serverAddress);
                    _cancelSource.Token.Register(() => _channel.CloseAsync());
                    await _channel.CloseCompletion;
                }
                catch (SocketException ex)
                {
                    ByteBlasterEventSource.Log.Error(ex.GetBaseException().Message, ex.GetBaseException());
                }

                await Task.Delay(5000, _cancelSource.Token);
            }

            lock (_observers)
            {
                _observers.ForEach(o => o.OnCompleted());
                _observers.Clear();
            }
        }

        #endregion Private Methods

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
