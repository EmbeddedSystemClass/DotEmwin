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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Emwin.ByteBlaster
{
    /// <summary>
    /// Class ByteBlasterServerList.
    /// </summary>
    public class ByteBlasterServerList
    {

        #region Public Fields

        public static string[] DefaultSatServers = { };

        public static string[] DefaultServers = {
            "w2.2y.net:2211",
            "140.90.128.133:1000",
            "2.pool.iemwin.net:2211",
            "4.pool.iemwin.net:2211",
            "woot.moses.bz:2211",
            "woot2.moses.bz:2211",
            "3.pool.iemwin.net:2211",
            "1.pool.iemwin.net:2211",
            "6.pool.iemwin.net:2211",
            "emwin.aprsfl.net:2211",
            "67.217.13.122:2211",
            "apollo2.netwrx1.com:1000",
            "mail.sacsoftware.com:2211",
            "140.90.6.245:1000",
            "140.90.128.146:1000",
            "140.90.6.240:1000",
            "emwin.ewarn.org:2211",
            "140.90.24.30:22"
        };

        #endregion Public Fields

        #region Private Fields

        private readonly List<EndPoint> _satServers;

        private readonly List<EndPoint> _servers;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ByteBlasterServerList"/> class.
        /// </summary>
        public ByteBlasterServerList() : this(DefaultServers, DefaultSatServers) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ByteBlasterServerList" /> class.
        /// </summary>
        /// <param name="servers">The servers.</param>
        /// <param name="satServers">The satServers.</param>
        public ByteBlasterServerList(IEnumerable<string> servers, IEnumerable<string> satServers)
        {
            _servers = servers.Select(ToEndPoint).ToList();
            _satServers = satServers.Select(ToEndPoint).ToList();

            var rng = new Random();
            Shuffle(rng, _servers);
            Shuffle(rng, _satServers);
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// Gets or sets the received at time.
        /// </summary>
        /// <value>The received at.</value>
        public DateTimeOffset ReceivedAt { get; set; }

        /// <summary>
        /// Gets or sets the sat servers.
        /// </summary>
        /// <value>The sat servers.</value>
        public IReadOnlyList<EndPoint> SatServers => _satServers;

        /// <summary>
        /// Gets or sets the servers.
        /// </summary>
        /// <value>The servers.</value>
        public IReadOnlyList<EndPoint> Servers => _servers;

        #endregion Public Properties

        #region Private Methods

        /// <summary>
        /// Shuffles the specified array using the passed in random number generator.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rng">The RNG.</param>
        /// <param name="array">The array.</param>
        private static void Shuffle<T>(Random rng, IList<T> array)
        {
            var n = array.Count;
            while (n > 1)
            {
                var k = rng.Next(n--);
                var temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
        }

        /// <summary>
        /// Converts host and port string to appropriate end point.
        /// </summary>
        /// <param name="hostAndPortString">The host and point string.</param>
        /// <returns>System.Net.EndPoint.</returns>
        private static EndPoint ToEndPoint(string hostAndPortString)
        {
            var split = hostAndPortString.Split(':');
            var hostName = split[0];
            var portNum = int.Parse(split[1]);
            IPAddress ipAddress;
            if (IPAddress.TryParse(hostName, out ipAddress))
                return new IPEndPoint(ipAddress, portNum);
            return new DnsEndPoint(hostName, portNum, AddressFamily.InterNetwork);
        }

        #endregion Private Methods
    }
}
