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
using DotNetty.Transport.Channels;

namespace Emwin.ByteBlaster.Protocol
{
    internal class ChannelEventHandler<T> : ChannelHandlerAdapter
    {
        private readonly Action<IChannelHandlerContext, T> _action;

        public override bool IsSharable => true;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelEventHandler{T}" /> class.
        /// </summary>
        /// <param name="action">The action.</param>
        public ChannelEventHandler(Action<IChannelHandlerContext, T> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            _action = action;
        }

        /// <summary>
        /// Fired when channel has user event.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="evt">The evt.</param>
        public override void UserEventTriggered(IChannelHandlerContext context, object evt)
        {
            if (evt is T)
                context.Executor.Execute(() => _action(context, (T)evt));

            base.UserEventTriggered(context, evt);
        }
    }
}