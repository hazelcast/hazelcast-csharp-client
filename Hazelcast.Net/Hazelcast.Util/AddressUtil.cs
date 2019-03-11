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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Hazelcast.IO;
using Hazelcast.Logging;

namespace Hazelcast.Util
{
    internal class AddressHolder
    {
        public readonly string Address;

        public readonly int Port;
        public readonly string ScopeId;

        public AddressHolder(string address, int port, string scopeId)
        {
            // ----------------- UTILITY CLASSES ------------------
            Address = address;
            ScopeId = scopeId;
            Port = port;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("AddressHolder ");
            sb.Append('[').Append(Address).Append("]:").Append(Port);
            return sb.ToString();
        }
    }

    internal static class AddressUtil
    {
        private const int MaxPortTries = 3;

        public static AddressHolder GetAddressHolder(string address, int defaultPort)
        {
            var indexBracketStart = address.IndexOf('[');
            var indexBracketEnd = indexBracketStart >= 0 ? address.IndexOf(']', indexBracketStart) : -1;
            var indexColon = address.IndexOf(':');
            var lastIndexColon = address.LastIndexOf(':');
            string host;
            var port = defaultPort;
            string scopeId = null;
            if (indexColon > -1 && lastIndexColon > indexColon)
            {
                // IPv6
                if (indexBracketStart == 0 && indexBracketEnd > indexBracketStart)
                {
                    host = address.Substring(indexBracketStart + 1, indexBracketEnd - indexBracketStart - 1);
                    if (lastIndexColon == indexBracketEnd + 1)
                    {
                        port = Convert.ToInt32(address.Substring(lastIndexColon + 1));
                    }
                }
                else
                {
                    host = address;
                }
                var indexPercent = host.IndexOf('%');
                if (indexPercent != -1)
                {
                    scopeId = host.Substring(indexPercent + 1);
                    host = host.Substring(0, indexPercent);
                }
            }
            else
            {
                if (indexColon > 0 && indexColon == lastIndexColon)
                {
                    host = address.Substring(0, indexColon);
                    port = Convert.ToInt32(address.Substring(indexColon + 1));
                }
                else
                {
                    host = address;
                }
            }
            return new AddressHolder(host, port, scopeId);
        }

        private static ICollection<IPAddress> GetPossibleIpAddressesFor(IPAddress ipAddress)
        {
            if ((ipAddress.IsIPv6SiteLocal || ipAddress.IsIPv6LinkLocal) && ipAddress.ScopeId <= 0)
            {
                var possibleAddresses = new List<IPAddress>();
                var strHostName = Dns.GetHostName();
                var ipEntry = Dns.GetHostEntry(strHostName);
                var addr = ipEntry.AddressList;
                foreach (var address in addr)
                {
                    if (address.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        possibleAddresses.Insert(0, address);
                    }
                }
                if (possibleAddresses.Count == 0)
                {
                    throw new ArgumentException("Could not find a proper network interface" + " to connect to " + ipAddress);
                }
                return possibleAddresses;
            }
            return new List<IPAddress>(1) {ipAddress};
        }

        public static bool IsIpAddress(string address)
        {
            try
            {
                IPAddress ipAddress;
                IPAddress.TryParse(address, out ipAddress);

                return (ipAddress != null);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static ICollection<Address> ParsePossibleAddresses(string address)
        {
            var addressHolder = GetAddressHolder(address, -1);
            var scopedAddress = addressHolder.ScopeId != null
                ? addressHolder.Address + "%" + addressHolder.ScopeId
                : addressHolder.Address;
            IPAddress ipAddress = null;
            try
            {
                ipAddress = Address.GetAddressByName(scopedAddress);
            }
            catch (Exception)
            {
                Logger.GetLogger(typeof(AddressUtil)).Finest("Address not available");
            }
            var port = addressHolder.Port;
            var portTryCount = 1;
            if (port == -1)
            {
                portTryCount = MaxPortTries;
                port = 5701;
            }
            ICollection<Address> socketAddresses = new List<Address>();
            if (ipAddress == null)
            {
                for (var i = 0; i < portTryCount; i++)
                {
                    if (IPAddress.TryParse(scopedAddress, out ipAddress))
                    {
                        socketAddresses.Add(new Address(scopedAddress, ipAddress, port + i));
                    }
                }
            }
            else
            {
                if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    for (var i = 0; i < portTryCount; i++)
                    {
                        socketAddresses.Add(new Address(scopedAddress, ipAddress, port + i));
                    }
                }
                else
                {
                    var addresses = GetPossibleIpAddressesFor(ipAddress);
                    foreach (var ipa in addresses)
                    {
                        for (var i = 0; i < portTryCount; i++)
                        {
                            socketAddresses.Add(new Address(scopedAddress, ipa, port + i));
                        }
                    }
                }
            }
            return socketAddresses;
        }

        public static Address ParseSocketAddress(string address)
        {
            var addressHolder = GetAddressHolder(address, -1);
            var scopedAddress = addressHolder.ScopeId != null
                ? addressHolder.Address + "%" + addressHolder.ScopeId
                : addressHolder.Address;
            try
            {
                var ipAddress = Address.GetAddressByName(scopedAddress);
                return new Address(scopedAddress, ipAddress, addressHolder.Port);
            }
            catch (Exception)
            {
                Logger.GetLogger(typeof(AddressUtil)).Finest("Address not available");
            }

            return null;
        }

        public static List<Address> Shuffle(IEnumerable<Address> list)
        {
            var r = new Random();
            return list.OrderBy(x => r.Next()).ToList();
        }

        [Serializable]
        internal class InvalidAddressException : ArgumentException
        {
            public InvalidAddressException(string s) : base("Illegal IP address format: " + s)
            {
            }
        }
    }
}