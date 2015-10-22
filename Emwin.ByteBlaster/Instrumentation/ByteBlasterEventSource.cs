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

        private const int ErrorEventId = 4;
        private const int InfoEventId = 2;
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

        #region Public Properties

        internal bool IsErrorEnabled => IsEnabled(EventLevel.Error, EventKeywords.None);

        internal bool IsInfoEnabled => IsEnabled(EventLevel.Informational, EventKeywords.None);

        internal bool IsVerboseEnabled => IsEnabled(EventLevel.Verbose, EventKeywords.None);

        internal bool IsWarningEnabled => IsEnabled(EventLevel.Warning, EventKeywords.None);

        #endregion Public Properties

        #region Public Methods

        [NonEvent]
        internal void Error(string message, Exception exception)
        {
            if (IsErrorEnabled)
                Error(message, exception?.ToString() ?? string.Empty);
        }

        [Event(ErrorEventId, Level = EventLevel.Error)]
        internal void Error(string message, string exception)
        {
            if (IsErrorEnabled)
                WriteEvent(ErrorEventId, message, exception);
        }

        [Event(InfoEventId, Level = EventLevel.Informational)]
        internal void Info(string message, string info)
        {
            if (IsInfoEnabled)
                WriteEvent(InfoEventId, message, info);
        }

        [Event(VerboseEventId, Level = EventLevel.Verbose)]
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

        [Event(WarningEventId, Level = EventLevel.Warning)]
        internal void Warning(string message, string exception)
        {
            if (IsWarningEnabled)
                WriteEvent(WarningEventId, message, exception);
        }

        #endregion Public Methods
    }
}