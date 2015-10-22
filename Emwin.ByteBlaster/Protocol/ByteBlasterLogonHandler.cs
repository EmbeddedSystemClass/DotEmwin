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
using System.Text;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Emwin.ByteBlaster.Instrumentation;

namespace Emwin.ByteBlaster.Protocol
{
    internal class ByteBlasterLogonHandler : ChannelHandlerAdapter
    {
        private readonly string _email;

        #region Private Fields

        private const string Logon = "ByteBlast Client|NM-{0}|V2";

        #endregion Private Fields

        #region Public Methods

        /// <summary>
        /// Initializes a new instance of the <see cref="ByteBlasterLogonHandler"/> class.
        /// </summary>
        /// <param name="email">The email used for logon.</param>
        public ByteBlasterLogonHandler(string email)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentException(nameof(email));

            _email = email;
        }

        /// <summary>
        /// Fired when channel is active.
        /// </summary>
        /// <param name="context">The context.</param>
        public override void ChannelActive(IChannelHandlerContext context)
        {
            SendIdentification(context.Channel);
            base.ChannelActive(context);
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Sends identification to the server.
        /// </summary>
        /// <param name="channel">The channel.</param>
        private void SendIdentification(IChannel channel)
        {
            var logon = string.Format(Logon, _email);
            channel.WriteAndFlushAsync(Unpooled.WrappedBuffer(XorArray(Encoding.ASCII.GetBytes(logon))));
            ByteBlasterEventSource.Log.Info("Sent Logon", logon);
        }

        /// <summary>
        /// Xor the bytes with 0xFF to get back the original content.
        /// </summary>
        /// <param name="array">The array.</param>
        private static byte[] XorArray(byte[] array)
        {
            for (var i = 0; i < array.Length; i++)
                array[i] ^= 0xFF;
            return array;
        }

        #endregion Private Methods
    }
}