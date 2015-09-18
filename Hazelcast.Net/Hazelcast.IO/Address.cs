using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Hazelcast.Util;

namespace Hazelcast.IO
{
    /// <summary>Represents an address of a member in the cluster.</summary>
    /// <remarks>Represents an address of a member in the cluster.</remarks>
    public sealed class Address
    {
        public const int Id = 1;
        private const byte IPv4 = 4;
        private const byte IPv6 = 6;
        private readonly string host;
        private readonly bool hostSet;
        private readonly int port = -1;
        private readonly byte type;
        private string scopeId;

        public Address()
        {
        }

        public Address(string host, int port) : this(GetAddressByName(host), port)
        {
        }

        public Address(IPAddress inetAddress, int port) : this(null, inetAddress, port)
        {
        }

        /// <summary>Creates a new Address</summary>
        /// <param name="inetSocketAddress">the InetSocketAddress to use</param>
        /// <exception cref="System.ArgumentNullException">if inetSocketAddress is null</exception>
        /// <exception cref="System.ArgumentException">if the address can't be resolved.</exception>
        public Address(IPEndPoint inetSocketAddress) : this(Resolve(inetSocketAddress), inetSocketAddress.Port)
        {
        }

        public Address(string hostname, IPAddress inetAddress, int port)
        {
            if (inetAddress == null)
            {
                throw new ArgumentNullException("inetAddress");
            }
            type = (inetAddress.AddressFamily == AddressFamily.InterNetwork) ? IPv4 : IPv6;
            var addressArgs = inetAddress.ToString().Split('%');
            host = hostname ?? addressArgs[0];
            if (addressArgs.Length == 2)
            {
                scopeId = addressArgs[1];
            }
            this.port = port;
            hostSet = !AddressUtil.IsIpAddress(host);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Address && Equals((Address) obj);
        }

        public static IPAddress GetAddressByName(string name)
        {
            return name == "0.0.0.0" ? IPAddress.Any : Dns.GetHostAddresses(name).FirstOrDefault();
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = port;
                hashCode = (hashCode*397) ^ (host != null ? host.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ type.GetHashCode();
                return hashCode;
            }
        }

        public string GetHost()
        {
            return host;
        }

        /// <exception cref="Hazelcast.Net.Ext.UnknownHostException"></exception>
        public IPAddress GetInetAddress()
        {
            return GetAddressByName(GetScopedHost());
        }

        /// <exception cref="Hazelcast.Net.Ext.UnknownHostException"></exception>
        public IPEndPoint GetInetSocketAddress()
        {
            return new IPEndPoint(GetInetAddress(), port);
        }

        public int GetPort()
        {
            return port;
        }

        public string GetScopedHost()
        {
            return (IsIPv4() || hostSet || scopeId == null) ? GetHost() : GetHost() + "%" + scopeId;
        }

        public string GetScopeId()
        {
            return IsIPv6() ? scopeId : null;
        }

        public bool IsIPv4()
        {
            return type == IPv4;
        }

        public bool IsIPv6()
        {
            return type == IPv6;
        }

        public void SetScopeId(string scopeId)
        {
            if (IsIPv6())
            {
                this.scopeId = scopeId;
            }
        }

        public override string ToString()
        {
            return "Address[" + GetHost() + "]:" + port;
        }

        private bool Equals(Address other)
        {
            return port == other.port && string.Equals(host, other.host) && type == other.type;
        }

        private static IPAddress Resolve(IPEndPoint inetSocketAddress)
        {
            if (inetSocketAddress == null)
            {
                throw new ArgumentNullException("inetSocketAddress");
            }
            var address = inetSocketAddress.Address;
            if (address == null)
            {
                throw new ArgumentException("Can't resolve address: " + inetSocketAddress);
            }
            return address;
        }
    }
}