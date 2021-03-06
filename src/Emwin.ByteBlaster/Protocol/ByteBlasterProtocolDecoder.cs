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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using Emwin.ByteBlaster.Instrumentation;
using Emwin.Core.DataObjects;

namespace Emwin.ByteBlaster.Protocol
{
    /// <summary>
    /// This class handles both version 1 and version 2 of the EMWIN Quick Block Transfer protocol.
    /// See http://www.nws.noaa.gov/emwin/winpro.htm
    /// and https://github.com/noaaport/npemwin/blob/master/dev-notes/EMWINProtocol.pdf
    /// </summary>
    internal class ByteBlasterProtocolDecoder : ByteToMessageDecoder
    {
        #region Protected Fields

        /// <summary>
        /// The QuickBlockPacket POCO created by this decoder and passed up the pipeline.
        /// </summary>
        protected QuickBlockTransferSegment Packet;

        /// <summary>
        /// The decoder state kept between decoder method invocations
        /// </summary>
        protected DecoderState State;

        #endregion Protected Fields

        #region Private Fields

        /// <summary>
        /// The number of frame synchronize null bytes
        /// </summary>
        private const int FrameSyncBytes = 6;

        /// <summary>
        /// The header date time format in UTC
        /// </summary>
        private const string HeaderDateTimeFormat = "M/d/yyyy h:mm:ss tt";

        /// <summary>
        /// The quick block fixed header size
        /// </summary>
        private const int QuickBlockHeaderSize = 80;

        /// <summary>
        /// The quick block v1 body size (v2 packets are variable sized up to 1024)
        /// </summary>
        private const int QuickBlockV1BodySize = 1024;

        /// <summary>
        /// The header regex (works with both V1 and V2 headers)
        /// </summary>
        private static readonly Regex HeaderRegex = new Regex(@"^/PF(?<PF>[A-Za-z0-9\-._]+)\s*/PN\s*(?<PN>[0-9]+)\s*/PT\s*(?<PT>[0-9]+)\s*/CS\s*(?<CS>[0-9]+)\s*/FD(?<FD>[0-9/: ]+[AP]M)\s*(/DL(?<DL>[0-9]+)\s*)?\r\n$", RegexOptions.ExplicitCapture | RegexOptions.Compiled);

        /// <summary>
        /// The server list regex which is sent periodically
        /// </summary>
        private static readonly Regex ServerListRegex = new Regex(@"^/ServerList/(?<ServerList>.*?)\\ServerList\\/SatServers/(?<SatServers>.*?)\\SatServers\\$", RegexOptions.Compiled);

        /// <summary>
        /// The channel remote address
        /// </summary>
        private string _remoteAddress;

        #endregion Private Fields

        #region Protected Enums

        /// <summary>
        /// Decoder State
        /// </summary>
        protected enum DecoderState
        {
            ReSync,
            StartFrame,
            FrameType,
            ServerList,
            BlockHeader,
            BlockBody,
            Validate
        }

        #endregion Protected Enums

        #region Public Methods

        /// <summary>
        /// Fired when channel is active.
        /// </summary>
        /// <param name="context">The context.</param>
        public override void ChannelActive(IChannelHandlerContext context)
        {
            _remoteAddress = context.Channel.RemoteAddress.ToString();
            State = DecoderState.ReSync;
            base.ChannelActive(context);
        }

        /// <summary>
        /// Fired when an exception is caught.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="exception">The exception.</param>
        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            State = DecoderState.ReSync;
            ByteBlasterEventSource.Log.Error(context.Name + " Channel Exception", exception);
            PerformanceCounters.DecoderExceptionsTotal.Increment();
            base.ExceptionCaught(context, exception);
        }

        #endregion Public Methods

        #region Protected Methods

