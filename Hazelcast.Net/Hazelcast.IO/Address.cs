// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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
        // ReSharper disable once InconsistentNaming
        private const byte IPv4 = 4;

        // ReSharper disable once InconsistentNaming
        private const byte IPv6 = 6;

        private readonly string _host;
        private readonly bool _hostSet;
        private readonly int _port = -1;
        private readonly byte _type;
        private string _scopeId;

        public Address()
        {
        }

        public Address(string host, int port) : this(host, GetAddressByName(host), port)
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

        public Address(string hostname, IPAddress ipAddress, int port)
        {
            if (ipAddress == null)
            {
                throw new ArgumentNullException("ipAddress");
            }
            _type = (ipAddress.AddressFamily == AddressFamily.InterNetwork) ? IPv4 : IPv6;
            var addressArgs = ipAddress.ToString().Split('%');
            _host = hostname ?? addressArgs[0];
            if (addressArgs.Length == 2)
            {
                _scopeId = addressArgs[1];
            }
            _port = port;
            _hostSet = !AddressUtil.IsIpAddress(_host);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Address && Equals((Address) obj);
        }

        public static IPAddress GetAddressByName(string name)
        {
            if (name == "0.0.0.0")
            {
                return IPAddress.Any;
            }
            var addresses = DnsUtil.GetHostAddresses(name);
            var ipv4 = addresses.FirstOrDefault(m => m.AddressFamily == AddressFamily.InterNetwork);
            return ipv4 ?? addresses.FirstOrDefault();
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _port;
                hashCode = (hashCode*397) ^ (_host != null ? _host.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ _type.GetHashCode();
                return hashCode;
            }
        }

        public string GetHost()
        {
            return _host;
        }

        public IPAddress GetInetAddress()
        {
            return GetAddressByName(GetScopedHost());
        }

        public IPEndPoint GetInetSocketAddress()
        {
            return new IPEndPoint(GetInetAddress(), _port);
        }

        public int GetPort()
        {
            return _port;
        }

        public string GetScopedHost()
        {
            return (IsIPv4() || _hostSet || _scopeId == null) ? GetHost() : GetHost() + "%" + _scopeId;
        }

        public string GetScopeId()
        {
            return IsIPv6() ? _scopeId : null;
        }

        public bool IsIPv4()
        {
            return _type == IPv4;
        }

        public bool IsIPv6()
        {
            return _type == IPv6;
        }

        public void SetScopeId(string scopeId)
        {
            if (IsIPv6())
            {
                _scopeId = scopeId;
            }
        }

        public override string ToString()
        {
            return "Address[" + GetHost() + "]:" + _port;
        }

        private bool Equals(Address other)
        {
            return _port == other._port && string.Equals(_host, other._host) && _type == other._type;
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