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
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Emwin.ByteBlaster.Events;
using Emwin.ByteBlaster.Instrumentation;
using Emwin.ByteBlaster.Processor;
using Emwin.ByteBlaster.Protocol;
using Emwin.Core;
using Emwin.Core.Models;

namespace Emwin.ByteBlaster
{
    public class ByteBlasterClient
    {

        #region Private Fields

        private static readonly IEventLoopGroup ExecutorGroup = new MultithreadEventLoopGroup();
        private readonly Bootstrap _channelBootstrap;
        private IChannel _channel;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ByteBlasterClient"/> class.
        /// </summary>
        public ByteBlasterClient(string email)
        {
            var transformer = new SegmentProcessor(Filters, wx =>
            {
                LastReceivedProduct = wx;
                OnReceive(new WeatherProductEventArgs(wx));
            });

            _channelBootstrap = new Bootstrap()
                .Group(ExecutorGroup)
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.ConnectTimeout, TimeSpan.FromSeconds(20))
                .Option(ChannelOption.SoKeepalive, true)
                .Handler(new ActionChannelInitializer<ISocketChannel>(ch => ch.Pipeline.AddLast(
                    new ByteBlasterProtocolDecoder(),
                    new ByteBlasterLogonHandler(email),
                    new ByteBlasterWatchdogHandler(),
                    new ChannelEventHandler<QuickBlockTransferSegment>((ctx,s) => transformer.Post(ctx,s)),
                    new ChannelEventHandler<ByteBlasterServerList>((ctx, serverList) =>
                    {
                        ServerList = serverList;
                        OnReceive(new ServerListEventArgs(serverList));
                    })
                )));
        }

        #endregion Public Constructors

        #region Public Events

        /// <summary>
        /// Occurs when a new server list is received.
        /// </summary>
        public event EventHandler<ServerListEventArgs> ServerListReceived;

        /// <summary>
        /// Occurs when new weather product is received.
        /// </summary>
        public event EventHandler<WeatherProductEventArgs> WeatherProductReceived;

        #endregion Public Events

        #region Public Properties

        /// <summary>
        /// Gets the transformer filters.
        /// </summary>
        /// <value>The filters.</value>
        public Filters Filters { get; } = new Filters();

        /// <summary>
        /// Gets if the channel is open and active.
        /// </summary>
        /// <value>The channel is open and active.</value>
        public bool IsActive => _channel?.Active ?? false;

        /// <summary>
        /// Gets the last received product (null if none yet received).
        /// </summary>
        /// <value>The last received product (or null).</value>
        public WeatherProduct LastReceivedProduct { get; private set; }

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
        /// Starts the client asynchronously and keeps it connected to an available server until canceled.
        /// </summary>
        /// <returns>Task.</returns>
        public Task StartAsync(CancellationToken cancellationToken) => Task.Run(() => DoConnect(cancellationToken), cancellationToken);

        #endregion Public Methods

        #region Protected Methods

        /// <summary>
        /// Handles the <see cref="E:WeatherProductReceived" /> event.
        /// </summary>
        /// <param name="e">The <see cref="WeatherProductEventArgs" /> instance containing the event data.</param>
        protected virtual void OnReceive(WeatherProductEventArgs e)
        {
            var handler = WeatherProductReceived;
            handler?.Invoke(this, e);
        }

        /// <summary>
        /// Handles the <see cref="E:ServerListReceived" /> event.
        /// </summary>
        /// <param name="e">The <see cref="ServerListEventArgs" /> instance containing the event data.</param>
        protected virtual void OnReceive(ServerListEventArgs e)
        {
            var handler = ServerListReceived;
            handler?.Invoke(this, e);
        }

        #endregion Protected Methods

        #region Private Methods

        /// <summary>
        /// Connects to a round-robin list of servers. If connection is closed or does not succeed, the next server is tried.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>System.Threading.Tasks.Task.</returns>
        private async Task DoConnect(CancellationToken cancellationToken)
        {
            var index = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                if (index > ServerList.Servers.Count) index = 0;
                var serverAddress = ServerList.Servers[index++];
                ByteBlasterEventSource.Log.Info("Connecting to server", serverAddress.ToString());

                try
                {
                    _channel = await _channelBootstrap.ConnectAsync(serverAddress);
                    cancellationToken.Register(() => _channel.CloseAsync());
                    await _channel.CloseCompletion;
                }
                catch (SocketException ex)
                {
                    ByteBlasterEventSource.Log.Error(ex.GetBaseException().Message, ex.GetBaseException());
                }

                await Task.Delay(5000, cancellationToken);
            }
        }

        #endregion Private Methods

    }
}
