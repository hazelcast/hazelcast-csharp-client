// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Protocol.Codecs;
using Hazelcast.Query;
using Hazelcast.Serialization;
using Hazelcast.Serialization.Collections;
using Microsoft.Extensions.Logging;

namespace Hazelcast.DistributedObjects.Impl
{
    internal class HTxMap<TKey, TValue> : TransactionalDistributedObjectBase, IHTxMap<TKey, TValue>
    {
        public HTxMap(string name, DistributedObjectFactory factory, Cluster cluster, MemberConnection transactionClientConnection, Guid transactionId, SerializationService serializationService, ILoggerFactory loggerFactory)
            : base(ServiceNames.Map, name, factory, cluster, transactionClientConnection, transactionId, serializationService, loggerFactory)
        { }

        public async Task<bool> ContainsKeyAsync(TKey key)
        {
            var keyData = ToSafeData(key);
            var requestMessage = TransactionalMapContainsKeyCodec.EncodeRequest(Name, TransactionId, ContextId, keyData);
            var responseMessage = await Cluster.Messaging.SendToMemberAsync(requestMessage, TransactionClientConnection).CfAwait();
            return TransactionalMapContainsKeyCodec.DecodeResponse(responseMessage).Response;
        }

        public async Task DeleteAsync(TKey key)
        {
            var keyData = ToSafeData(key);
            var requestMessage = TransactionalMapDeleteCodec.EncodeRequest(Name, TransactionId, ContextId, keyData);
            var responseMessage = await Cluster.Messaging.SendToMemberAsync(requestMessage, TransactionClientConnection).CfAwait();
            _ = TransactionalMapDeleteCodec.DecodeResponse(responseMessage);
        }

        public async Task<TValue> GetAsync(TKey key)
        {
            var keyData = ToSafeData(key);
            var requestMessage = TransactionalMapGetCodec.EncodeRequest(Name, TransactionId, ContextId, keyData);
            var responseMessage = await Cluster.Messaging.SendToMemberAsync(requestMessage, TransactionClientConnection).CfAwait();
            var response = TransactionalMapGetCodec.DecodeResponse(responseMessage).Response;

            return await ToObjectAsync<TValue>(response).CfAwait();
        }

        public async Task<TValue> GetForUpdateAsync(TKey key)
        {
            var keyData = ToSafeData(key);
            var requestMessage = TransactionalMapGetForUpdateCodec.EncodeRequest(Name, TransactionId, ContextId, keyData);
            var responseMessage = await Cluster.Messaging.SendToMemberAsync(requestMessage, TransactionClientConnection).CfAwait();
            var response = TransactionalMapGetForUpdateCodec.DecodeResponse(responseMessage).Response;
            return await ToObjectAsync<TValue>(response).CfAwait();
        }

        public async Task<bool> IsEmptyAsync()
        {
            var requestMessage = TransactionalMapIsEmptyCodec.EncodeRequest(Name, TransactionId, ContextId);
            var responseMessage = await Cluster.Messaging.SendToMemberAsync(requestMessage, TransactionClientConnection).CfAwait();
            return TransactionalMapIsEmptyCodec.DecodeResponse(responseMessage).Response;
        }

        public async Task<IReadOnlyList<TKey>> GetKeysAsync()
        {
            var requestMessage = TransactionalMapKeySetCodec.EncodeRequest(Name, TransactionId, ContextId);
            var responseMessage = await Cluster.Messaging.SendToMemberAsync(requestMessage, TransactionClientConnection).CfAwait();
            var response = TransactionalMapKeySetCodec.DecodeResponse(responseMessage).Response;
            var result = new ReadOnlyLazyList<TKey>(SerializationService);
            await result.AddAsync(response).CfAwait();
            return result;
        }

        public async Task<IReadOnlyList<TKey>> GetKeysAsync(IPredicate predicate)
        {
            var predicateData = ToSafeData(predicate);
            var requestMessage = TransactionalMapKeySetWithPredicateCodec.EncodeRequest(Name, TransactionId, ContextId, predicateData);
            var responseMessage = await Cluster.Messaging.SendToMemberAsync(requestMessage, TransactionClientConnection).CfAwait();
            var response = TransactionalMapKeySetWithPredicateCodec.DecodeResponse(responseMessage).Response;
            var result = new ReadOnlyLazyList<TKey>(SerializationService);
            await result.AddAsync(response).CfAwait();
            return result;
        }

        public async Task SetAsync(TKey key, TValue value)
        {
            var (keyData, valueData) = ToSafeData(key, value);

            var requestMessage = TransactionalMapSetCodec.EncodeRequest(Name, TransactionId, ContextId, keyData, valueData);
            var responseMessage = await Cluster.Messaging.SendToMemberAsync(requestMessage, TransactionClientConnection).CfAwait();
            _ = TransactionalMapSetCodec.DecodeResponse(responseMessage);
        }

        public Task<TValue> PutAsync(TKey key, TValue value)
            => PutAsync(key, value, TimeSpanExtensions.MinusOneMillisecond);

