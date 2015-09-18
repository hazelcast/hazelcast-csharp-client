using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Hazelcast.Client.Spi;
using Hazelcast.IO;
using Hazelcast.Logging;

namespace Hazelcast.Core
{
    internal sealed class Member : IMember
    {
        private readonly ConcurrentDictionary<string, string> _attributes = new ConcurrentDictionary<string, string>();
        private readonly Address address;
        private readonly string uuid;
        private volatile ILogger logger;

        public Member()
        {
        }

        public Member(Address address)
            : this(address, null, new Dictionary<string, string>())
        {
        }

        public Member(Address address, string uuid)
            : this(address, uuid, new Dictionary<string, string>())
        {
        }

        public Member(Address address, string uuid, IDictionary<string, string> attributes)
        {
            this.address = address;
            this.uuid = uuid;
            foreach (var kv in attributes)
            {
                _attributes.TryAdd(kv.Key, kv.Value);
            }
        }

        public Address GetAddress()
        {
            return address;
        }

        public IPEndPoint GetSocketAddress()
        {
            try
            {
                return address.GetInetSocketAddress();
            }
            catch (Exception e)
            {
                if (logger != null)
                {
                    logger.Warning(e);
                }
                return null;
            }
        }

        public string GetUuid()
        {
            return uuid;
        }

        public IDictionary<string, string> GetAttributes()
        {
            return _attributes;
        }

        public string GetAttribute(string key)
        {
            string _out;
            _attributes.TryGetValue(key, out _out);
            return _out;
        }

        public override string ToString()
        {
            var sb = new StringBuilder("IMember [");
            sb.Append(address.GetHost());
            sb.Append("]");
            sb.Append(":");
            sb.Append(address.GetPort());
            return sb.ToString();
        }

        public override int GetHashCode()
        {
            var Prime = 31;
            var result = 1;
            result = Prime*result + ((address == null) ? 0 : address.GetHashCode());
            return result;
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (obj == null)
            {
                return false;
            }
            if (GetType() != obj.GetType())
            {
                return false;
            }
            var other = (Member) obj;
            if (address == null)
            {
                if (other.address != null)
                {
                    return false;
                }
            }
            else
            {
                if (!address.Equals(other.address))
                {
                    return false;
                }
            }
            return true;
        }

        internal void UpdateAttribute(MemberAttributeOperationType operationType, string key, string value)
        {
            switch (operationType)
            {
                case MemberAttributeOperationType.PUT:
                    _attributes.TryAdd(key, value);
                    break;
                case MemberAttributeOperationType.REMOVE:
                    string _out;
                    _attributes.TryRemove(key, out _out);
                    break;
            }
        }
    }
}