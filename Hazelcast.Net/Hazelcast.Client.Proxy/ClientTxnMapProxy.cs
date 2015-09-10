using System.Collections.Generic;
using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Core;

namespace Hazelcast.Client.Proxy
{
    internal class ClientTxnMapProxy<K, V> : ClientTxnProxy, ITransactionalMap<K, V>
    {
        public ClientTxnMapProxy(string name, TransactionContextProxy proxy) : base(name, proxy)
        {
        }

        public virtual bool ContainsKey(object key)
        {
            var keyData = ToData(key);
            var request = TransactionalMapContainsKeyCodec.EncodeRequest(GetName(), GetTransactionId(), GetThreadId(),
                keyData);

            return Invoke(request, m => TransactionalMapContainsKeyCodec.DecodeResponse(m).response);
        }

        public virtual V Get(object key)
        {
            var keyData = ToData(key);
            var request = TransactionalMapGetCodec.EncodeRequest(GetName(), GetTransactionId(), GetThreadId(), keyData);
            var result = Invoke(request, m => TransactionalMapGetCodec.DecodeResponse(m).response);
            return ToObject<V>(result);
        }

        public virtual V GetForUpdate(object key)
        {
            var keyData = ToData(key);
            var request = TransactionalMapGetForUpdateCodec.EncodeRequest(GetName(), GetTransactionId(), GetThreadId(),
                keyData);
            var result = Invoke(request, m => TransactionalMapGetForUpdateCodec.DecodeResponse(m).response);
            return ToObject<V>(result);
        }

        public virtual int Size()
        {
            var request = TransactionalMapSizeCodec.EncodeRequest(GetName(), GetTransactionId(), GetThreadId());
            return Invoke(request, m => TransactionalMapSizeCodec.DecodeResponse(m).response);
        }

        public virtual bool IsEmpty()
        {
            var request = TransactionalMapIsEmptyCodec.EncodeRequest(GetName(), GetTransactionId(), GetThreadId());
            return Invoke(request, m => TransactionalMapIsEmptyCodec.DecodeResponse(m).response);
        }

        public virtual V Put(K key, V value)
        {
            return Put(key, value, -1, TimeUnit.MILLISECONDS);
        }

        public V Put(K key, V value, long ttl, TimeUnit timeunit)
        {
            var keyData = ToData(key);
            var valueData = ToData(value);

            var request = TransactionalMapPutCodec.EncodeRequest(GetName(), GetTransactionId(), GetThreadId(),
                keyData, valueData, timeunit.ToMillis(ttl));
            var result = Invoke(request, m => TransactionalMapPutCodec.DecodeResponse(m).response);
            return ToObject<V>(result);
        }

        public virtual void Set(K key, V value)
        {
            var keyData = ToData(key);
            var valueData = ToData(value);

            var request = TransactionalMapSetCodec.EncodeRequest(GetName(), GetTransactionId(), GetThreadId(),
                keyData, valueData);
            Invoke(request);
        }

        public virtual V PutIfAbsent(K key, V value)
        {
            var keyData = ToData(key);
            var valueData = ToData(value);

            var request = TransactionalMapPutIfAbsentCodec.EncodeRequest(GetName(), GetTransactionId(), GetThreadId(),
                keyData, valueData);
            var result = Invoke(request, m => TransactionalMapPutIfAbsentCodec.DecodeResponse(m).response);
            return ToObject<V>(result);
        }

        public virtual V Replace(K key, V value)
        {
            var keyData = ToData(key);
            var valueData = ToData(value);

            var request = TransactionalMapReplaceCodec.EncodeRequest(GetName(), GetTransactionId(), GetThreadId(),
                keyData, valueData);
            var result = Invoke(request, m => TransactionalMapReplaceCodec.DecodeResponse(m).response);
            return ToObject<V>(result);
        }

        public virtual bool Replace(K key, V oldValue, V newValue)
        {
            var keyData = ToData(key);
            var oldValueData = ToData(oldValue);
            var newValueData = ToData(newValue);

            var request = TransactionalMapReplaceIfSameCodec.EncodeRequest(GetName(), GetTransactionId(), GetThreadId(),
                keyData, oldValueData, newValueData);
            return Invoke(request, m => TransactionalMapReplaceIfSameCodec.DecodeResponse(m).response);
        }

        public virtual V Remove(object key)
        {
            var keyData = ToData(key);
            var request = TransactionalMapRemoveCodec.EncodeRequest(GetName(), GetTransactionId(), GetThreadId(), keyData);
            var result = Invoke(request, m => TransactionalMapRemoveCodec.DecodeResponse(m).response);
            return ToObject<V>(result);
        }

        public virtual void Delete(object key)
        {
            var keyData = ToData(key);
            var request = TransactionalMapDeleteCodec.EncodeRequest(GetName(), GetTransactionId(), GetThreadId(), keyData);
            Invoke(request);
        }

        public virtual bool Remove(object key, object value)
        {
            var keyData = ToData(key);
            var valueData = ToData(value);
            var request = TransactionalMapRemoveIfSameCodec.EncodeRequest(GetName(), GetTransactionId(),
                GetThreadId(), keyData, valueData);
            return Invoke(request, m => TransactionalMapRemoveIfSameCodec.DecodeResponse(m).response);
        }

        public virtual ICollection<K> KeySet()
        {
            var request = TransactionalMapKeySetCodec.EncodeRequest(GetName(), GetTransactionId(), GetThreadId());
            var dataKeySet = Invoke(request, m => TransactionalMapKeySetCodec.DecodeResponse(m).set);
            return ToSet<K>(dataKeySet);
        }

        public ICollection<K> KeySet(IPredicate<K, V> predicate)
        {
            var data = ToData(predicate);
            var request = TransactionalMapKeySetWithPredicateCodec.EncodeRequest(GetName(), GetTransactionId(), GetThreadId(), data);
            var dataKeySet = Invoke(request, m => TransactionalMapKeySetWithPredicateCodec.DecodeResponse(m).set);
            return ToSet<K>(dataKeySet);
        }

        public virtual ICollection<V> Values()
        {
            var request = TransactionalMapValuesCodec.EncodeRequest(GetName(), GetTransactionId(), GetThreadId());
            var dataValues = Invoke(request, m => TransactionalMapValuesCodec.DecodeResponse(m).list);
            return ToList<V>(dataValues);
        }

        public ICollection<V> Values(IPredicate<K, V> predicate)
        {
            var data = ToData(predicate);
            var request = TransactionalMapValuesWithPredicateCodec.EncodeRequest(GetName(), GetTransactionId(), GetThreadId(), data);
            var dataValues = Invoke(request, m => TransactionalMapValuesWithPredicateCodec.DecodeResponse(m).list);
            return ToList<V>(dataValues);
        }

        public override string GetServiceName()
        {
            return ServiceNames.Map;
        }

        internal override void OnDestroy()
        {
        }
    }
}