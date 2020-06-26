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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Predicates;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;
using Hazelcast.Serialization.Collections;
using Microsoft.Extensions.Logging;

namespace Hazelcast.DistributedObjects.HTxMapImpl
{
    internal class HTxMap<TKey, TValue> : TransactionalDistributedObjectBase, IHTxMap<TKey, TValue>
    {
        public HTxMap(string name, Cluster cluster, Client transactionClient, Guid transactionId, ISerializationService serializationService, ILoggerFactory loggerFactory)
            : base(HMap.ServiceName, name, cluster, transactionClient, transactionId, serializationService, loggerFactory)
        { }

        public Task<bool> ContainsKeyAsync(TKey key, TimeSpan timeout = default)
            => TaskEx.WithTimeout(ContainsKeyAsync, key, timeout, DefaultOperationTimeoutMilliseconds);

        public async Task<bool> ContainsKeyAsync(TKey key, CancellationToken cancellationToken)
        {
            var keyData = ToSafeData(key);
            var requestMessage = TransactionalMapContainsKeyCodec.EncodeRequest(Name, TransactionId, ContextId, keyData);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClient, cancellationToken).CAF();
            return TransactionalMapContainsKeyCodec.DecodeResponse(responseMessage).Response;
        }

        public Task RemoveAsync(TKey key, TimeSpan timeout = default)
            => TaskEx.WithTimeout(RemoveAsync, key, timeout, DefaultOperationTimeoutMilliseconds);

