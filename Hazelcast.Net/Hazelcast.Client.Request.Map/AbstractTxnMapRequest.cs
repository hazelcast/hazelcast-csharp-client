using Hazelcast.Client.Request.Base;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Map
{
    internal abstract class AbstractTxnMapRequest : BaseTransactionRequest
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
            GetForUpdate = 16,
            PutWithTtl = 17
        }

        internal string name;
        internal TxnMapRequestType requestType;
        internal IData key;
        internal IData value;
        internal IData newValue;
        internal long ttl = -1;

        protected AbstractTxnMapRequest(string name, TxnMapRequestType requestType)
        {
            this.name = name;
            this.requestType = requestType;
        }

        protected AbstractTxnMapRequest(string name, TxnMapRequestType requestType, IData key) : this(name, requestType)
        {
            this.key = key;
        }

        protected AbstractTxnMapRequest(string name, TxnMapRequestType requestType, IData key, IData value)
            : this(name, requestType, key)
        {
            this.value = value;
        }

        protected AbstractTxnMapRequest(string name, TxnMapRequestType requestType, IData key, IData value,
            IData newValue) : this(name, requestType, key, value)
        {
            this.newValue = newValue;
        }

        protected AbstractTxnMapRequest(string name, TxnMapRequestType requestType, IData key, IData value, long ttl, TimeUnit timeUnit)
            : this(name, requestType, key, value)
        {
            this.ttl = timeUnit.ToMillis(ttl);
        }

        public override int GetFactoryId()
        {
            return MapPortableHook.FId;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            base.Write(writer);
            writer.WriteUTF("n", name);
            writer.WriteInt("t", (int) requestType);
            IObjectDataOutput output = writer.GetRawDataOutput();
            output.WriteData(key);
            output.WriteData(value);
            output.WriteData(newValue);
            WriteDataInner(output);
            output.WriteLong(ttl);
        }

        /// <exception cref="System.IO.IOException"></exception>
        protected internal abstract void WriteDataInner(IObjectDataOutput writer);

    }
}