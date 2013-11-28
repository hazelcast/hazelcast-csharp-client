using System;
using System.Net;
using System.Text;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Logging;
using Hazelcast.Serialization.Hook;
using Hazelcast.Util;

namespace Hazelcast.Core
{
    [Serializable]
    public sealed class Member : IMember, IIdentifiedDataSerializable
    {
        private readonly bool localMember;

        private Address address;
        
        //[NonSerialized] 
        //private volatile long lastPing = 0;

        //[NonSerialized] 
        //private volatile long lastRead;

        //[NonSerialized] 
        //private volatile long lastWrite = 0;

        [NonSerialized] private volatile ILogger logger;
        private string uuid;

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

        //FIXME REFACTOR
        //    public void setHazelcastInstance(IHazelcastInstance hazelcastInstance) {
        ////        if (hazelcastInstance instanceof HazelcastInstanceImpl) {
        ////            HazelcastInstanceImpl instance = (HazelcastInstanceImpl) hazelcastInstance;
        ////            localMember = instance.node.address.equals(address);
        ////            logger = instance.node.getLogger(this.getClass().getName());
        ////        }
        //    }
        /// <exception cref="System.IO.IOException"></exception>
        public void ReadData(IObjectDataInput input)
        {
            address = new Address();
            address.ReadData(input);
            uuid = input.ReadUTF();
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteData(IObjectDataOutput output)
        {
            address.WriteData(output);
            output.WriteUTF(uuid);
        }

        public int GetPort()
        {
            return address.GetPort();
        }

        public IPAddress GetInetAddress()
        {
            try
            {
                return address.GetInetAddress();
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

        public IPEndPoint GetInetSocketAddress()
        {
            return GetSocketAddress();
        }

        internal void SetUuid(string uuid)
        {
            this.uuid = uuid;
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
    }
}