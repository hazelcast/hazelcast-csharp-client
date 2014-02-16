using System;
using Hazelcast.Client.Request.Base;
using Hazelcast.Client.Request.Transaction;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Map
{
    public abstract class AbstractTxnMapRequest : BaseTransactionRequest
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
            PuttWithTTL = 17
        }

        internal string name;
        internal TxnMapRequestType requestType;
        internal Data key;
        internal Data value;
        internal Data newValue;
        internal long ttl = -1;

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

        public AbstractTxnMapRequest(string name, TxnMapRequestType requestType, Data key, Data value, long ttl, TimeUnit timeUnit)
            : this(name, requestType, key, value)
        {
            this.ttl = timeUnit.ToMillis(ttl);
        }

        public override int GetFactoryId()
        {
            return MapPortableHook.FId;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            writer.WriteInt("t", (int) requestType);
            IObjectDataOutput output = writer.GetRawDataOutput();
            IOUtil.WriteNullableData(output, key);
            IOUtil.WriteNullableData(output, value);
            IOUtil.WriteNullableData(output, newValue);
            WriteDataInner(output);
            output.WriteLong(ttl);
        }


        /// <exception cref="System.IO.IOException"></exception>
        protected internal abstract void WriteDataInner(IObjectDataOutput writer);

    }
}