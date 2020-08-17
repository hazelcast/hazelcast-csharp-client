// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Predicates;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;
using Hazelcast.Serialization.Collections;
using Microsoft.Extensions.Logging;

namespace Hazelcast.DistributedObjects.Impl
{
    internal class HTxDictionary<TKey, TValue> : TransactionalDistributedObjectBase, IHTxDictionary<TKey, TValue>
    {
        public HTxDictionary(string name, DistributedObjectFactory factory, Cluster cluster, ClientConnection transactionClientConnection, Guid transactionId, ISerializationService serializationService, ILoggerFactory loggerFactory)
            : base(HDictionary.ServiceName, name, factory, cluster, transactionClientConnection, transactionId, serializationService, loggerFactory)
        { }

        public async Task<bool> ContainsKeyAsync(TKey key)
        {
            var keyData = ToSafeData(key);
            var requestMessage = TransactionalMapContainsKeyCodec.EncodeRequest(Name, TransactionId, ContextId, keyData);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClientConnection).CAF();
            return TransactionalMapContainsKeyCodec.DecodeResponse(responseMessage).Response;
        }

        public async Task RemoveAsync(TKey key)
        {
            var keyData = ToSafeData(key);
            var requestMessage = TransactionalMapDeleteCodec.EncodeRequest(Name, TransactionId, ContextId, keyData);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClientConnection).CAF();
            _ = TransactionalMapDeleteCodec.DecodeResponse(responseMessage);
        }

        public async Task<TValue> GetAsync(TKey key)
        {
            var keyData = ToSafeData(key);
            var requestMessage = TransactionalMapGetCodec.EncodeRequest(Name, TransactionId, ContextId, keyData);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClientConnection).CAF();
            var response = TransactionalMapGetCodec.DecodeResponse(responseMessage).Response;
            return ToObject<TValue>(response);
        }

        public async Task<TValue> GetForUpdateAsync(TKey key)
        {
            var keyData = ToSafeData(key);
            var requestMessage = TransactionalMapGetForUpdateCodec.EncodeRequest(Name, TransactionId, ContextId, keyData);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClientConnection).CAF();
            var response = TransactionalMapGetForUpdateCodec.DecodeResponse(responseMessage).Response;
            return ToObject<TValue>(response);
        }

        public async Task<bool> IsEmpty()
        {
            var requestMessage = TransactionalMapIsEmptyCodec.EncodeRequest(Name, TransactionId, ContextId);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClientConnection).CAF();
            return TransactionalMapIsEmptyCodec.DecodeResponse(responseMessage).Response;
        }

        public async Task<IReadOnlyList<TKey>> GetKeysAsync()
        {
            var requestMessage = TransactionalMapKeySetCodec.EncodeRequest(Name, TransactionId, ContextId);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClientConnection).CAF();
            var response = TransactionalMapKeySetCodec.DecodeResponse(responseMessage).Response;
            return new ReadOnlyLazyList<TKey>(response, SerializationService);
        }

        public async Task<IReadOnlyList<TKey>> GetKeysAsync(IPredicate predicate)
        {
            var predicateData = ToSafeData(predicate);
            var requestMessage = TransactionalMapKeySetWithPredicateCodec.EncodeRequest(Name, TransactionId, ContextId, predicateData);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClientConnection).CAF();
            var response = TransactionalMapKeySetWithPredicateCodec.DecodeResponse(responseMessage).Response;
            return new ReadOnlyLazyList<TKey>(response, SerializationService);
        }

        public Task<TValue> AddOrUpdateAndReturnAsync(TKey key, TValue value)
            => AddOrUpdateAndReturnTtlAsync(key, value, TimeToLive.InfiniteTimeSpan);

        public async Task<TValue> AddOrUpdateAndReturnTtlAsync(TKey key, TValue value, TimeSpan timeToLive)
        {
            var (keyData, valueData) = ToSafeData(key, value);
            var timeToLiveMilliseconds = timeToLive.CodecMilliseconds(-1);
            var requestMessage = TransactionalMapPutCodec.EncodeRequest(Name, TransactionId, ContextId, keyData, valueData, timeToLiveMilliseconds);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClientConnection).CAF();
            var response = TransactionalMapPutCodec.DecodeResponse(responseMessage).Response;
            return ToObject<TValue>(response);
        }

        public async Task<TValue> AddAsync(TKey key, TValue value)
        {
            var (keyData, valueData) = ToSafeData(key, value);

            var requestMessage = TransactionalMapPutIfAbsentCodec.EncodeRequest(Name, TransactionId, ContextId, keyData, valueData);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClientConnection).CAF();
            var response = TransactionalMapPutIfAbsentCodec.DecodeResponse(responseMessage).Response;
            return ToObject<TValue>(response);
        }

        public async Task<TValue> RemoveAndReturnAsync(TKey key)
        {
            var keyData = ToSafeData(key);
            var requestMessage = TransactionalMapRemoveCodec.EncodeRequest(Name, TransactionId, ContextId, keyData);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClientConnection).CAF();
            var response = TransactionalMapRemoveCodec.DecodeResponse(responseMessage).Response;
            return ToObject<TValue>(response);
        }


        public async Task<bool> RemoveAsync(TKey key, TValue value)
        {
            var (keyData, valueData) = ToSafeData(key, value);
            var requestMessage = TransactionalMapRemoveIfSameCodec.EncodeRequest(Name, TransactionId, ContextId, keyData, valueData);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClientConnection).CAF();
            return TransactionalMapRemoveIfSameCodec.DecodeResponse(responseMessage).Response;
        }

        public async Task<TValue> ReplaceAndReturnAsync(TKey key, TValue value)
        {
            var (keyData, valueData) = ToSafeData(key, value);

            var requestMessage = TransactionalMapReplaceCodec.EncodeRequest(Name, TransactionId, ContextId, keyData, valueData);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClientConnection).CAF();
            var response = TransactionalMapReplaceCodec.DecodeResponse(responseMessage).Response;
            return ToObject<TValue>(response);
        }

        public async Task<bool> ReplaceAsync(TKey key, TValue oldValue, TValue newValue)
        {
            var (keyData, oldValueData, newValueData) = ToSafeData(key, oldValue, newValue);

            var requestMessage = TransactionalMapReplaceIfSameCodec.EncodeRequest(Name, TransactionId, ContextId, keyData, oldValueData, newValueData);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClientConnection).CAF();
            return TransactionalMapReplaceIfSameCodec.DecodeResponse(responseMessage).Response;
        }

        public async Task AddOrUpdateAsync(TKey key, TValue value)
        {
            var (keyData, valueData) = ToSafeData(key, value);

            var requestMessage = TransactionalMapSetCodec.EncodeRequest(Name, TransactionId, ContextId, keyData, valueData);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClientConnection).CAF();
            _ = TransactionalMapSetCodec.DecodeResponse(responseMessage);
        }

        public async Task<int> CountAsync()
        {
            var requestMessage = TransactionalMapSizeCodec.EncodeRequest(Name, TransactionId, ContextId);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClientConnection).CAF();
            return TransactionalMapSizeCodec.DecodeResponse(responseMessage).Response;
        }

        public async Task<IReadOnlyList<TValue>> GetValuesAsync()
        {
            var requestMessage = TransactionalMapValuesCodec.EncodeRequest(Name, TransactionId, ContextId);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClientConnection).CAF();
            var response = TransactionalMapValuesCodec.DecodeResponse(responseMessage).Response;
            return new ReadOnlyLazyList<TValue>(response, SerializationService);
        }

        public async Task<IReadOnlyList<TValue>> GetValuesAsync(IPredicate predicate)
        {
            var predicateData = ToSafeData(predicate);
            var requestMessage = TransactionalMapValuesWithPredicateCodec.EncodeRequest(Name, TransactionId, ContextId, predicateData);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClientConnection).CAF();
            var response = TransactionalMapValuesWithPredicateCodec.DecodeResponse(responseMessage).Response;
            return new ReadOnlyLazyList<TValue>(response, SerializationService);
        }
    }
}
