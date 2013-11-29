using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Map
{
    public abstract class AbstractTxnMapRequest : IPortable
    {
        public enum TxnMapRequestType
        {
            ContainsKey = 1,
            Get = 2,
            Size = 3,
            Put = 4,
            PutIfAbsent = 5,
            Replace = 6,
            ReplaceIfSame = 7,
            Set = 8,
            Remove = 9,
            Delete = 10,
            RemoveIfSame = 11,
            Keyset = 12,
            KeysetByPredicate = 13,
            Values = 14,
            ValuesByPredicate = 15,
            GetForUpdate = 16
        }

        internal Data key;

        internal string name;
        internal Data newValue;

        internal TxnMapRequestType requestType;

        internal Data value;

        protected AbstractTxnMapRequest()
        {
        }

        protected AbstractTxnMapRequest(string name, TxnMapRequestType requestType)
        {
            this.name = name;
            this.requestType = requestType;
        }

        protected AbstractTxnMapRequest(string name, TxnMapRequestType requestType, Data key) : this(name, requestType)
        {
            this.key = key;
        }

        protected AbstractTxnMapRequest(string name, TxnMapRequestType requestType, Data key, Data value)
            : this(name, requestType, key)
        {
            this.value = value;
        }

        protected AbstractTxnMapRequest(string name, TxnMapRequestType requestType, Data key, Data value, Data newValue)
            : this(name, requestType, key, value)
        {
            this.newValue = newValue;
        }

        public virtual int GetFactoryId()
        {
            return MapPortableHook.FId;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            writer.WriteInt("t", (int) requestType);
            IObjectDataOutput output = writer.GetRawDataOutput();
            IOUtil.WriteNullableData(output, key);
            IOUtil.WriteNullableData(output, value);
            IOUtil.WriteNullableData(output, newValue);
            WriteDataInner(output);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void ReadPortable(IPortableReader reader)
        {
            name = reader.ReadUTF("n");
            requestType = (TxnMapRequestType) reader.ReadInt("t");
            IObjectDataInput input = reader.GetRawDataInput();
            key = IOUtil.ReadNullableData(input);
            value = IOUtil.ReadNullableData(input);
            newValue = IOUtil.ReadNullableData(input);
            ReadDataInner(input);
        }

        public abstract int GetClassId();

        /// <exception cref="System.IO.IOException"></exception>
        protected internal abstract void WriteDataInner(IObjectDataOutput writer);

        /// <exception cref="System.IO.IOException"></exception>
        protected internal abstract void ReadDataInner(IObjectDataInput reader);
    }
}