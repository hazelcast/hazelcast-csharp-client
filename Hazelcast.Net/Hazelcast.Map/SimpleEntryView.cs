using System;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Map
{
    [Serializable]
    public class SimpleEntryView<K, V> : IdentifiedDataSerializable, IEntryView<K, V>, IIdentifiedDataSerializable
    {
        private long cost;

        private long creationTime;

        private long expirationTime;

        private long hits;
        private K key;

        private long lastAccessTime;

        private long lastStoredTime;

        private long lastUpdateTime;
        private V value;

        private long version;

        //public SimpleEntryView(K key, V value, RecordStatistics statistics, long recordVersion)
        //{
        //    //import com.hazelcast.map.record.RecordStatistics;
        //    this.key = key;
        //    this.value = value;
        //    cost = statistics == null ? -1 : statistics.GetCost();
        //    creationTime = statistics == null ? -1 : statistics.GetCreationTime();
        //    expirationTime = statistics == null ? -1 : statistics.GetExpirationTime();
        //    hits = statistics == null ? -1 : statistics.GetHits();
        //    lastAccessTime = statistics == null ? -1 : statistics.GetLastAccessTime();
        //    lastStoredTime = statistics == null ? -1 : statistics.GetLastStoredTime();
        //    lastUpdateTime = statistics == null ? -1 : statistics.GetLastUpdateTime();
        //    version = recordVersion;
        //}

        public virtual K GetKey()
        {
            return key;
        }

        public virtual V GetValue()
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

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteData(IObjectDataOutput output)
        {
            output.WriteObject(key);
            output.WriteObject(value);
            output.WriteLong(cost);
            output.WriteLong(creationTime);
            output.WriteLong(expirationTime);
            output.WriteLong(hits);
            output.WriteLong(lastAccessTime);
            output.WriteLong(lastStoredTime);
            output.WriteLong(lastUpdateTime);
            output.WriteLong(version);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void ReadData(IObjectDataInput input)
        {
            key = input.ReadObject<K>();
            value = input.ReadObject<V>();
            cost = input.ReadLong();
            creationTime = input.ReadLong();
            expirationTime = input.ReadLong();
            hits = input.ReadLong();
            lastAccessTime = input.ReadLong();
            lastStoredTime = input.ReadLong();
            lastUpdateTime = input.ReadLong();
            version = input.ReadLong();
        }

        public virtual int GetFactoryId()
        {
            return MapDataSerializerHook.FId;
        }

        public virtual int GetId()
        {
            return MapDataSerializerHook.EntryView;
        }

        public virtual void SetKey(K key)
        {
            this.key = key;
        }

        public virtual void SetValue(V value)
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
    }
}