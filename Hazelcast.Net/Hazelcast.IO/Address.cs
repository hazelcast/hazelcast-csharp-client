using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;
using Hazelcast.Util;

namespace Hazelcast.IO
{
    /// <summary>Represents an address of a member in the cluster.</summary>
    /// <remarks>Represents an address of a member in the cluster.</remarks>
    public sealed class Address : IdentifiedDataSerializable, IIdentifiedDataSerializable
    {
        public const int Id = 1;

        private const byte IPv4 = 4;
        private const byte IPv6 = 6;
        private readonly bool hostSet;

        private string host;
        private int port = -1;

        private string scopeId;
        private byte type;

        public Address()
        {
        }

        public Address(string host, int port):this(GetAddressByName(host),port)
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
            string[] addressArgs = inetAddress.ToString().Split('%');
            host = hostname ?? addressArgs[0];
            if (addressArgs.Length == 2)
            {
                scopeId = addressArgs[1];
            }
            this.port = port;
            hostSet = !AddressUtil.IsIpAddress(host);
        }

        public int GetFactoryId()
        {
            return ClusterDataSerializerHook.FId;
        }

        public int GetId()
        {
            return Id;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteData(IObjectDataOutput output)
        {
            output.WriteInt(port);
            output.Write(type);
            if (host != null)
            {
                byte[] address = Encoding.UTF8.GetBytes(host);
                output.WriteInt(address.Length);
                output.Write(address);
            }
            else
            {
                output.WriteInt(0);
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void ReadData(IObjectDataInput input)
        {
            port = input.ReadInt();
            type = input.ReadByte();
            int len = input.ReadInt();
            if (len > 0)
            {
                var address = new byte[len];
                input.ReadFully(address);
                host = Encoding.UTF8.GetString(address);
            }
        }

        public string GetHost()
        {
            return host;
        }

        public int GetPort()
        {
            return port;
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

        public bool IsIPv4()
        {
            return type == IPv4;
        }

        public bool IsIPv6()
        {
            return type == IPv6;
        }

        public string GetScopeId()
        {
            return IsIPv6() ? scopeId : null;
        }

        public void SetScopeId(string scopeId)
        {
            if (IsIPv6())
            {
                this.scopeId = scopeId;
            }
        }

        public string GetScopedHost()
        {
            return (IsIPv4() || hostSet || scopeId == null) ? GetHost() : GetHost() + "%" + scopeId;
        }

        public override bool Equals(object o)
        {
            if (this == o)
            {
                return true;
            }
            if (!(o is Address))
            {
                return false;
            }
            var address = (Address) o;
            return port == address.port && type == address.type && host.Equals(address.host);
        }

        public override int GetHashCode()
        {
            int result = port;
            result = 31*result + host.GetHashCode();
            return result;
        }

        public override string ToString()
        {
            return "Address[" + GetHost() + "]:" + port;
        }

        private static IPAddress Resolve(IPEndPoint inetSocketAddress)
        {
            if (inetSocketAddress == null)
            {
                throw new ArgumentNullException("inetSocketAddress");
            }
            IPAddress address = inetSocketAddress.Address;
            if (address == null)
            {
                throw new ArgumentException("Can't resolve address: " + inetSocketAddress);
            }
            return address;
        }

        public static IPAddress GetAddressByName(string name)
        {
            return name == "0.0.0.0" ? IPAddress.Any : Dns.GetHostAddresses(name).FirstOrDefault();
        }
    }
}