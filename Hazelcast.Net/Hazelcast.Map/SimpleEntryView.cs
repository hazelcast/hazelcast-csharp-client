using System;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Map
{

    internal class SimpleEntryView<K, V> : SimpleEntryView,IEntryView<K, V>
    {
        public K GetKey()
        {
            return (K)base.GetKey();
        }

        public V GetValue()
        {
            return (V)base.GetValue();
        }
    }
    [Serializable]
    internal class SimpleEntryView : IdentifiedDataSerializable, IIdentifiedDataSerializable
    {
        private object key;
        private object value;

        private long cost;
        private long creationTime;
        private long expirationTime;
        private long hits;
        private long lastAccessTime;
        private long lastStoredTime;
        private long lastUpdateTime;
        private long version;
        private long evictionCriteriaNumber;
        private long ttl;

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteData(IObjectDataOutput output)
        {
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void ReadData(IObjectDataInput input)
        {
            key = input.ReadObject<object>();
            value = input.ReadObject<object>();
            cost = input.ReadLong();
            creationTime = input.ReadLong();
            expirationTime = input.ReadLong();
            hits = input.ReadLong();
            lastAccessTime = input.ReadLong();
            lastStoredTime = input.ReadLong();
            lastUpdateTime = input.ReadLong();
            version = input.ReadLong();
            evictionCriteriaNumber = input.ReadLong();
            ttl = input.ReadLong();
        }

        public virtual object GetKey()
        {
            return key;
        }

        public virtual object GetValue()
        {
            return value;
        }

        public virtual long GetCost()
        {
            return cost;
        }

        public virtual long GetCreationTime()
        {
            return creationTime;
        }

        public virtual long GetExpirationTime()
        {
            return expirationTime;
        }

        public virtual long GetHits()
        {
            return hits;
        }

        public virtual long GetLastAccessTime()
        {
            return lastAccessTime;
        }

        public virtual long GetLastStoredTime()
        {
            return lastStoredTime;
        }

        public virtual long GetLastUpdateTime()
        {
            return lastUpdateTime;
        }

        public virtual long GetVersion()
        {
            return version;
        }

        public virtual int GetFactoryId()
        {
            return MapDataSerializerHook.FId;
        }

        public virtual int GetId()
        {
            return MapDataSerializerHook.EntryView;
        }

        public virtual void SetKey(object key)
        {
            this.key = key;
        }

        public virtual void SetValue(object value)
        {
            this.value = value;
        }

        public virtual void SetCost(long cost)
        {
            this.cost = cost;
        }

        public virtual void SetCreationTime(long creationTime)
        {
            this.creationTime = creationTime;
        }

        public virtual void SetExpirationTime(long expirationTime)
        {
            this.expirationTime = expirationTime;
        }

        public virtual void SetHits(long hits)
        {
            this.hits = hits;
        }

        public virtual void SetLastAccessTime(long lastAccessTime)
        {
            this.lastAccessTime = lastAccessTime;
        }

        public virtual void SetLastStoredTime(long lastStoredTime)
        {
            this.lastStoredTime = lastStoredTime;
        }

        public virtual void SetLastUpdateTime(long lastUpdateTime)
        {
            this.lastUpdateTime = lastUpdateTime;
        }

        public virtual void SetVersion(long version)
        {
            this.version = version;
        }

        public long GetEvictionCriteriaNumber()
        {
            return evictionCriteriaNumber;
        }

        public void SetEvictionCriteriaNumber(long evictionCriteriaNumber)
        {
            this.evictionCriteriaNumber = evictionCriteriaNumber;
        }

        public long GetTtl()
        {
            return ttl;
        }

        public void SetTtl(long ttl)
        {
            this.ttl = ttl;
        }

    }
}