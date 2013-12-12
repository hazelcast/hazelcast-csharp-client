using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Util
{
    /// <summary>
    /// 
    /// </summary>
    public interface IQueryResultEntry
    {
        Data GetKeyData();

        Data GetValueData();

        Data GetIndexKey();
    }


    [System.Serializable]
    public class QueryResultEntry : IIdentifiedDataSerializable, IQueryResultEntry
    {
        private Data indexKey;

        private Data keyData;

        private Data valueData;

        public QueryResultEntry()
        {
        }

        public QueryResultEntry(Data keyData, Data indexKey, Data valueData)
        {
            this.keyData = keyData;
            this.indexKey = indexKey;
            this.valueData = valueData;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteData(IObjectDataOutput output)
        {
            IOUtil.WriteNullableData(output, GetIndexKey());
            IOUtil.WriteNullableData(output, GetKeyData());
            IOUtil.WriteNullableData(output, GetValueData());
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void ReadData(IObjectDataInput input)
        {
            indexKey = IOUtil.ReadNullableData(input);
            keyData = IOUtil.ReadNullableData(input);
            valueData = IOUtil.ReadNullableData(input);
        }

        public string GetJavaClassName()
        {
            throw new System.NotImplementedException();
        }

        public override bool Equals(object o)
        {
            if (this == o)
            {
                return true;
            }
            if (o == null || GetType() != o.GetType())
            {
                return false;
            }
            var that = (QueryResultEntry)o;
            if (indexKey != null ? !indexKey.Equals(that.indexKey) : that.indexKey != null)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return indexKey != null ? indexKey.GetHashCode() : 0;
        }

        public virtual Data GetKeyData()
        {
            return keyData;
        }

        public virtual Data GetValueData()
        {
            return valueData;
        }

        public virtual Data GetIndexKey()
        {
            return indexKey;
        }

        public virtual int GetFactoryId()
        {
            return MapDataSerializerHook.FId;
        }

        public virtual int GetId()
        {
            return MapDataSerializerHook.QueryResultEntry;
        }
    }
}