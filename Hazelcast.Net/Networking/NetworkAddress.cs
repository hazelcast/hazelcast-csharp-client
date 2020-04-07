﻿using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Hazelcast.Networking
{
    /// <summary>
    /// Represents a network address.
    /// </summary>
    public class NetworkAddress
    {
        // NOTES
        //
        // an IP v4 address is 'x.x.x.x' where each octet 'x' is a byte (8 bits unsigned)
        // an IP v4 endpoint is 'x.x.x.x:p' where 'p' is the port number
        //
        // an IP v6 address can be normal (pure v6) or dual (v6 + v4)
        //  normal is 'y:y:y:y:y:y:y:y' where each segment 'y' is a word (16 bits unsigned)
        //  dual is 'y:y:y:y:y:y:x.x.x.x'
        //  missing segments are assumed to be zeros
        //
        // it can also be 'y:y:y:y:y:y:y:y%i' where 'i' is the scope id (a number, 'eth0'..)
        // read
        // https://superuser.com/questions/99746/why-is-there-a-percent-sign-in-the-ipv6-address
        // https://docs.microsoft.com/en-us/previous-versions/aa917150(v=msdn.10)
        // which explain the 'node-local' vs 'link-local' vs 'global' scopes

        // TODO revisit original code
        // - hostname vs scopeID, can we do www.hazelcast.com%33, etc.
        // - get 'possible addresses' etc.

        public const int DefaultPort = 5701;

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkAddress"/> class with a hostname and a port.
        /// </summary>
        /// <param name="hostName">The hostname.</param>
        /// <param name="port">The port.</param>
        public NetworkAddress(string hostName, int port = DefaultPort)
            : this(hostName, GetIPAddressByName(hostName), port)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkAddress"/> class with an IP address and a port.
        /// </summary>
        /// <param name="ipAddress">The IP address.</param>
        /// <param name="port">The port.</param>
        public NetworkAddress(IPAddress ipAddress, int port = DefaultPort)
            : this(null, ipAddress, port)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkAddress"/> with an IP endpoint.
        /// </summary>
        /// <param name="endpoint">The IP endpoint.</param>
        internal NetworkAddress(IPEndPoint endpoint)
        {
            IPEndPoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));

            var ipAddress = IPEndPoint.Address;
            if (ipAddress == null) throw new ArgumentException("Address cannot be null.", nameof(IPEndPoint));

            //SetHostName(ipAddress);
            HostName = ipAddress.ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkAddress"/>.
        /// </summary>
        /// <param name="hostName">The hostname.</param>
        /// <param name="ipAddress">The IP address.</param>
        /// <param name="port">The port.</param>
        /// <remarks>
        /// <para>The <paramref name="hostName"/> and <paramref name="ipAddress"/> are assumed to be consistent.</para>
        /// </remarks>
        private NetworkAddress(string hostName, IPAddress ipAddress, int port)
        {
            if (ipAddress == null) throw new ArgumentNullException(nameof(ipAddress));
            if (port < 0) throw new ArgumentOutOfRangeException(nameof(port));

            IPEndPoint = new IPEndPoint(ipAddress, port);

            HostName = hostName;
            if (string.IsNullOrWhiteSpace(hostName))
                //SetHostName(ipAddress);
                HostName = ipAddress.ToString();
            else
                HostName = hostName;
        }

        // TODO consider removing this code
        /*
        /// <summary>
        /// Sets the hostname from an address.
        /// </summary>
        /// <param name="ipAddress">The address.</param>
        private void SetHostName(IPAddress ipAddress)
        {
            // need to figure out a hostname...
            // IPv6 might have a scope id, remove it
            var s = ipAddress.ToString();
            var p = s.IndexOf('%');
            HostName = p < 0 ? s : s.Substring(0, p);
        }
        */

        /// <summary>
        /// Gets or sets the host name.
        /// </summary>
        public string HostName { get; }

        /// <summary>
        /// Gets the port.
        /// </summary>
        public int Port => IPEndPoint.Port;

        /// <summary>
        /// Gets the IP address corresponding to this address.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public IPAddress IPAddress => IPEndPoint.Address;

        /// <summary>
        /// Gets the IP endpoint corresponding to this address.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public IPEndPoint IPEndPoint { get; }

        /// <summary>
        /// Gets an IP address by name, via DNS.
        /// </summary>
        /// <param name="hostname">The hostname.</param>
        /// <returns>The corresponding IP address.</returns>
        /// <remarks>
        /// <para>Returns the first IP v4 address available, if any,
        /// else the first IP v6 address available. Throws if it cannot
        /// get an IP for the hostname via DNS.</para>
        /// </remarks>
        // ReSharper disable once InconsistentNaming
        public static IPAddress GetIPAddressByName(string hostname)
        {
            if (hostname == "0.0.0.0") return IPAddress.Any;

            var addresses = Dns.GetHostAddresses(hostname);

            // prefer an IP v4, if possible
            return addresses.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork) ??
                   addresses.FirstOrDefault();
        }

        /// <summary>
        /// Tries to parse a string into a <see cref="NetworkAddress"/> instance.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <param name="address">The network address.</param>
        /// <returns>Whether the string could be parsed into an address.</returns>
        public static bool TryParse(string s, out NetworkAddress address)
        {
            address = null;

            var span = s.AsSpan();
            var colon1 = span.IndexOf(':');
            var colon2 = span.LastIndexOf(':');
            var brket1 = span.IndexOf('[');
            var brket2 = span.IndexOf(']');
            var port = DefaultPort;

            // opening bracket must be first
            if (brket1 > 0) return false;

            // must have both brackets, or none
            if (brket1 == 0 != brket2 >= 0) return false;

            // brackets => colon if any *must* be right after closing bracket
            if (brket1 == 0 && colon2 > brket2 + 1) return false;

            // no bracket and single colon, or one colon after brackets
            // => parse port
            if ((brket2 < 0 && colon2 > 0 && colon1 == colon2) ||
                (brket2 > 0 && colon2 > brket2))
            {
                if (!int.TryParse(span.Slice(colon2 + 1), out port))
                    return false;
            }

            ReadOnlySpan<char> hostName;

            if (brket1 == 0)
            {
                // if we have brackets, they must contain colons
                if (colon2 < 0 || colon1 > brket2) return false;

                // hostname is in-between brackets
                // (and has to be parseable as an ip address)
                hostName = span.Slice(1, brket2 - 1);
            }
            else
            {
                // one single colon = hostname is whatever is before
                // otherwise, hostname is the whole string
                hostName = (colon2 > 0 && colon1 == colon2) ? span.Slice(0, colon2) : span;
            }

            // must have a hostname
            if (hostName.Length == 0)
                return false;

            string hostNameString;

            if (IPAddress.TryParse(hostName, out var ipAddress))
            {
                // if the hostname parses to an ip address, fine
                hostNameString = ipAddress.ToString();
            }
            else
            {
                // if we have brackets, hostname must be parseable
                if (brket1 == 0) return false;

                hostNameString = hostName.ToString();

                // else, try to get the ip via DNS
                try
                {
                    ipAddress = GetIPAddressByName(hostNameString);
                }
                catch
                {
                    return false;
                }
            }

            address = new NetworkAddress(hostNameString, ipAddress, port);
            return true;
        }

        //public static bool TryParse2(string s, out ICollection<NetworkAddress> addresses)
        //{ }

        /// <inheritdoc />
        public override string ToString()
        {
            return IPAddress.AddressFamily == AddressFamily.InterNetworkV6
                ? $"[{HostName}]:{Port}"
                : $"{HostName}:{Port}";
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return IPEndPoint.GetHashCode();
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is NetworkAddress other && other.IPEndPoint == IPEndPoint;
        }
    }
}
