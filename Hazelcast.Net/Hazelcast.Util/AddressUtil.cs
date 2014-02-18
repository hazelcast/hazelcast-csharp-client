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

    internal sealed class AddressUtil
    {
        private AddressUtil()
        {
        }


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
                try
                {
                    NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
                    foreach (NetworkInterface ni in interfaces)
                    {
                        if (ni.Supports(NetworkInterfaceComponent.IPv6))
                        {
                            IEnumerator<IPAddress> enumerator = ni.GetIPProperties().DnsAddresses.GetEnumerator();
                            while (enumerator.MoveNext())
                            {
                                IPAddress address = enumerator.Current;
                                if (address.AddressFamily == AddressFamily.InterNetwork)
                                {
                                    continue;
                                }
                                if (ipAddress.IsIPv6LinkLocal && address.IsIPv6LinkLocal ||
                                    ipAddress.IsIPv6SiteLocal && address.IsIPv6SiteLocal)
                                {
                                    var newAddress = new IPAddress(ipAddress.GetAddressBytes(), address.ScopeId);
                                    possibleAddresses.Insert(0, newAddress);
                                }
                            }
                        }
                    }
                }
                catch (IOException)
                {
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


        public static AddressMatcher GetAddressMatcher(string address)
        {
            AddressMatcher matcher;
            int indexColon = address.IndexOf(':');
            int lastIndexColon = address.LastIndexOf(':');
            int indexDot = address.IndexOf('.');
            int lastIndexDot = address.LastIndexOf('.');
            if (indexColon > -1 && lastIndexColon > indexColon)
            {
                if (indexDot == -1)
                {
                    matcher = new Ip6AddressMatcher();
                    ParseIpv6(matcher, address);
                }
                else
                {
                    // IPv4 mapped IPv6
                    if (indexDot >= lastIndexDot)
                    {
                        throw new InvalidAddressException(address);
                    }
                    int lastIndexColon2 = address.LastIndexOf(':');
                    string host2 = address.Substring(lastIndexColon2 + 1);
                    matcher = new Ip4AddressMatcher();
                    ParseIpv4(matcher, host2);
                }
            }
            else
            {
                if (indexDot > -1 && lastIndexDot > indexDot && indexColon == -1)
                {
                    // IPv4
                    matcher = new Ip4AddressMatcher();
                    ParseIpv4(matcher, address);
                }
                else
                {
                    throw new InvalidAddressException(address);
                }
            }
            return matcher;
        }

        private static void ParseIpv4(AddressMatcher matcher, string address)
        {
            string[] parts = address.Split('.');
            if (parts.Length != 4)
            {
                throw new InvalidAddressException(address);
            }
            foreach (string part in parts)
            {
                if (!IsValidIpAddressPart(part, false))
                {
                    throw new InvalidAddressException(address);
                }
            }
            matcher.SetAddress(parts);
        }

        private static bool IsValidIpAddressPart(string part, bool ipv6)
        {
            if (part.Length == 1 && "*".Equals(part))
            {
                return true;
            }
            int rangeIndex = part.IndexOf('-');
            if (rangeIndex > -1 && (rangeIndex != part.LastIndexOf('-') || rangeIndex == part.Length - 1))
            {
                return false;
            }
            string[] subParts;
            if (rangeIndex > -1)
            {
                subParts = part.Split('-');
            }
            else
            {
                subParts = new[] {part};
            }
            foreach (string subPart in subParts)
            {
                try
                {
                    int num;
                    if (ipv6)
                    {
                        num = Convert.ToInt32(subPart, 16);
                        if (num > 65535)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        num = Convert.ToInt32(subPart);
                        if (num > 255)
                        {
                            return false;
                        }
                    }
                }
                catch (FormatException)
                {
                    return false;
                }
            }
            return true;
        }

        private static void ParseIpv6(AddressMatcher matcher, string address)
        {
            if (address.IndexOf('%') > -1)
            {
                string[] parts = address.Split('%');
                address = parts[0];
            }

            string[] parts1 = Regex.Split(address, "((?<=:)|(?=:))"); //from java code:address.Split("((?<=:)|(?=:))");
            var ipString = new List<string>();
            int count = 0;
            int mark = -1;
            for (int i = 0; i < parts1.Length; i++)
            {
                string part = parts1[i];
                string nextPart = i < parts1.Length - 1 ? parts1[i + 1] : null;
                if (string.IsNullOrEmpty(part))
                {
                    continue;
                }
                if (":".Equals(part) && ":".Equals(nextPart))
                {
                    if (mark != -1)
                    {
                        throw new InvalidAddressException(address);
                    }
                    mark = count;
                }
                else
                {
                    if (!":".Equals(part))
                    {
                        count++;
                        ipString.Add(part);
                    }
                }
            }
            if (mark > -1)
            {
                int remaining = (8 - count);
                for (int i_1 = 0; i_1 < remaining; i_1++)
                {
                    ipString.Insert((i_1 + mark), "0");
                }
            }
            if (ipString.Count != 8)
            {
                throw new InvalidAddressException(address);
            }
            string[] addressParts = ipString.ToArray();
            foreach (string part_1 in addressParts)
            {
                if (!IsValidIpAddressPart(part_1, true))
                {
                    throw new InvalidAddressException(address);
                }
            }
            matcher.SetAddress(addressParts);
        }

        /// <summary>http://docs.oracle.com/javase/1.5.0/docs/guide/net/ipv6_guide/index.html</summary>
        internal abstract class AddressMatcher
        {
            protected internal readonly string[] address;

            protected internal AddressMatcher(string[] address)
            {
                this.address = address;
            }

            public abstract bool IsIPv4();

            public abstract bool IsIPv6();

            public abstract void SetAddress(string[] ip);

            protected internal bool Match(string[] mask, string[] input, int radix)
            {
                if (input != null && mask != null)
                {
                    for (int i = 0; i < mask.Length; i++)
                    {
                        if (!DoMatch(mask[i], input[i], radix))
                        {
                            return false;
                        }
                    }
                    return true;
                }
                return false;
            }

            protected internal bool DoMatch(string mask, string input, int radix)
            {
                int dashIndex = mask.IndexOf('-');
                int ipa = Convert.ToInt32(input, radix);
                if ("*".Equals(mask))
                {
                    return true;
                }
                if (dashIndex != -1)
                {
                    int start = Convert.ToInt32(mask.Substring(0, dashIndex).Trim(), radix);
                    int end = Convert.ToInt32(mask.Substring(dashIndex + 1).Trim(), radix);
                    if (ipa >= start && ipa <= end)
                    {
                        return true;
                    }
                }
                else
                {
                    int x = Convert.ToInt32(mask, radix);
                    if (x == ipa)
                    {
                        return true;
                    }
                }
                return false;
            }

            public abstract string GetAddress();

            public abstract bool Match(AddressMatcher matcher);

            public virtual bool Match(string address)
            {
                try
                {
                    return Match(GetAddressMatcher(address));
                }
                catch (Exception)
                {
                    return false;
                }
            }

            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.Append(GetType().Name);
                sb.Append('{');
                sb.Append(GetAddress());
                sb.Append('}');
                return sb.ToString();
            }
        }

        [Serializable]
        internal class InvalidAddressException : ArgumentException
        {
            public InvalidAddressException(string s) : base("Illegal IP address format: " + s)
            {
            }
        }

        internal class Ip4AddressMatcher : AddressMatcher
        {
            public Ip4AddressMatcher() : base(new string[4])
            {
            }

            // d.d.d.d
            public override bool IsIPv4()
            {
                return true;
            }

            public override bool IsIPv6()
            {
                return false;
            }

            public override void SetAddress(string[] ip)
            {
                for (int i = 0; i < ip.Length; i++)
                {
                    address[i] = ip[i];
                }
            }

            public override bool Match(AddressMatcher matcher)
            {
                if (matcher.IsIPv6())
                {
                    return false;
                }
                string[] mask = address;
                string[] input = matcher.address;
                return Match(mask, input, 10);
            }

            public override string GetAddress()
            {
                var sb = new StringBuilder();
                for (int i = 0; i < address.Length; i++)
                {
                    sb.Append(address[i]);
                    if (i != address.Length - 1)
                    {
                        sb.Append('.');
                    }
                }
                return sb.ToString();
            }
        }

        internal class Ip6AddressMatcher : AddressMatcher
        {
            public Ip6AddressMatcher() : base(new string[8])
            {
            }

            // x:x:x:x:x:x:x:x%s
            public override bool IsIPv4()
            {
                return false;
            }

            public override bool IsIPv6()
            {
                return true;
            }

            public override void SetAddress(string[] ip)
            {
                for (int i = 0; i < ip.Length; i++)
                {
                    address[i] = ip[i];
                }
            }

            public override bool Match(AddressMatcher matcher)
            {
                if (matcher.IsIPv4())
                {
                    return false;
                }
                var a = (Ip6AddressMatcher) matcher;
                string[] mask = address;
                string[] input = a.address;
                return Match(mask, input, 16);
            }

            public override string GetAddress()
            {
                var sb = new StringBuilder();
                for (int i = 0; i < address.Length; i++)
                {
                    sb.Append(address[i]);
                    if (i != address.Length - 1)
                    {
                        sb.Append(':');
                    }
                }
                return sb.ToString();
            }
        }
    }
}