        /// <summary>
        /// Decodes the byte buffer and builds QuickBlockTransfer packets.
        /// </summary>
        /// <param name="context">The handler context.</param>
        /// <param name="input">The input byte buffer from the socket.</param>
        /// <param name="output">The output packets.</param>
        protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            switch (State)
            {
                case DecoderState.ReSync:
                    if (!input.IsReadable(QuickBlockV1BodySize + FrameSyncBytes)) break;
                    PerformanceCounters.FrameSyncTotal.Increment();
                    if (!SynchronizeFrame(input)) break;
                    State = DecoderState.StartFrame;
                    goto case DecoderState.StartFrame;

                case DecoderState.StartFrame:
                    if (!SkipNullBytes(input)) break;
                    State = DecoderState.FrameType;
                    goto case DecoderState.FrameType;

                case DecoderState.FrameType:
                    if (!input.IsReadable(QuickBlockHeaderSize)) break;
                    if (IsDataBlockHeader(input))
                    {
                        State = DecoderState.BlockHeader;
                        goto case DecoderState.BlockHeader;
                    }
                    if (IsServerList(input))
                    {
                        PerformanceCounters.ServerListReceivedTotal.Increment();
                        State = DecoderState.ServerList;
                        goto case DecoderState.ServerList;
                    }
                    throw new InvalidOperationException("Unknown frame type");

                case DecoderState.ServerList:
                    var content = ReadString(input);
                    if (content.Length == 0) break;
                    context.FireUserEventTriggered(ParseServerList(content));
                    State = DecoderState.StartFrame;
                    goto case DecoderState.StartFrame;

                case DecoderState.BlockHeader:
                    Packet = ParsePacketHeader(input);
                    PerformanceCounters.BlocksReceivedTotal.Increment();
                    if (Packet.Version == 2)
                        PerformanceCounters.CompressedBlocksReceivedTotal.Increment();
                    State = DecoderState.BlockBody;
                    goto case DecoderState.BlockBody;

                case DecoderState.BlockBody:
                    if (!input.IsReadable(Packet.Length)) break;
                    Packet.Content = ReadPacketBody(input, Packet.Length, Packet.Version);
                    PerformanceCounters.BlocksProcessedPerSecond.Increment();
                    State = DecoderState.Validate;
                    goto case DecoderState.Validate;

                case DecoderState.Validate:
                    if (Packet.TotalBlocks <= 0 || Packet.BlockNumber <= 0)
                    {
                        PerformanceCounters.ChecksumErrorsTotal.Increment();
                        throw new InvalidDataException("Header block values out of range. " + Packet);
                    }

                    if (VerifyChecksum(Packet.Content, Packet.Checksum))
                    {
                        ByteBlasterEventSource.Log.PacketCreated(Packet.ToString());
                        context.FireUserEventTriggered(Packet);
                    }
                    else
                    {
                        PerformanceCounters.ChecksumErrorsTotal.Increment();
                        throw new InvalidDataException("Block Checksum failed. " + Packet);
                    }

                    State = DecoderState.StartFrame;
                    goto case DecoderState.StartFrame;

                default:
                    throw new InvalidOperationException("Unknown Decoder State: " + State);
            }
        }

        #endregion Protected Methods

        #region Private Methods

        /// <summary>
        /// Determines whether the buffer contains a data block header.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns><c>true</c> if the buffer contains a data block header; otherwise, <c>false</c>.</returns>
        private static bool IsDataBlockHeader(IByteBuffer input) =>
            ((input.GetByte(input.ReaderIndex) ^ 0xFF) == '/' &&
             (input.GetByte(input.ReaderIndex + 1) ^ 0xFF) == 'P' &&
             (input.GetByte(input.ReaderIndex + 2) ^ 0xFF) == 'F');

        /// <summary>
        /// Determines whether the buffer contains a server list.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns><c>true</c> if the buffer contains a server list header; otherwise, <c>false</c>.</returns>
        private static bool IsServerList(IByteBuffer input) =>
                    ((input.GetByte(input.ReaderIndex) ^ 0xFF) == '/' &&
                     (input.GetByte(input.ReaderIndex + 1) ^ 0xFF) == 'S' &&
                     (input.GetByte(input.ReaderIndex + 2) ^ 0xFF) == 'e');

        /// <summary>
        /// Parses the Quick Block Transfer V1 header.
        /// </summary>
        /// <param name="groups">The match from the regular expression parser.</param>
        /// <param name="packet">The packet to be populated.</param>
        private static void ParseHeaderV1(GroupCollection groups, QuickBlockTransferSegment packet)
        {
            packet.Version = 1;
            packet.Length = QuickBlockV1BodySize;
            packet.Filename = groups["PF"].Value;
            packet.BlockNumber = int.Parse(groups["PN"].Value);
            packet.TotalBlocks = int.Parse(groups["PT"].Value);
            packet.Checksum = int.Parse(groups["CS"].Value);

            DateTimeOffset ts;
            var dateText = groups["FD"].Value;
            if (DateTimeOffset.TryParseExact(dateText, HeaderDateTimeFormat, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal, out ts))
            {
                packet.TimeStamp = ts;
            }
            else
            {
                ByteBlasterEventSource.Log.ParserWarning("Unable to parse header date", dateText);
            }
        }

        /// <summary>
        /// Parses the Quick Block Transfer V2 header.
        /// </summary>
        /// <param name="groups">The match from the regular expression parser.</param>
        /// <param name="packet">The packet to be populated.</param>
        private void ParseHeaderV2(GroupCollection groups, QuickBlockTransferSegment packet)
        {
            if (!groups["DL"].Success) return;
            
            packet.Version = 2;
            packet.Length = int.Parse(groups["DL"].Value);

            if (packet.Length <= 0 || packet.Length > 1024)
                throw new IndexOutOfRangeException("DL (length) value " + packet.Length + " is out of range (1-1024).");
        }

        /// <summary>
        /// Reads and parses the packet header.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>QuickBlockTransferPacket.</returns>
        private QuickBlockTransferSegment ParsePacketHeader(IByteBuffer input)
        {
            var header = ReadString(input, QuickBlockHeaderSize);

            ByteBlasterEventSource.Log.HeaderReceived(header);
            var match = HeaderRegex.Match(header);
            if (match.Success)
            {
                var packet = new QuickBlockTransferSegment
                {
                    Header = header,
                    Source = _remoteAddress
                };

                ParseHeaderV1(match.Groups, packet);
                ParseHeaderV2(match.Groups, packet);

                return packet;
            }

            throw new InvalidDataException("QuickBlockPacketDecoder Unknown header: " + header);
        }

        /// <summary>
        /// Parses the server list.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns>ServerList.</returns>
        private ByteBlasterServerList ParseServerList(string content)
        {
            var match = ServerListRegex.Match(content);
            if (!match.Success)
                throw new InvalidDataException("QuickBlockPacketDecoder: Unable to parse server List: " + content);

            ByteBlasterEventSource.Log.ServerList(content);
            var serverList = match.Groups["ServerList"].Value.Split(new [] {'|'}, StringSplitOptions.RemoveEmptyEntries);
            var satServers = match.Groups["SatServers"].Value.Split(new[] {'+'}, StringSplitOptions.RemoveEmptyEntries);
            return new ByteBlasterServerList(serverList, satServers) { ReceivedAt = DateTimeOffset.UtcNow };
        }

        /// <summary>
        /// Reads the packet body.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="size">The size.</param>
        /// <param name="version">The packet version.</param>
        /// <returns>System.Byte[].</returns>
        private static byte[] ReadPacketBody(IByteBuffer input, int size, byte version)
        {
            var body = XorArray(input.ReadBytes(size).ToArray());

            if (version == 2)
                body = ZLibInflate(body);

            return body;
        }

        /// <summary>
        /// Reads the string from the byte buffer.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="count">The count.</param>
        /// <returns>System.String.</returns>
        private static string ReadString(IByteBuffer input, int count)
        {
            var bytes = XorArray(input.ReadBytes(count).ToArray());
            return Encoding.ASCII.GetString(bytes);
        }

        /// <summary>
        /// Reads the string from the byte buffer until null terminator.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>System.String.</returns>
        private static string ReadString(IByteBuffer input)
        {
            // Scan buffer for end of string null terminator
            for (var index = input.ReaderIndex; index < input.WriterIndex; index++)
            {
                if (input.GetByte(index) != 0xFF) continue;

                var bytes = XorArray(input.ReadBytes(index - input.ReaderIndex).ToArray());
                return Encoding.ASCII.GetString(bytes);
            }

            return string.Empty;
        }

        /// <summary>
        /// Skips the null bytes.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        private static bool SkipNullBytes(IByteBuffer input)
        {
            while (input.IsReadable())
            {
                if (input.GetByte(input.ReaderIndex) != 0xFF) return true;
                input.SkipBytes(1);
            }

            return false;
        }

        /// <summary>
        /// Synchronizes the frame.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        private static bool SynchronizeFrame(IByteBuffer input)
        {
            var count = 0;
            while (input.IsReadable())
            {
                count = (input.ReadByte() == 0xFF) ? count + 1 : 0;
                if (count >= FrameSyncBytes) return true;
            }

            ByteBlasterEventSource.Log.SynchronizationWarning(count);
            return false;
        }

        /// <summary>
        /// Verifies the checksum against the body bytes.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <param name="checksum">The checksum.</param>
        /// <returns><c>true</c> if checksum matches, <c>false</c> otherwise.</returns>
        private static bool VerifyChecksum(IEnumerable<byte> body, int checksum) => checksum == body.Sum(b => b);

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

        /// <summary>
        /// Inflates the specified bytes using ZLib decompression.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns>System.Byte[].</returns>
        private static byte[] ZLibInflate(byte[] bytes)
        {
            // Skip first 2 bytes because DeflateStream doesn't understand the compression type header
            using (var inflater = new DeflateStream(new MemoryStream(bytes, 2, bytes.Length - 2), CompressionMode.Decompress))
            using (var output = new MemoryStream())
            {
                inflater.CopyTo(output);
                return output.ToArray();
            }
        }

        #endregion Private Methods

    }
}
