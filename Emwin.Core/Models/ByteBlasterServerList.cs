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
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Emwin.Core.Models
{
    /// <summary>
    /// Class ByteBlasterServerList.
    /// </summary>
    public class ByteBlasterServerList
    {

        #region Public Fields

        public static string[] DefaultSatServers = { };

        public static string[] DefaultServers = {
            "140.90.128.133:1000", "140.90.6.245:1000", "140.90.128.132:1000",
            "140.90.128.146:1000", "140.90.6.240:1000"
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
        /// <param name="hostAndPointString">The host and point string.</param>
        /// <returns>System.Net.EndPoint.</returns>
        private static EndPoint ToEndPoint(string hostAndPointString)
        {
            var split = hostAndPointString.Split(':');
            var hostName = split[0];
            var portNum = int.Parse(split[1]);
            IPAddress ipAddress;
            if (IPAddress.TryParse(hostName, out ipAddress))
                return new IPEndPoint(ipAddress, portNum);
            return new DnsEndPoint(hostName, portNum);
        }

        #endregion Private Methods
    }
}
