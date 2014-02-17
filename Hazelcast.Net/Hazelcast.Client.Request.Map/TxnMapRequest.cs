using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Map
{
    internal class TxnMapRequest<K, V> : AbstractTxnMapRequest
    {
        internal IPredicate<K, V> predicate;

        public TxnMapRequest()
        {
        }

        public TxnMapRequest(string name, TxnMapRequestType requestType) : base(name, requestType)
        {
        }

        public TxnMapRequest(string name, TxnMapRequestType requestType, Data key) : this(name, requestType)
        {
            this.key = key;
        }

        public TxnMapRequest(string name, TxnMapRequestType requestType, Data key, Data value)
            : this(name, requestType, key)
        {
            this.value = value;
        }

        public TxnMapRequest(string name, TxnMapRequestType requestType, Data key, Data value, Data newValue)
            : this(name, requestType, key, value)
        {
            this.newValue = newValue;
        }

        public TxnMapRequest(string name, TxnMapRequestType requestType, IPredicate<K, V> predicate)
            : this(name, requestType, null, null, null)
        {
            this.predicate = predicate;
        }

        public TxnMapRequest(string name, TxnMapRequestType requestType, Data key, Data value, long ttl, TimeUnit timeUnit)
            : base(name,requestType,key,value,ttl,timeUnit)
        {
        }

        public override int GetClassId()
        {
            return MapPortableHook.TxnRequest;
        }

        /// <exception cref="System.IO.IOException"></exception>
        protected internal override void WriteDataInner(IObjectDataOutput writer)
        {
            writer.WriteObject(predicate);
        }

    }
}