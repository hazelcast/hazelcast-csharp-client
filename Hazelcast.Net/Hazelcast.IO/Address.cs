using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Hazelcast.IO.Serialization;
using Hazelcast.Util;

namespace Hazelcast.IO
{
    [Serializable]
    public sealed class Address : IdentifiedDataSerializable,IIdentifiedDataSerializable
    {
        public const int Id = 1;

        private const byte IPv4 = 4;

        private const byte IPv6 = 6;
        [NonSerialized] private readonly bool hostSet;

        private string host;
        private int port = -1;

        [NonSerialized] private string scopeId;
        private byte type;

        public Address()
        {
        }

        /// <exception cref="Hazelcast.Net.Ext.UnknownHostException"></exception>
        public Address(string host, int port) : this(host, GetAddressByName(host), port)
        {
            hostSet = !AddressUtil.IsIpAddress(host);
        }

        public Address(IPAddress inetAddress, int port) : this(null, inetAddress, port)
        {
            hostSet = false;
        }

        public Address(IPEndPoint inetSocketAddress) : this(inetSocketAddress.Address, inetSocketAddress.Port)
        {
        }

        private Address(string hostname, IPAddress inetAddress, int port)
        {
            type = (inetAddress.AddressFamily == AddressFamily.InterNetwork) ? IPv4 : IPv6;
            string[] addressArgs = inetAddress.ToString().Split('%');
            host = hostname ?? addressArgs[0];
            if (addressArgs.Length == 2)
            {
                scopeId = addressArgs[1];
            }
            this.port = port;
        }

        public Address(Address address)
        {
            host = address.host;
            port = address.port;
            type = address.type;
            scopeId = address.scopeId;
            hostSet = address.hostSet;
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

        public int GetFactoryId()
        {
            return Data.FactoryId;
        }

        public int GetId()
        {
            return Id;
        }

        public string GetHost()
        {
            return host;
        }

        public override string ToString()
        {
            return "Address[" + GetHost() + "]:" + port;
        }

        public int GetPort()
        {
            return port;
        }

        public IPAddress GetInetAddress()
        {
            return GetAddressByName(GetScopedHost());
        }

        public IPEndPoint GetInetSocketAddress()
        {
            return new IPEndPoint(GetInetAddress(), port);
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
            return Hash(Encoding.UTF8.GetBytes(host))*29 + port;
        }

        private int Hash(byte[] bytes)
        {
            int hash = 0;
            foreach (byte b in bytes)
            {
                hash = (hash*29) + b;
            }
            return hash;
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

        public static IPAddress GetAddressByName(string name)
        {
            if (name == "0.0.0.0")
                return IPAddress.Any;
            return Dns.GetHostAddresses(name).FirstOrDefault();
        }
    }
}