        public async Task<TValue> PutAsync(TKey key, TValue value, TimeSpan timeToLive)
        {
            var (keyData, valueData) = ToSafeData(key, value);

            var timeToLiveMs = timeToLive.RoundedMilliseconds(false);

             var requestMessage = TransactionalMapPutCodec.EncodeRequest(Name, TransactionId, ContextId, keyData, valueData, timeToLiveMs);
            var responseMessage = await Cluster.Messaging.SendToMemberAsync(requestMessage, TransactionClientConnection).CfAwait();
            var response = TransactionalMapPutCodec.DecodeResponse(responseMessage).Response;
            return await ToObjectAsync<TValue>(response).CfAwait();
        }

        public async Task<TValue> PutIfAbsentAsync(TKey key, TValue value)
        {
            var (keyData, valueData) = ToSafeData(key, value);

            var requestMessage = TransactionalMapPutIfAbsentCodec.EncodeRequest(Name, TransactionId, ContextId, keyData, valueData);
            var responseMessage = await Cluster.Messaging.SendToMemberAsync(requestMessage, TransactionClientConnection).CfAwait();
            var response = TransactionalMapPutIfAbsentCodec.DecodeResponse(responseMessage).Response;
            return await ToObjectAsync<TValue>(response).CfAwait();
        }

        public async Task<TValue> RemoveAsync(TKey key)
        {
            var keyData = ToSafeData(key);
            var requestMessage = TransactionalMapRemoveCodec.EncodeRequest(Name, TransactionId, ContextId, keyData);
            var responseMessage = await Cluster.Messaging.SendToMemberAsync(requestMessage, TransactionClientConnection).CfAwait();
            var response = TransactionalMapRemoveCodec.DecodeResponse(responseMessage).Response;
            return await ToObjectAsync<TValue>(response).CfAwait();
        }


        public async Task<bool> RemoveAsync(TKey key, TValue value)
        {
            var (keyData, valueData) = ToSafeData(key, value);
            var requestMessage = TransactionalMapRemoveIfSameCodec.EncodeRequest(Name, TransactionId, ContextId, keyData, valueData);
            var responseMessage = await Cluster.Messaging.SendToMemberAsync(requestMessage, TransactionClientConnection).CfAwait();
            return TransactionalMapRemoveIfSameCodec.DecodeResponse(responseMessage).Response;
        }

        public async Task<TValue> ReplaceAsync(TKey key, TValue newValue)
        {
            var (keyData, valueData) = ToSafeData(key, newValue);

            var requestMessage = TransactionalMapReplaceCodec.EncodeRequest(Name, TransactionId, ContextId, keyData, valueData);
            var responseMessage = await Cluster.Messaging.SendToMemberAsync(requestMessage, TransactionClientConnection).CfAwait();
            var response = TransactionalMapReplaceCodec.DecodeResponse(responseMessage).Response;
            return await ToObjectAsync<TValue>(response).CfAwait();
        }

        public async Task<bool> ReplaceAsync(TKey key, TValue oldValue, TValue newValue)
        {
            var (keyData, oldValueData, newValueData) = ToSafeData(key, oldValue, newValue);

            var requestMessage = TransactionalMapReplaceIfSameCodec.EncodeRequest(Name, TransactionId, ContextId, keyData, oldValueData, newValueData);
            var responseMessage = await Cluster.Messaging.SendToMemberAsync(requestMessage, TransactionClientConnection).CfAwait();
            return TransactionalMapReplaceIfSameCodec.DecodeResponse(responseMessage).Response;
        }

        public async Task<int> GetSizeAsync()
        {
            var requestMessage = TransactionalMapSizeCodec.EncodeRequest(Name, TransactionId, ContextId);
            var responseMessage = await Cluster.Messaging.SendToMemberAsync(requestMessage, TransactionClientConnection).CfAwait();
            return TransactionalMapSizeCodec.DecodeResponse(responseMessage).Response;
        }

        public async Task<IReadOnlyCollection<TValue>> GetValuesAsync()
        {
            var requestMessage = TransactionalMapValuesCodec.EncodeRequest(Name, TransactionId, ContextId);
            var responseMessage = await Cluster.Messaging.SendToMemberAsync(requestMessage, TransactionClientConnection).CfAwait();
            var response = TransactionalMapValuesCodec.DecodeResponse(responseMessage).Response;
            var result = new ReadOnlyLazyList<TValue>(SerializationService);
            await result.AddAsync(response).CfAwait();
            return result;
        }

        public async Task<IReadOnlyCollection<TValue>> GetValuesAsync(IPredicate predicate)
        {
            var predicateData = ToSafeData(predicate);
            var requestMessage = TransactionalMapValuesWithPredicateCodec.EncodeRequest(Name, TransactionId, ContextId, predicateData);
            var responseMessage = await Cluster.Messaging.SendToMemberAsync(requestMessage, TransactionClientConnection).CfAwait();
            var response = TransactionalMapValuesWithPredicateCodec.DecodeResponse(responseMessage).Response;
            var result = new ReadOnlyLazyList<TValue>(SerializationService);
            await result.AddAsync(response).CfAwait();
            return result;
        }
    }
}
