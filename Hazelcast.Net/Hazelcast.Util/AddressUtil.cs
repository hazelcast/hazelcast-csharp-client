using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace Hazelcast.Util
{
    internal class AddressHolder
    {
        public readonly string address;

        public readonly int port;
        public readonly string scopeId;

        public AddressHolder(string address, int port, string scopeId)
        {
            // ----------------- UTILITY CLASSES ------------------
            this.address = address;
            this.scopeId = scopeId;
            this.port = port;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("AddressHolder ");
            sb.Append('[').Append(address).Append("]:").Append(port);
            return sb.ToString();
        }
    }

    internal static class AddressUtil
    {
        public static AddressHolder GetAddressHolder(string address, int defaultPort)
        {
            int indexBracketStart = address.IndexOf('[');
            int indexBracketEnd = indexBracketStart >= 0 ? address.IndexOf(']', indexBracketStart) : -1;
            int indexColon = address.IndexOf(':');
            int lastIndexColon = address.LastIndexOf(':');
            string host;
            int port = defaultPort;
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
                int indexPercent = host.IndexOf('%');
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

        public static bool IsIpAddress(string address)
        {
            try
            {
                IPAddress ipAddress = null;
                IPAddress.TryParse(address, out ipAddress);

                return (ipAddress != null);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static ICollection<IPAddress> GetPossibleInetAddressesFor(IPAddress ipAddress)
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
                    throw new ArgumentException("Could not find a proper network interface" + " to connect to " +
                                                ipAddress);
                }
                return possibleAddresses;
            }
            return new List<IPAddress>(1) {ipAddress};
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