﻿// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using System.Linq;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Core.Collections;
using Hazelcast.Data.Map;
using Hazelcast.Messaging;
using Hazelcast.Predicates;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Protocol.Data;
using Hazelcast.Serialization;

namespace Hazelcast.DistributedObjects.Implementation.Map
{
    // partial: getting
    internal partial class Map<TKey, TValue>
    {
        // NOTES
        //
        // all protected getters return objects, because a cached version of the map, overriding these
        // methods, may choose to cache either the raw IData values coming from the server, or the
        // de-serialized objects. In the first case, ToObject<TValue> deserializes the value. In the
        // second case, ToObject<TValue> casts the object value to TValue.

        /// <inheritdoc />
        public async Task<TValue> GetAsync(TKey key)
            => ToObject<TValue>(await GetAsync(ToSafeData(key)));

        /// <summary>
        /// Gets the value for a key, or null if the map does not contain an entry with this key.
        /// </summary>
        /// <param name="keyData">The key data.</param>
        /// <returns>The value data for the specified key, or null if the map does not contain an entry with this key.</returns>
        protected virtual async Task<object> GetAsync(IData keyData)
        {
            var requestMessage = MapGetCodec.EncodeRequest(Name, keyData, ThreadId);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, keyData);
            var response = MapGetCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyDictionary<TKey, TValue>> GetAsync(ICollection<TKey> keys)
        {
            var ownerKeys = new Dictionary<Guid, Dictionary<int, List<IData>>>();

            // verify keys + group by owner and partitions
            foreach (var key in keys)
            {
                var keyData = ToSafeData(key);

                var partitionId = Cluster.Partitioner.GetPartitionId(keyData);
                var ownerId = Cluster.Partitioner.GetPartitionOwner(partitionId);
                if (!ownerKeys.TryGetValue(ownerId, out var part))
                    part = ownerKeys[ownerId] = new Dictionary<int, List<IData>>();
                if (!part.TryGetValue(partitionId, out var list))
                    list = part[partitionId] = new List<IData>();
                list.Add(keyData);
            }

            return await GetAsync(ownerKeys);
        }

        /// <summary>
        /// Gets all entries for keys.
        /// </summary>
        /// <param name="ownerKeys">Keys.</param>
        /// <returns>The values for the specified keys.</returns>
        protected virtual async Task<ReadOnlyLazyDictionary<TKey, TValue>> GetAsync(Dictionary<Guid, Dictionary<int, List<IData>>> ownerKeys)
        {
            // create parallel tasks to fire a request for each owner
            var tasks = new List<Task<ClientMessage>>();
            foreach (var (ownerId, part) in ownerKeys)
            {
                foreach (var (partitionId, list) in part)
                {
                    if (list.Count == 0) continue;

                    var requestMessage = MapGetAllCodec.EncodeRequest(Name, list);
                    requestMessage.PartitionId = partitionId;
                    var task = Cluster.SendToMemberAsync(requestMessage, ownerId).AsTask();
                    tasks.Add(task);
                }
            }

            // and wait on all tasks, gathering the responses
            await Task.WhenAll(tasks);

            // decode all responses, in 1 thread: this is CPU-bound
            // (we may want to introduce some parallelism, though, depending on # of cores)
            var result = new ReadOnlyLazyDictionary<TKey, TValue>(SerializationService);
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var task in tasks)
            {
                var responseMessage = task.Result; // safe: we know the task has completed
                var response = MapGetAllCodec.DecodeResponse(responseMessage).Response;
                result.Add(response);
            }

            return result;
        }

        /// <inheritdoc />
        public async Task<IMapEntry<TKey, TValue>> GetEntryAsync(TKey key)
        {
            var keyData = ToSafeData(key);

            var requestMessage = MapGetEntryViewCodec.EncodeRequest(Name, keyData, ThreadId);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, keyData);
            var response = MapGetEntryViewCodec.DecodeResponse(responseMessage).Response;

            if (response == null) return null;

