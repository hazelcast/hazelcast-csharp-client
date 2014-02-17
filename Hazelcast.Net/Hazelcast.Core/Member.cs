using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Hazelcast.Client.Request.Cluster;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Logging;
using Hazelcast.Serialization.Hook;
using Hazelcast.Util;

namespace Hazelcast.Core
{
    [Serializable]
    internal sealed class Member :IdentifiedDataSerializable, IMember, IIdentifiedDataSerializable
    {
        private readonly bool localMember;

        private Address address;

        private volatile ILogger logger;
        
        private string uuid;

        private readonly ConcurrentDictionary<string,object> _attributes= new ConcurrentDictionary<string, object>(); 
        public Member()
        {
        }

        public Member(Address address, bool localMember) : this(address, localMember, null)
        {
        }

        public Member(Address address, bool localMember, string uuid) : this()
        {
            this.localMember = localMember;
            this.address = address;
            //lastRead = Clock.CurrentTimeMillis();
            this.uuid = uuid;
        }

        public int GetFactoryId()
        {
            return ClusterDataSerializerHook.FId;
        }

        public int GetId()
        {
            return ClusterDataSerializerHook.Member;
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

        public bool LocalMember()
        {
            return localMember;
        }

        public string GetUuid()
        {
            return uuid;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void ReadData(IObjectDataInput input)
        {
            address = new Address();
            address.ReadData(input);
            uuid = input.ReadUTF();
            int size = input.ReadInt();
            for (int i = 0; i < size; i++)
            {
                string key = input.ReadUTF();
                object value = IOUtil.ReadAttributeValue(input);
                _attributes.TryAdd(key, value);
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteData(IObjectDataOutput output)
        {
            address.WriteData(output);
            output.WriteUTF(uuid);
            output.WriteInt(_attributes.Count);
            foreach (var entry in _attributes)
            {
                output.WriteUTF(entry.Key);
                IOUtil.WriteAttributeValue(entry.Value,output);
            }

        }

        public override string ToString()
        {
            var sb = new StringBuilder("IMember [");
            sb.Append(address.GetHost());
            sb.Append("]");
            sb.Append(":");
            sb.Append(address.GetPort());
            if (localMember)
            {
                sb.Append(" this");
            }
            return sb.ToString();
        }

        public override int GetHashCode()
        {
            int Prime = 31;
            int result = 1;
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


        public void UpdateAttribute(MapOperationType operationType, String key, Object value)
        {
            switch (operationType)
            {
                case MapOperationType.PUT:
                    _attributes.TryAdd(key, value);
                    break;
                case MapOperationType.REMOVE:
                    object _out;
                    _attributes.TryRemove(key,out _out);
                    break;
            }
        }
        public IDictionary<string, object> GetAttributes()
        {
            return _attributes;
        }

        public object GetAttribute(string key)
        {
            object _out;
            _attributes.TryGetValue(key,out _out);
            return _out;
        }

        public T GetAttribute<T>(string key)
        {
            object _out;
            _attributes.TryGetValue(key,out _out);
            return (T)_out;
        }

        public void SetAttribute<T>(string key, T value)
        {
            if (!localMember)
            {
                throw new NotSupportedException("Attributes on remote members must not be changed");
            }
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            (_attributes as IDictionary<string,object>).Add(key,value);
        }

        public void RemoveAttribute(string key)
        {
            if (!localMember)
            {
                throw new NotSupportedException("Attributes on remote members must not be changed");
            }
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            object removed;
            _attributes.TryRemove(key, out removed);
        }

    }
}