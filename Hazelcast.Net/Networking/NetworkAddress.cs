using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;

namespace Hazelcast.Networking
{
    /// <summary>
    /// Represents a network address.
    /// </summary>
    public class NetworkAddress
    {
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1);

        // NOTES
        //
        // an IP v4 address is 'x.x.x.x' where each octet 'x' is a byte (8 bits unsigned)
        //
        // an IP v4 endpoint is '<address>:p' where 'p' is the port number
        //
        // an IP v6 address can be normal (pure v6) or dual (v6 + v4)
        //  normal is 'y:y:y:y:y:y:y:y' where each segment 'y' is a word (16 bits unsigned)
        //  dual is 'y:y:y:y:y:y:x.x.x.x'
        //  missing segments are assumed to be zeros
        //
        // it can also be 'y:y:y:y:y:y:y:y%i' where 'i' is the scope id (a number, 'eth0'..)
        //
        // an IP v6 endpoint is '[<address>]:p' where 'p' is the port number
        //
        // read
        // https://superuser.com/questions/99746/why-is-there-a-percent-sign-in-the-ipv6-address
        // https://docs.microsoft.com/en-us/previous-versions/aa917150(v=msdn.10)
        // which explain the 'node-local' vs 'link-local' vs 'global' scopes

        // TODO revisit original code
        // - hostname vs scopeID, can we do www.hazelcast.com%33, etc.
        // - get 'possible addresses' etc.

        /// <summary>
        /// Gets the default Hazelcast server port.
        /// </summary>
        public const int DefaultPort = 5701;

        /// <summary>
        /// Gets the port range to scan.
        /// </summary>
        public const int PortRange = 3;

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

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkAddress"/>.
        /// </summary>
        /// <param name="source">The origin address.</param>
        /// <param name="port">The port.</param>
        private NetworkAddress(NetworkAddress source, int port)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (port < 0) throw new ArgumentOutOfRangeException(nameof(port));

            IPEndPoint = new IPEndPoint(source.IPAddress, port);
            HostName = source.HostName;
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
        /// Locks the address.
        /// </summary>
        /// <returns>A disposable object that needs to be disposed to release the lock.</returns>
#if OPTIMIZE_ASYNC
        public ValueTask<IDisposable> LockAsync()
            => new SemaphoreDisposableReleaser(_lock).WaitAsync();
#else
        public async ValueTask<IDisposable> LockAsync()
            => await new LockAcquisition(_lock).WaitAsync();
#endif

        /// <summary>
        /// Tries to the address.
        /// </summary>
        /// <returns>A disposable object that needs to be disposed to release the lock.</returns>
#if OPTIMIZE_ASYNC
        public ValueTask<LockAquisition> TryLockAsync()
            => new LockAquisition(_lock).TryWaitAsync();
#else
        public async ValueTask<LockAcquisition> TryLockAsync()
            => await new LockAcquisition(_lock).TryWaitAsync();
#endif

        /// <summary>
        /// Gets the host name.
        /// </summary>
        public string HostName { get; }

        /// <summary>
        /// Gets the host name (prefer <see cref="HostName"/>).
        /// </summary>
        /// <returns>
        /// <para>This property returns <see cref="HostName"/> and is required by codecs
        /// that use names derived from the protocol definitions, because the protocol
        /// codecs generator does not know yet how to map / rename properties.</para>
        /// </returns>
        internal string Host => HostName;

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
        /// Whether the address is an IP v4 address.
        /// </summary>
        public bool IsIpV4 => IPAddress.AddressFamily == AddressFamily.InterNetwork;

        /// <summary>
        /// Whether the address is an IP v6 address.
        /// </summary>
        public bool IsIpV6 => IPAddress.AddressFamily == AddressFamily.InterNetworkV6;

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
        /// Parses a string into a <see cref="NetworkAddress"/>.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <returns>The network address.</returns>
        public static NetworkAddress Parse(string s)
        {
            if (TryParse(s, out NetworkAddress address)) return address;
            throw new FormatException($"The string \"{s}\" does not represent a valid network address.");
        }

        /// <summary>
        /// Tries to parse a string into a <see cref="NetworkAddress"/> instance.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <param name="address">The network address.</param>
        /// <returns>Whether the string could be parsed into an address.</returns>
        public static bool TryParse(string s, out NetworkAddress address)
            => TryParse(s, out address, DefaultPort);

