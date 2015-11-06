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
using System.Diagnostics.Tracing;

namespace Emwin.Processor.Instrumentation
{
    [EventSource(Name = "EmwinProcessor")]
    public class ProcessorEventSource : EventSource
    {

        #region Public Fields

        /// <summary>
        /// The event source log
        /// </summary>
        public static readonly ProcessorEventSource Log = new ProcessorEventSource();

        #endregion Public Fields

        #region Private Fields

        private const int ErrorEventId = 4;
        private const int InfoEventId = 2;
        private const int VerboseEventId = 1;
        private const int WarningEventId = 3;

        #endregion Private Fields

        #region Private Constructors

        /// <summary>
        /// Prevents a default instance of the <see cref="ProcessorEventSource"/> class from being created.
        /// </summary>
        private ProcessorEventSource()
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

        [Event(InfoEventId, Level = EventLevel.Informational, Message = "Info: {0}")]
        internal void Info(string message, string info)
        {
            if (IsInfoEnabled)
                WriteEvent(InfoEventId, message, info);
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