using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Util
{
    /// <summary>
    /// 
    /// </summary>
    internal interface IQueryResultEntry
    {
        IData GetKeyData();

        IData GetValueData();

        IData GetIndexKey();
    }


    [System.Serializable]
    internal class QueryResultEntry : IdentifiedDataSerializable,IIdentifiedDataSerializable, IQueryResultEntry
    {
        private IData indexKey;

        private IData keyData;

        private IData valueData;

        public QueryResultEntry()
        {
        }

        public QueryResultEntry(IData keyData, IData indexKey, IData valueData)
        {
            this.keyData = keyData;
            this.indexKey = indexKey;
            this.valueData = valueData;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteData(IObjectDataOutput output)
        {
            output.WriteData(indexKey);
            output.WriteData(keyData);
            output.WriteData(valueData);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void ReadData(IObjectDataInput input)
        {
            indexKey = input.ReadData();
            keyData = input.ReadData();
            valueData = input.ReadData();
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

        public virtual IData GetKeyData()
        {
            return keyData;
        }

        public virtual IData GetValueData()
        {
            return valueData;
        }

        public virtual IData GetIndexKey()
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