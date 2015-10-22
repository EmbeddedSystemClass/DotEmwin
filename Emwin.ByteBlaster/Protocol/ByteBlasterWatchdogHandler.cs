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
using System.Diagnostics;
using System.Threading;
using DotNetty.Transport.Channels;

namespace Emwin.ByteBlaster.Protocol
{
    internal class ByteBlasterWatchdogHandler : ChannelHandlerAdapter
    {

        #region Public Fields

        public static int MaxExceptions = 10;

        #endregion Public Fields

        #region Private Fields

        private int _exceptionCount;
        private bool _isReceiving;
        private Timer _watchdogTimer;

        #endregion Private Fields

        #region Public Methods

        /// <summary>
        /// Fired when channel is active.
        /// </summary>
        /// <param name="context">The context.</param>
        public override void ChannelActive(IChannelHandlerContext context)
        {
            _exceptionCount = 0;
            _watchdogTimer = new Timer(IdleCheck, context, TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(20));
            base.ChannelActive(context);
        }

        /// <summary>
        /// Fired when channel is inactive.
        /// </summary>
        /// <param name="context">The context.</param>
        public override void ChannelInactive(IChannelHandlerContext context)
        {
            _watchdogTimer?.Dispose();
            base.ChannelInactive(context);
        }

        /// <summary>
        /// Fired when channel read is complete.
        /// </summary>
        /// <param name="context">The context.</param>
        public override void ChannelReadComplete(IChannelHandlerContext context)
        {
            _isReceiving = true;
            base.ChannelReadComplete(context);
        }

        /// <summary>
        /// Fired when exception is caught.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="exception">The exception.</param>
        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            _exceptionCount++;
            base.ExceptionCaught(context, exception);
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Checks for idle (no received data).
        /// </summary>
        /// <param name="obj">The object.</param>
        private void IdleCheck(object obj)
        {
            var context = (IChannelHandlerContext) obj;
            if (!context.Channel.Active) return;

            if (!_isReceiving)
            {
                Trace.TraceWarning("ByteBlasterWatchdogHandler: No data received in the last 20 seconds, closing channel.");
                context.Channel.CloseAsync();
                return;
            }

            if (_exceptionCount > MaxExceptions)
            {
                Trace.TraceWarning("ByteBlasterWatchdogHandler: Exception threshold exceeded, closing channel.");
                context.Channel.CloseAsync();
                return;
            }

            _isReceiving = false;
        }

        #endregion Private Methods

    }
}