        /// <summary>
        /// Tries to parse a string into a <see cref="NetworkAddress"/> instance.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <param name="address">The network address.</param>
        /// <param name="defaultPort">The default port.</param>
        /// <returns>Whether the string could be parsed into an address.</returns>
        private static bool TryParse(string s, out NetworkAddress address, int defaultPort)
        {
            address = null;

            var span = s.AsSpan();
            var colon1 = span.IndexOf(':');
            var colon2 = span.LastIndexOf(':');
            var brket1 = span.IndexOf('[');
            var brket2 = span.IndexOf(']');
            var port = defaultPort;

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
#if NETSTANDARD2_0
                if (!int.TryParse(span.Slice(colon2 + 1).ToString(), out port))
                    return false;
#endif
#if NETSTANDARD2_1
                if (!int.TryParse(span.Slice(colon2 + 1), out port))
                    return false;
#endif
            }

            ReadOnlySpan<char> hostName;

#pragma warning disable IDE0057 // Slice can be simplified

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

#pragma warning restore IDE0057 // Slice can be simplified

            // must have a hostname
            if (hostName.Length == 0)
                return false;

            // note that in IPv6 case, hostname can contain a % followed by a scope id
            // which is fine, IPAddress.TryParse handles it

            string hostNameString;

#if NETSTANDARD2_0
            if (IPAddress.TryParse(hostName.ToString(), out var ipAddress))
#endif
#if NETSTANDARD2_1
            if (IPAddress.TryParse(hostName, out var ipAddress))
#endif
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

        /// <summary>
        /// Tries to parse a string into all possible <see cref="NetworkAddress"/> instance.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <param name="addresses">The network addresses.</param>
        /// <returns>Whether the string could be parsed into addresses.</returns>
        public static bool TryParse(string s, out IEnumerable<NetworkAddress> addresses)
        {
            if (!TryParse(s, out var address, 0))
            {
                addresses = Enumerable.Empty<NetworkAddress>();
                return false;
            }

            if (address.IsIpV4)
            {
                var list4 = new List<NetworkAddress>();
                var port4 = address.Port > 0 ? address.Port : DefaultPort;
                var range = address.Port > 0 ? 1 : PortRange;
                for (var i = 0; i < range; port4++, i++)
                {
                    list4.Add(new NetworkAddress(address, port4));
                }

                addresses = list4;
                return true;
            }

            if (!address.IsIpV6)
                throw new NotSupportedException($"Address family {address.IPAddress.AddressFamily} is not supported.");

            var list6 = new List<NetworkAddress>();
            foreach (var ipAddress in ExplodeV6Local(address.IPAddress))
            {
                var port6 = address.Port > 0 ? address.Port : DefaultPort;
                var range = address.Port > 0 ? 1 : PortRange;
                for (var i = 0; i < range; i++)
                {
                    list6.Add(new NetworkAddress(address.HostName, ipAddress, port6));
                }
            }

            addresses = list6;
            return true;
        }

        /// <summary>
        /// Gets all scoped IP addresses corresponding to a non-scoped IP v6 local address.
        /// </summary>
        /// <param name="ipAddress">The non-scoped IP v6 local address.</param>
        /// <returns>All scoped IP addresses corresponding to the specified address.</returns>
        /// <remarks>
        /// <para>If <paramref name="ipAddress"/> is not local, or scoped, return it unchanged.</para>
        /// </remarks>
        private static IEnumerable<IPAddress> ExplodeV6Local(IPAddress ipAddress)
        {
            if (!ipAddress.IsIPv6SiteLocal && !ipAddress.IsIPv6LinkLocal ||
                ipAddress.ScopeId > 0)
            {
                yield return ipAddress;
            }

            // if the address is IP v6 local without a scope,
            // resolve

            var hostname = Dns.GetHostName();
            var entry = Dns.GetHostEntry(hostname);
            foreach (var address in entry.AddressList)
            {
                if (address.AddressFamily == AddressFamily.InterNetworkV6)
                    yield return address;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return IPAddress.AddressFamily == AddressFamily.InterNetworkV6
                ? $"[{HostName}]:{Port}"
                : $"{HostName}:{Port}";
        }

        /// <inheritdoc />
        public override int GetHashCode() => IPEndPoint.GetHashCode();

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is NetworkAddress other && Equals(this, other);
        }

        /// <summary>
        /// Compares two instances for equality.
        /// </summary>
        private static bool Equals(NetworkAddress a1, NetworkAddress a2)
        {
            // return true if both are null or both are the same instance
            if (ReferenceEquals(a1, a2)) return true;

            // return false if either is null since the other cannot be null
            if (a1 is null || a2 is null) return false;

            // actual comparison
            return a1.IPEndPoint.Equals(a2.IPEndPoint);
        }

        /// <summary>
        /// Overrides the == operator.
        /// </summary>
        public static bool operator ==(NetworkAddress a1, NetworkAddress a2)
            => Equals(a1, a2);

        /// <summary>
        /// Overrides the != operator.
        /// </summary>
        public static bool operator !=(NetworkAddress a1, NetworkAddress a2)
            => !(a1 == a2);
    }
}