            return new MapEntry<TKey, TValue>
            {
                Key = ToObject<TKey>(response.Key),
                Value = ToObject<TValue>(response.Value),
                Cost = response.Cost,
                CreationTime = response.CreationTime,
                ExpirationTime = response.ExpirationTime,
                Hits = response.Hits,
                LastAccessTime = response.LastAccessTime,
                LastStoredTime = response.LastStoredTime,
                LastUpdateTime = response.LastUpdateTime,
                Version = response.Version,
                EvictionCriteriaNumber = response.EvictionCriteriaNumber,
                Ttl = response.Ttl
            };
        }

        /// <inheritdoc />
        public async Task<IReadOnlyDictionary<TKey, TValue>> GetAsync(IPredicate predicate = null)
        {
            if (predicate == null)
            {
                var requestMessage = MapEntrySetCodec.EncodeRequest(Name);
                var responseMessage = await Cluster.SendAsync(requestMessage);
                var response = MapEntrySetCodec.DecodeResponse(responseMessage).Response;
                return new ReadOnlyLazyDictionary<TKey, TValue>(SerializationService) { response };
            }

            var pagingPredicate = UnwrapPagingPredicate(predicate);
            if (pagingPredicate != null)
            {
                pagingPredicate.IterationType = IterationType.Entry;

                var pagingPredicateHolder = PagingPredicateHolder.Of(predicate, SerializationService);
                var requestMessage = MapEntriesWithPagingPredicateCodec.EncodeRequest(Name, pagingPredicateHolder);
                var responseMessage = await Cluster.SendAsync(requestMessage);
                var response = MapEntriesWithPagingPredicateCodec.DecodeResponse(responseMessage);
                pagingPredicate.AnchorList = response.AnchorDataList.AsAnchorIterator(SerializationService).ToList();
                return new ReadOnlyLazyDictionary<TKey, TValue>(SerializationService) { response.Response };
            }

            {
                var requestMessage = MapEntriesWithPredicateCodec.EncodeRequest(Name, ToData(predicate));
                var responseMessage = await (predicate is PartitionPredicate pp
                    ? Cluster.SendToKeyPartitionOwnerAsync(requestMessage, SerializationService.ToData(pp.GetPartitionKey()))
                    : Cluster.SendAsync(requestMessage));
                var response = MapEntriesWithPredicateCodec.DecodeResponse(responseMessage).Response;
                return new ReadOnlyLazyDictionary<TKey, TValue>(SerializationService) { response };
            }
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<TKey>> GetKeysAsync(IPredicate predicate = null)
        {
            if (predicate == null)
            {
                var requestMessage = MapKeySetCodec.EncodeRequest(Name);
                var responseMessage = await Cluster.SendAsync(requestMessage);
                var response = MapKeySetCodec.DecodeResponse(responseMessage).Response;
                return new ReadOnlyLazyList<TKey>(response, SerializationService);
            }

            var pagingPredicate = UnwrapPagingPredicate(predicate);
            if (pagingPredicate != null)
            {
                pagingPredicate.IterationType = IterationType.Key;

                var pagingPredicateHolder = PagingPredicateHolder.Of(predicate, SerializationService);
                var requestMessage = MapKeySetWithPagingPredicateCodec.EncodeRequest(Name, pagingPredicateHolder);
                var responseMessage = await Cluster.SendAsync(requestMessage);
                var response = MapKeySetWithPagingPredicateCodec.DecodeResponse(responseMessage);
                pagingPredicate.AnchorList = response.AnchorDataList.AsAnchorIterator(SerializationService).ToList();
                return new ReadOnlyLazyList<TKey>(response.Response, SerializationService);
            }

            {
                var requestMessage = MapKeySetWithPredicateCodec.EncodeRequest(Name, ToData(predicate));
                var responseMessage = await (predicate is PartitionPredicate pp
                    ? Cluster.SendToKeyPartitionOwnerAsync(requestMessage, SerializationService.ToData(pp.GetPartitionKey()))
                    : Cluster.SendAsync(requestMessage));
                var response = MapKeySetWithPredicateCodec.DecodeResponse(responseMessage).Response;
                return new ReadOnlyLazyList<TKey>(response, SerializationService);
            }
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<TValue>> GetValuesAsync(IPredicate predicate = null)
        {
            if (predicate == null)
            {
                var requestMessage = MapValuesCodec.EncodeRequest(Name);
                var responseMessage = await Cluster.SendAsync(requestMessage);
                var response = MapValuesCodec.DecodeResponse(responseMessage).Response;
                return new ReadOnlyLazyList<TValue>(response, SerializationService);
            }

            var pagingPredicate = UnwrapPagingPredicate(predicate);
            if (pagingPredicate != null)
            {
                pagingPredicate.IterationType = IterationType.Value;

                var pagingPredicateHolder = PagingPredicateHolder.Of(predicate, SerializationService);
                var requestMessage = MapValuesWithPagingPredicateCodec.EncodeRequest(Name, pagingPredicateHolder);
                var responseMessage = await Cluster.SendAsync(requestMessage);
                var response = MapValuesWithPagingPredicateCodec.DecodeResponse(responseMessage);
                pagingPredicate.AnchorList = response.AnchorDataList.AsAnchorIterator(SerializationService).ToList();
                return new ReadOnlyLazyList<TValue>(response.Response, SerializationService);
            }

            {
                var requestMessage = MapValuesWithPredicateCodec.EncodeRequest(Name, ToData(predicate));
                var responseMessage = await (predicate is PartitionPredicate pp
                    ? Cluster.SendToKeyPartitionOwnerAsync(requestMessage, SerializationService.ToData(pp.GetPartitionKey()))
                    : Cluster.SendAsync(requestMessage));
                var response = MapValuesWithPredicateCodec.DecodeResponse(responseMessage).Response;
                return new ReadOnlyLazyList<TValue>(response, SerializationService);
            }
        }

        /// <inheritdoc />
        public async Task<int> CountAsync()
        {
            var requestMessage = MapSizeCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.SendAsync(requestMessage);
            var response = MapSizeCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public async Task<bool> IsEmptyAsync()
        {
            var requestMessage = MapIsEmptyCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.SendAsync(requestMessage);
            var response = MapIsEmptyCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public async Task<bool> ContainsKeyAsync(TKey key)
            => await ContainsKeyAsync(ToSafeData(key));

        /// <summary>
        /// Determines whether this map contains an entry for a key.
        /// </summary>
        /// <param name="keyData">The key.</param>
        /// <returns>True if the map contains an entry for the specified key; otherwise false.</returns>
        protected virtual async Task<bool> ContainsKeyAsync(IData keyData)
        {
            var requestMessage = MapContainsKeyCodec.EncodeRequest(Name, keyData, ThreadId);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, keyData);
            var response = MapContainsKeyCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public async Task<bool> ContainsValueAsync(TValue value)
        {
            var valueData = ToSafeData(value);

            var requestMessage = MapContainsValueCodec.EncodeRequest(Name, valueData);
            var responseMessage = await Cluster.SendAsync(requestMessage);
            var response = MapContainsValueCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        private static PagingPredicate UnwrapPagingPredicate(IPredicate predicate)
            => predicate as PagingPredicate ?? (predicate as PartitionPredicate)?.GetTarget() as PagingPredicate;
    }
}