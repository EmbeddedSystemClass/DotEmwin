// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.Tracing;

namespace Emwin.ByteBlaster.Instrumentation
{
    [EventSource(Name = "EmwinByteBlaster")]
    public class ByteBlasterEventSource : EventSource
    {

        #region Public Fields

        /// <summary>
        /// The event source log
        /// </summary>
        public static readonly ByteBlasterEventSource Log = new ByteBlasterEventSource();

        #endregion Public Fields

        #region Private Fields

        private const int ConnectEventId = 14;
        private const int ErrorEventId = 4;
        private const int HeaderReceivedEventId = 13;
        private const int InfoEventId = 2;
        private const int PacketCreatedEventId = 12;
        private const int ParserWarningEventId = 11;
        private const int ServerListEventId = 15;
        private const int SynchronizationEventId = 10;
        private const int VerboseEventId = 1;
        private const int WarningEventId = 3;

        #endregion Private Fields

        #region Private Constructors

        /// <summary>
        /// Prevents a default instance of the <see cref="ByteBlasterEventSource"/> class from being created.
        /// </summary>
        private ByteBlasterEventSource()
        {
        }

        #endregion Private Constructors

        #region Internal Properties

        internal bool IsErrorEnabled => IsEnabled(EventLevel.Error, EventKeywords.None);

        internal bool IsInfoEnabled => IsEnabled(EventLevel.Informational, EventKeywords.None);

        internal bool IsVerboseEnabled => IsEnabled(EventLevel.Verbose, EventKeywords.None);

        internal bool IsWarningEnabled => IsEnabled(EventLevel.Warning, EventKeywords.None);

        #endregion Internal Properties

        #region Internal Methods

        [Event(ConnectEventId, Level = EventLevel.Informational, Message = "Connecting to {0}")]
        internal void Connect(string hostname)
        {
            if (IsInfoEnabled)
                WriteEvent(ConnectEventId, hostname);
        }

        [NonEvent]
        internal void Error(string message, Exception exception)
        {
            if (IsErrorEnabled)
                Error(message, exception?.ToString() ?? string.Empty);
        }

        [Event(ErrorEventId, Level = EventLevel.Error, Message = "Error: {0}")]
        internal void Error(string message, string exception)
        {
            if (IsErrorEnabled)
                WriteEvent(ErrorEventId, message, exception);
        }

        [Event(HeaderReceivedEventId, Level = EventLevel.Verbose, Message = "Header Received")]
        internal void HeaderReceived(string header)
        {
            if (IsVerboseEnabled)
                WriteEvent(HeaderReceivedEventId, header);
        }

        [Event(InfoEventId, Level = EventLevel.Informational, Message = "Info: {0}")]
        internal void Info(string message, string info)
        {
            if (IsInfoEnabled)
                WriteEvent(InfoEventId, message, info);
        }

        [Event(PacketCreatedEventId, Level = EventLevel.Verbose, Message = "Segment Block Created")]
        internal void PacketCreated(string packet)
        {
            if (IsVerboseEnabled)
                WriteEvent(PacketCreatedEventId, packet);
        }

        [Event(ParserWarningEventId, Level = EventLevel.Warning, Message = "Parser error: {0}")]
        internal void ParserWarning(string message, string content)
        {
            if (IsWarningEnabled)
                WriteEvent(ParserWarningEventId, message, content);
        }

        [Event(ServerListEventId, Level = EventLevel.Informational, Message = "Server list receieved")]
        internal void ServerList(string servers)
        {
            if (IsInfoEnabled)
                WriteEvent(ServerListEventId, servers);
        }

        [Event(SynchronizationEventId, Level = EventLevel.Warning, Message = "Unable to synchronize stream after {0} attempts")]
        internal void SynchronizationWarning(int attempts)
        {
            if (IsWarningEnabled)
                WriteEvent(SynchronizationEventId, attempts);
        }

        [Event(VerboseEventId, Level = EventLevel.Verbose, Message = "{0}")]
        internal void Verbose(string message, string info)
        {
            if (IsVerboseEnabled)
                WriteEvent(VerboseEventId, message, info);
        }

        [NonEvent]
        internal void Warning(string message)
        {
            Warning(message, string.Empty);
        }

        [NonEvent]
        internal void Warning(string message, Exception exception)
        {
            if (IsWarningEnabled)
                Warning(message, exception?.ToString() ?? string.Empty);
        }

        [Event(WarningEventId, Level = EventLevel.Warning, Message = "Warning: {0}")]
        internal void Warning(string message, string exception)
        {
            if (IsWarningEnabled)
                WriteEvent(WarningEventId, message, exception);
        }

        #endregion Internal Methods

    }
}