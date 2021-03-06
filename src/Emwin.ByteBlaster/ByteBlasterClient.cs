﻿/*
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
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using Emwin.ByteBlaster.Channel;
using Emwin.ByteBlaster.Instrumentation;
using Emwin.ByteBlaster.Protocol;
using Emwin.Core.DataObjects;

namespace Emwin.ByteBlaster
{
    /// <summary>
    /// Class ByteBlasterClient implements a persistent connection to a Byte Blaster Server
    /// and provides an observable provider of QuickBlockTransferSegment objects for processing.
    /// </summary>
    public class ByteBlasterClient : ObservableBase<QuickBlockTransferSegment>
    {

        #region Private Fields

        private readonly Bootstrap _channelBootstrap;
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
            _channelBootstrap = ByteBlasterChannelFactory.CreateBootstrap()
                .Handler(new ByteBlasterChannelInitializer(email,
                    new ChannelEventHandler<QuickBlockTransferSegment>((ctx, segment) => NotifySubscribers(segment)),
                    new ChannelEventHandler<ByteBlasterServerList>((ctx, serverList) => UpdateServerList(serverList))
                ));

            if (observer != null)
                Subscribe(observer);
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// Gets or sets the enable server list updates option.
        /// </summary>
        /// <value>The enable server list updates option.</value>
        public bool EnableServerListUpdates { get; set; } = true;

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
        public static Task ShutdownGracefullyAsync() => ByteBlasterChannelFactory.ShutdownGracefullyAsync();

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
        {
            StartAsync(CancellationToken.None);
        }

        /// <summary>
        /// Starts the instance and returns the task.
        /// </summary>
        /// <returns>System.Threading.Tasks.Task.</returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (_task != null && _task.IsCompleted == false)
                throw new InvalidOperationException("Task is already running");

            _cancelSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _task = Task.Factory
                .StartNew(ExecuteAsync, _cancelSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default)
                .Unwrap();

            return _task;
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
                if (index >= ServerList.Servers.Count) index = 0;
                var serverAddress = ServerList.Servers[index++];
                ByteBlasterEventSource.Log.Connect(serverAddress.ToString());

                try
                {
                    _channel = await _channelBootstrap.ConnectAsync(serverAddress);
                    _cancelSource.Token.Register(() => _channel.CloseAsync());
                    await _channel.CloseCompletion;
                }
                catch (Exception ex)
                {
                    ByteBlasterEventSource.Log.Error(ex.GetBaseException().Message, ex.ToString());
                }

                await Task.Delay(5000, _cancelSource.Token);
            }

            NotifyCompleted();
        }

        /// <summary>
        /// Updates the server list.
        /// </summary>
        /// <param name="serverList">The server list.</param>
        private void UpdateServerList(ByteBlasterServerList serverList)
        {
            if (EnableServerListUpdates)
                ServerList = serverList;
        }

        #endregion Private Methods
    }
}
