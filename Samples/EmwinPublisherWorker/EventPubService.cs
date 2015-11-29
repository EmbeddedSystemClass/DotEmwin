using System;
using System.Diagnostics.Tracing;
using DotNetty.Common.Concurrency;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using Emwin.ByteBlaster;
using Emwin.ByteBlaster.Instrumentation;
using Emwin.Core.DataObjects;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using Microsoft.ServiceBus.Messaging;

namespace EmwinPublisherWorker
{
    using System.Threading;
    using System.Threading.Tasks;
    using Topshelf;
    using Topshelf.Logging;

    internal class EventPubService : ServiceControl
    {
        #region Private Fields

        private readonly CancellationTokenSource _cancel = new CancellationTokenSource();
        private readonly Configuration _configuration;
        private readonly LogWriter _log = HostLogger.Get<EventPubService>();
        private Task _task;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EventPubService"/> class.
        /// </summary>
        /// <param name="configuration">The service configuration.</param>
        public EventPubService(Configuration configuration)
        {
            _configuration = configuration;
        }

        #endregion Public Constructors

        #region Public Methods

        /// <summary>
        /// Starts the specified host control.
        /// </summary>
        /// <param name="hostControl">The host control.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool Start(HostControl hostControl)
        {
            _task = RunAsync(_cancel.Token);
            return !_task.IsFaulted;
        }

        public bool Stop(HostControl hostControl)
        {
            _cancel.Cancel();
            _task.Wait();

            return true;
        }

        #endregion Public Methods

        #region Private Methods

        private async Task Publish(EventHubClient client, QuickBlockTransferSegment segment)
        {
            try
            {
                await client.SendAsync(new EventData(segment.Content)
                {
                    PartitionKey = segment.GetKey(),
                    Properties =
                    {
                        {nameof(segment.Header), segment.Header},
                        {nameof(segment.ReceivedAt), segment.ReceivedAt},
                        {nameof(segment.Filename), segment.Filename},
                        {nameof(segment.TimeStamp), segment.TimeStamp},
                        {nameof(segment.Checksum), segment.Checksum},
                        {nameof(segment.BlockNumber), segment.BlockNumber},
                        {nameof(segment.TotalBlocks), segment.TotalBlocks},
                        {nameof(segment.Version), segment.Version},
                        {nameof(segment.Source), segment.Source}
                    }
                });
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
            }
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            var eventListener = new ObservableEventListener();
            eventListener.LogToWindowsAzureTable(
                _configuration.Identifier,
                _configuration.StorageConnectionString);

            eventListener.EnableEvents(ChannelEventSource.Log, EventLevel.Informational);
            eventListener.EnableEvents(BootstrapEventSource.Log, EventLevel.Informational);
            eventListener.EnableEvents(ExecutorEventSource.Log, EventLevel.Informational);
            eventListener.EnableEvents(ByteBlasterEventSource.Log, EventLevel.Informational);

            var eventHubClient = EventHubClient.CreateFromConnectionString(
                _configuration.EventHubConnectionString,
                _configuration.EventHubName);

            var byteBlasterClient = new ByteBlasterClient(_configuration.Email);

            // Subscribe and send Quick Block Transfer Segments to the Event Hub
            byteBlasterClient
                .Subscribe(async e => await Publish(eventHubClient, e));

            _log.InfoFormat("Starting EMWIN ByteBlaster Client");
            await byteBlasterClient.StartAsync(cancellationToken);

            eventHubClient.Close();
        }

        #endregion Private Methods
    }
}