        public async Task RemoveAsync(TKey key, CancellationToken cancellationToken)
        {
            var keyData = ToSafeData(key);
            var requestMessage = TransactionalMapDeleteCodec.EncodeRequest(Name, TransactionId, ContextId, keyData);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClient, cancellationToken).CAF();
            _ = TransactionalMapDeleteCodec.DecodeResponse(responseMessage);
        }

        public Task<TValue> GetAsync(TKey key, TimeSpan timeout = default)
            => TaskEx.WithTimeout(GetAsync, key, timeout, DefaultOperationTimeoutMilliseconds);

        public async Task<TValue> GetAsync(TKey key, CancellationToken cancellationToken)
        {
            var keyData = ToSafeData(key);
            var requestMessage = TransactionalMapGetCodec.EncodeRequest(Name, TransactionId, ContextId, keyData);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClient, cancellationToken).CAF();
            var response = TransactionalMapGetCodec.DecodeResponse(responseMessage).Response;
            return ToObject<TValue>(response);
        }

        public Task<TValue> GetForUpdateAsync(TKey key, TimeSpan timeout = default)
            => TaskEx.WithTimeout(GetForUpdateAsync, key, timeout, DefaultOperationTimeoutMilliseconds);

        public async Task<TValue> GetForUpdateAsync(TKey key, CancellationToken cancellationToken)
        {
            var keyData = ToSafeData(key);
            var requestMessage = TransactionalMapGetForUpdateCodec.EncodeRequest(Name, TransactionId, ContextId, keyData);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClient, cancellationToken).CAF();
            var response = TransactionalMapGetForUpdateCodec.DecodeResponse(responseMessage).Response;
            return ToObject<TValue>(response);
        }

        public Task<bool> IsEmpty(TimeSpan timeout = default)
            => TaskEx.WithTimeout(IsEmpty, timeout, DefaultOperationTimeoutMilliseconds);

        public async Task<bool> IsEmpty(CancellationToken cancellationToken)
        {
            var requestMessage = TransactionalMapIsEmptyCodec.EncodeRequest(Name, TransactionId, ContextId);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClient, cancellationToken).CAF();
            return TransactionalMapIsEmptyCodec.DecodeResponse(responseMessage).Response;
        }

        public Task<IReadOnlyList<TKey>> GetKeysAsync(TimeSpan timeout = default)
            => TaskEx.WithTimeout(GetKeysAsync, timeout, DefaultOperationTimeoutMilliseconds);

        public async Task<IReadOnlyList<TKey>> GetKeysAsync(CancellationToken cancellationToken)
        {
            var requestMessage = TransactionalMapKeySetCodec.EncodeRequest(Name, TransactionId, ContextId);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClient, cancellationToken).CAF();
            var response = TransactionalMapKeySetCodec.DecodeResponse(responseMessage).Response;
            return new ReadOnlyLazyList<TKey>(response, SerializationService);
        }

        public Task<IReadOnlyList<TKey>> GetKeysAsync(IPredicate predicate, TimeSpan timeout = default)
            => TaskEx.WithTimeout(GetKeysAsync, timeout, DefaultOperationTimeoutMilliseconds);

        public async Task<IReadOnlyList<TKey>> GetKeysAsync(IPredicate predicate, CancellationToken cancellationToken)
        {
            var predicateData = ToSafeData(predicate);
            var requestMessage = TransactionalMapKeySetWithPredicateCodec.EncodeRequest(Name, TransactionId, ContextId, predicateData);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClient, cancellationToken).CAF();
            var response = TransactionalMapKeySetWithPredicateCodec.DecodeResponse(responseMessage).Response;
            return new ReadOnlyLazyList<TKey>(response, SerializationService);
        }

        public Task<TValue> AddOrReplaceAndReturnAsync(TKey key, TValue value, TimeSpan timeout = default)
            => TaskEx.WithTimeout(AddOrReplaceAndReturnTtlAsync, key, value, TimeToLive.InfiniteTimeSpan, timeout, DefaultOperationTimeoutMilliseconds);

        public Task<TValue> AddOrReplaceAndReturnAsync(TKey key, TValue value, CancellationToken cancellationToken)
            => AddOrReplaceAndReturnTtlAsync(key, value, TimeToLive.InfiniteTimeSpan, cancellationToken);

        public Task<TValue> AddOrReplaceAndReturnTtlAsync(TKey key, TValue value, TimeSpan timeToLive, TimeSpan timeout = default)
            => TaskEx.WithTimeout(AddOrReplaceAndReturnTtlAsync, key, value, timeToLive, timeout, DefaultOperationTimeoutMilliseconds);

        public async Task<TValue> AddOrReplaceAndReturnTtlAsync(TKey key, TValue value, TimeSpan timeToLive, CancellationToken cancellationToken)
        {
            var (keyData, valueData) = ToSafeData(key, value);
            var timeToLiveMilliseconds = timeToLive.CodecMilliseconds(-1);
            var requestMessage = TransactionalMapPutCodec.EncodeRequest(Name, TransactionId, ContextId, keyData, valueData, timeToLiveMilliseconds);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClient, cancellationToken).CAF();
            var response = TransactionalMapPutCodec.DecodeResponse(responseMessage).Response;
            return ToObject<TValue>(response);
        }

        public Task<TValue> AddIfMissing(TKey key, TValue value, TimeSpan timeout = default)
            => TaskEx.WithTimeout(AddIfMissing, key, value, timeout, DefaultOperationTimeoutMilliseconds);

        public async Task<TValue> AddIfMissing(TKey key, TValue value, CancellationToken cancellationToken)
        {
            var (keyData, valueData) = ToSafeData(key, value);

            var requestMessage = TransactionalMapPutIfAbsentCodec.EncodeRequest(Name, TransactionId, ContextId, keyData, valueData);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClient, cancellationToken).CAF();
            var response = TransactionalMapPutIfAbsentCodec.DecodeResponse(responseMessage).Response;
            return ToObject<TValue>(response);
        }

        public Task<TValue> RemoveAndReturnAsync(TKey key, TimeSpan timeout = default)
            => TaskEx.WithTimeout(RemoveAndReturnAsync, key, timeout, DefaultOperationTimeoutMilliseconds);

        public async Task<TValue> RemoveAndReturnAsync(TKey key, CancellationToken cancellationToken)
        {
            var keyData = ToSafeData(key);
            var requestMessage = TransactionalMapRemoveCodec.EncodeRequest(Name, TransactionId, ContextId, keyData);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClient, cancellationToken).CAF();
            var response = TransactionalMapRemoveCodec.DecodeResponse(responseMessage).Response;
            return ToObject<TValue>(response);
        }

        public Task<bool> RemoveAsync(TKey key, TValue value, TimeSpan timeout = default)
            => TaskEx.WithTimeout(RemoveAsync, key, value, timeout, DefaultOperationTimeoutMilliseconds);

        public async Task<bool> RemoveAsync(TKey key, TValue value, CancellationToken cancellationToken)
        {
            var (keyData, valueData) = ToSafeData(key, value);
            var requestMessage = TransactionalMapRemoveIfSameCodec.EncodeRequest(Name, TransactionId, ContextId, keyData, valueData);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClient, cancellationToken).CAF();
            return TransactionalMapRemoveIfSameCodec.DecodeResponse(responseMessage).Response;
        }

        public Task<TValue> ReplaceAndReturnAsync(TKey key, TValue value, TimeSpan timeout = default)
            => TaskEx.WithTimeout(ReplaceAndReturnAsync, key, value, timeout, DefaultOperationTimeoutMilliseconds);

        public async Task<TValue> ReplaceAndReturnAsync(TKey key, TValue value, CancellationToken cancellationToken)
        {
            var (keyData, valueData) = ToSafeData(key, value);

            var requestMessage = TransactionalMapReplaceCodec.EncodeRequest(Name, TransactionId, ContextId, keyData, valueData);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClient, cancellationToken).CAF();
            var response = TransactionalMapReplaceCodec.DecodeResponse(responseMessage).Response;
            return ToObject<TValue>(response);
        }

        public Task<bool> ReplaceAsync(TKey key, TValue oldValue, TValue newValue, TimeSpan timeout = default)
            => TaskEx.WithTimeout(ReplaceAsync, key, oldValue, newValue, timeout, DefaultOperationTimeoutMilliseconds);

        public async Task<bool> ReplaceAsync(TKey key, TValue oldValue, TValue newValue, CancellationToken cancellationToken)
        {
            var (keyData, oldValueData, newValueData) = ToSafeData(key, oldValue, newValue);

            var requestMessage = TransactionalMapReplaceIfSameCodec.EncodeRequest(Name, TransactionId, ContextId, keyData, oldValueData, newValueData);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClient, cancellationToken).CAF();
            return TransactionalMapReplaceIfSameCodec.DecodeResponse(responseMessage).Response;
        }

        public Task AddOrReplaceAsync(TKey key, TValue value, TimeSpan timeout = default)
            => TaskEx.WithTimeout(AddOrReplaceAsync, key, value, timeout, DefaultOperationTimeoutMilliseconds);

        public async Task AddOrReplaceAsync(TKey key, TValue value, CancellationToken cancellationToken)
        {
            var (keyData, valueData) = ToSafeData(key, value);

            var requestMessage = TransactionalMapSetCodec.EncodeRequest(Name, TransactionId, ContextId, keyData, valueData);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClient, cancellationToken).CAF();
            _ = TransactionalMapSetCodec.DecodeResponse(responseMessage);
        }

        public Task<int> CountAsync(TimeSpan timeout = default)
            => TaskEx.WithTimeout(CountAsync, timeout, DefaultOperationTimeoutMilliseconds);

        public async Task<int> CountAsync(CancellationToken cancellationToken)
        {
            var requestMessage = TransactionalMapSizeCodec.EncodeRequest(Name, TransactionId, ContextId);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClient, cancellationToken).CAF();
            return TransactionalMapSizeCodec.DecodeResponse(responseMessage).Response;
        }

        public Task<IReadOnlyList<TValue>> GetValuesAsync(TimeSpan timeout = default)
            => TaskEx.WithTimeout(GetValuesAsync, timeout, DefaultOperationTimeoutMilliseconds);

        public async Task<IReadOnlyList<TValue>> GetValuesAsync(CancellationToken cancellationToken)
        {
            var requestMessage = TransactionalMapValuesCodec.EncodeRequest(Name, TransactionId, ContextId);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClient, cancellationToken).CAF();
            var response = TransactionalMapValuesCodec.DecodeResponse(responseMessage).Response;
            return new ReadOnlyLazyList<TValue>(response, SerializationService);
        }

        public Task<IReadOnlyList<TValue>> GetValuesAsync(IPredicate predicate, TimeSpan timeout = default)
            => TaskEx.WithTimeout(GetValuesAsync, predicate, timeout, DefaultOperationTimeoutMilliseconds);

        public async Task<IReadOnlyList<TValue>> GetValuesAsync(IPredicate predicate, CancellationToken cancellationToken)
        {
            var predicateData = ToSafeData(predicate);
            var requestMessage = TransactionalMapValuesWithPredicateCodec.EncodeRequest(Name, TransactionId, ContextId, predicateData);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClient, cancellationToken).CAF();
            var response = TransactionalMapValuesWithPredicateCodec.DecodeResponse(responseMessage).Response;
            return new ReadOnlyLazyList<TValue>(response, SerializationService);
        }
    }
}
