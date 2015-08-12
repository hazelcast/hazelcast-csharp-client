using System.Collections.Generic;
using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Core;

namespace Hazelcast.Client.Proxy
{
    internal class ClientTxnMultiMapProxy<K, V> : ClientTxnProxy, ITransactionalMultiMap<K, V>
    {
        public ClientTxnMultiMapProxy(string name, TransactionContextProxy proxy) : base(name, proxy)
        {
        }

        /// <exception cref="Hazelcast.Transaction.TransactionException"></exception>
        public virtual bool Put(K key, V value)
        {
            var keyData = ToData(key);
            var valueData = ToData(value);
            var request = TransactionalMultiMapPutCodec.EncodeRequest(GetName(), GetTransactionId(), GetThreadId(),
                keyData, valueData);

            return Invoke(request, m => TransactionalMultiMapPutCodec.DecodeResponse(m).response);
        }

        public virtual ICollection<V> Get(K key)
        {
            var keyData = ToData(key);
            var request = TransactionalMultiMapGetCodec.EncodeRequest(GetName(), GetTransactionId(), GetThreadId(), keyData);
            var list = Invoke(request, m => TransactionalMultiMapGetCodec.DecodeResponse(m).list);
            return ToList<V>(list);
        }

        public virtual bool Remove(object key, object value)
        {
            var keyData = ToData(key);
            var valueData = ToData(value);
            var request = TransactionalMultiMapRemoveEntryCodec.EncodeRequest(GetName(), GetTransactionId(), GetThreadId(),
                keyData, valueData);

            return Invoke(request, m => TransactionalMultiMapRemoveEntryCodec.DecodeResponse(m).response);
        }

        public virtual ICollection<V> Remove(object key)
        {
            var keyData = ToData(key);
            var request = TransactionalMultiMapRemoveCodec.EncodeRequest(GetName(), GetTransactionId(), GetThreadId(),
                keyData);

            var result = Invoke(request, m => TransactionalMultiMapRemoveCodec.DecodeResponse(m).list);
            return ToList<V>(result);
        }

        public virtual int ValueCount(K key)
        {
            var keyData = ToData(key);
            var request = TransactionalMultiMapValueCountCodec.EncodeRequest(GetName(), GetTransactionId(), GetThreadId(),
                keyData);

            return Invoke(request, m => TransactionalMultiMapValueCountCodec.DecodeResponse(m).response);
        }

        public virtual int Size()
        {
            var request = TransactionalMultiMapSizeCodec.EncodeRequest(GetName(), GetTransactionId(), GetThreadId());
            return Invoke(request, m => TransactionalMultiMapSizeCodec.DecodeResponse(m).response);
        }

        public override string GetServiceName()
        {
            return ServiceNames.MultiMap;
        }

        internal override void OnDestroy()
        {
        }
    }
}