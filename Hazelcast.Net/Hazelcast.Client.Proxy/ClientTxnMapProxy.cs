// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections.Generic;
using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Core;

namespace Hazelcast.Client.Proxy
{
    internal class ClientTxnMapProxy<TKey, TValue> : ClientTxnProxy, ITransactionalMap<TKey, TValue>
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

        public virtual TValue Get(object key)
        {
            var keyData = ToData(key);
            var request = TransactionalMapGetCodec.EncodeRequest(GetName(), GetTransactionId(), GetThreadId(), keyData);
            var result = Invoke(request, m => TransactionalMapGetCodec.DecodeResponse(m).response);
            return ToObject<TValue>(result);
        }

        public virtual TValue GetForUpdate(object key)
        {
            var keyData = ToData(key);
            var request = TransactionalMapGetForUpdateCodec.EncodeRequest(GetName(), GetTransactionId(), GetThreadId(),
                keyData);
            var result = Invoke(request, m => TransactionalMapGetForUpdateCodec.DecodeResponse(m).response);
            return ToObject<TValue>(result);
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

        public virtual TValue Put(TKey key, TValue value)
        {
            return Put(key, value, -1, TimeUnit.Milliseconds);
        }

        public TValue Put(TKey key, TValue value, long ttl, TimeUnit timeunit)
        {
            var keyData = ToData(key);
            var valueData = ToData(value);

            var request = TransactionalMapPutCodec.EncodeRequest(GetName(), GetTransactionId(), GetThreadId(),
                keyData, valueData, timeunit.ToMillis(ttl));
            var result = Invoke(request, m => TransactionalMapPutCodec.DecodeResponse(m).response);
            return ToObject<TValue>(result);
        }

        public virtual void Set(TKey key, TValue value)
        {
            var keyData = ToData(key);
            var valueData = ToData(value);

            var request = TransactionalMapSetCodec.EncodeRequest(GetName(), GetTransactionId(), GetThreadId(),
                keyData, valueData);
            Invoke(request);
        }

        public virtual TValue PutIfAbsent(TKey key, TValue value)
        {
            var keyData = ToData(key);
            var valueData = ToData(value);

            var request = TransactionalMapPutIfAbsentCodec.EncodeRequest(GetName(), GetTransactionId(), GetThreadId(),
                keyData, valueData);
            var result = Invoke(request, m => TransactionalMapPutIfAbsentCodec.DecodeResponse(m).response);
            return ToObject<TValue>(result);
        }

        public virtual TValue Replace(TKey key, TValue value)
        {
            var keyData = ToData(key);
            var valueData = ToData(value);

            var request = TransactionalMapReplaceCodec.EncodeRequest(GetName(), GetTransactionId(), GetThreadId(),
                keyData, valueData);
            var result = Invoke(request, m => TransactionalMapReplaceCodec.DecodeResponse(m).response);
            return ToObject<TValue>(result);
        }

        public virtual bool Replace(TKey key, TValue oldValue, TValue newValue)
        {
            var keyData = ToData(key);
            var oldValueData = ToData(oldValue);
            var newValueData = ToData(newValue);

            var request = TransactionalMapReplaceIfSameCodec.EncodeRequest(GetName(), GetTransactionId(), GetThreadId(),
                keyData, oldValueData, newValueData);
            return Invoke(request, m => TransactionalMapReplaceIfSameCodec.DecodeResponse(m).response);
        }

        public virtual TValue Remove(object key)
        {
            var keyData = ToData(key);
            var request = TransactionalMapRemoveCodec.EncodeRequest(GetName(), GetTransactionId(), GetThreadId(),
                keyData);
            var result = Invoke(request, m => TransactionalMapRemoveCodec.DecodeResponse(m).response);
            return ToObject<TValue>(result);
        }

        public virtual void Delete(object key)
        {
            var keyData = ToData(key);
            var request = TransactionalMapDeleteCodec.EncodeRequest(GetName(), GetTransactionId(), GetThreadId(),
                keyData);
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

        public virtual ICollection<TKey> KeySet()
        {
            var request = TransactionalMapKeySetCodec.EncodeRequest(GetName(), GetTransactionId(), GetThreadId());
            var dataKeySet = Invoke(request, m => TransactionalMapKeySetCodec.DecodeResponse(m).response);
            return ToSet<TKey>(dataKeySet);
        }

        public ICollection<TKey> KeySet(IPredicate predicate)
        {
            var data = ToData(predicate);
            var request = TransactionalMapKeySetWithPredicateCodec.EncodeRequest(GetName(), GetTransactionId(),
                GetThreadId(), data);
            var dataKeySet = Invoke(request, m => TransactionalMapKeySetWithPredicateCodec.DecodeResponse(m).response);
            return ToSet<TKey>(dataKeySet);
        }

        public virtual ICollection<TValue> Values()
        {
            var request = TransactionalMapValuesCodec.EncodeRequest(GetName(), GetTransactionId(), GetThreadId());
            var dataValues = Invoke(request, m => TransactionalMapValuesCodec.DecodeResponse(m).response);
            return ToList<TValue>(dataValues);
        }

        public ICollection<TValue> Values(IPredicate predicate)
        {
            var data = ToData(predicate);
            var request = TransactionalMapValuesWithPredicateCodec.EncodeRequest(GetName(), GetTransactionId(),
                GetThreadId(), data);
            var dataValues = Invoke(request, m => TransactionalMapValuesWithPredicateCodec.DecodeResponse(m).response);
            return ToList<TValue>(dataValues);
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