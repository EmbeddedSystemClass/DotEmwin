/*
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