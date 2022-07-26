// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Core;
using Hazelcast.Messaging;
using Hazelcast.Models;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Protocol.Models;
using Hazelcast.Query;
using Hazelcast.Serialization;
using Hazelcast.Serialization.Collections;

namespace Hazelcast.DistributedObjects.Impl
{
    internal partial class HMap<TKey, TValue> // Getting
    {
        /// <inheritdoc />
        public Task<TValue> GetAsync(TKey key)
            => GetAsync(key, CancellationToken.None);

        private async Task<TValue> GetAsync(TKey key, CancellationToken cancellationToken)
            => await GetAsync(ToSafeData(key), cancellationToken).CfAwait();

        /// <summary>
        /// Gets the value for a key, or null if the map does not contain an entry with this key.
        /// </summary>
        /// <param name="keyData">The key data.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The value for the specified key.</returns>
        protected virtual async Task<TValue> GetAsync(IData keyData, CancellationToken cancellationToken)
        {
            // TODO: avoid boxing when ToObject-ing the value
            var valueData = await GetDataAsync(keyData, cancellationToken).CfAwait();
            return ToObject<TValue>(valueData);
        }

        /// <summary>
        /// Gets the value data for a key, or null if the map does not contain an entry with this key.
        /// </summary>
        /// <param name="keyData">The key data.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The value data for the specified key, or null if the map does not contain an entry with this key.</returns>
        protected async Task<IData> GetDataAsync(IData keyData, CancellationToken cancellationToken)
        {
            var requestMessage = MapGetCodec.EncodeRequest(Name, keyData, ContextId);
            var responseMessage = await Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, keyData, cancellationToken).CfAwait();
            var response = MapGetCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public Task<IReadOnlyDictionary<TKey, TValue>> GetAllAsync(ICollection<TKey> keys)
            => GetAllAsync(keys, CancellationToken.None);

        private
#if !HZ_OPTIMIZE_ASYNC
        async
#endif
        Task<IReadOnlyDictionary<TKey, TValue>> GetAllAsync(ICollection<TKey> keys, CancellationToken cancellationToken)
        {
            var ownerKeys = new Dictionary<Guid, Dictionary<int, List<IData>>>();

            // verify keys + group by owner and partitions
            foreach (var key in keys)
            {
                var keyData = ToSafeData(key);

                var partitionId = Cluster.Partitioner.GetPartitionId(keyData.PartitionHash);
                var ownerId = Cluster.Partitioner.GetPartitionOwner(partitionId);
                if (!ownerKeys.TryGetValue(ownerId, out var part))
                    part = ownerKeys[ownerId] = new Dictionary<int, List<IData>>();
                if (!part.TryGetValue(partitionId, out var list))
                    list = part[partitionId] = new List<IData>();
                list.Add(keyData);
            }

            var task = GetAllAsync(ownerKeys, cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CfAwait();
#endif
        }

        /// <summary>
        /// Gets all entries for keys.
        /// </summary>
        /// <param name="ownerKeys">Keys.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The values for the specified keys.</returns>
        protected virtual async Task<ReadOnlyLazyDictionary<TKey, TValue>> GetAllAsync(Dictionary<Guid, Dictionary<int, List<IData>>> ownerKeys, CancellationToken cancellationToken)
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
                    var task = Cluster.Messaging.SendToMemberAsync(requestMessage, ownerId, cancellationToken);
                    tasks.Add(task);
                }
            }

            // and wait on all tasks, gathering the responses
            await Task.WhenAll(tasks).CfAwait();

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
        public Task<IMapEntryStats<TKey, TValue>> GetEntryViewAsync(TKey key)
            => GetEntryStatsAsync(key, CancellationToken.None);

        private async Task<IMapEntryStats<TKey, TValue>> GetEntryStatsAsync(TKey key, CancellationToken cancellationToken)
        {
            var keyData = ToSafeData(key);

            var requestMessage = MapGetEntryViewCodec.EncodeRequest(Name, keyData, ContextId);
            var responseMessage = await Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, keyData, cancellationToken).CfAwait();
            var response = MapGetEntryViewCodec.DecodeResponse(responseMessage).Response;

            if (response == null) return null;

            return new MapEntryStats<TKey, TValue>
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
        public Task<IReadOnlyDictionary<TKey, TValue>> GetEntriesAsync()
            => GetEntriesAsync(CancellationToken.None);

        private async Task<IReadOnlyDictionary<TKey, TValue>> GetEntriesAsync(CancellationToken cancellationToken)
        {
            var requestMessage = MapEntrySetCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.Messaging.SendAsync(requestMessage, cancellationToken).CfAwait();
            var response = MapEntrySetCodec.DecodeResponse(responseMessage).Response;
            return new ReadOnlyLazyDictionary<TKey, TValue>(SerializationService) { response };
        }

        /// <inheritdoc />
        public Task<IReadOnlyDictionary<TKey, TValue>> GetEntriesAsync(IPredicate predicate)
            => GetEntriesAsync(predicate, CancellationToken.None);

        private async Task<IReadOnlyDictionary<TKey, TValue>> GetEntriesAsync(IPredicate predicate, CancellationToken cancellationToken)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            var pagingPredicate = UnwrapPagingPredicate(predicate);
            if (pagingPredicate != null)
            {
                pagingPredicate.IterationType = IterationType.Entry;

                var pagingPredicateHolder = PagingPredicateHolder.Of(predicate, SerializationService);
                var requestMessage = MapEntriesWithPagingPredicateCodec.EncodeRequest(Name, pagingPredicateHolder);
                var responseMessage = await Cluster.Messaging.SendAsync(requestMessage, cancellationToken).CfAwait();
                var response = MapEntriesWithPagingPredicateCodec.DecodeResponse(responseMessage);
                pagingPredicate.UpdateAnchors(response.AnchorDataList.AsAnchorIterator(SerializationService));
                return new ReadOnlyLazyDictionary<TKey, TValue>(SerializationService) { response.Response };
            }

            {
                var requestMessage = MapEntriesWithPredicateCodec.EncodeRequest(Name, ToData(predicate));
                var responseMessage = await (predicate is PartitionPredicate pp
                    ? Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, SerializationService.ToData(pp.PartitionKey), cancellationToken)
                    : Cluster.Messaging.SendAsync(requestMessage, cancellationToken))
                    .CfAwait();
                var response = MapEntriesWithPredicateCodec.DecodeResponse(responseMessage).Response;
                return new ReadOnlyLazyDictionary<TKey, TValue>(SerializationService) { response };
            }
        }

        /// <inheritdoc />
        public Task<IReadOnlyCollection<TKey>> GetKeysAsync()
            => GetKeysAsync(CancellationToken.None);

        private async Task<IReadOnlyCollection<TKey>> GetKeysAsync(CancellationToken cancellationToken)
        {
            var requestMessage = MapKeySetCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.Messaging.SendAsync(requestMessage, cancellationToken).CfAwait();
            var response = MapKeySetCodec.DecodeResponse(responseMessage).Response;
            return new ReadOnlyLazyList<TKey>(response, SerializationService);
        }

        /// <inheritdoc />
        public Task<IReadOnlyCollection<TKey>> GetKeysAsync(IPredicate predicate)
            => GetKeysAsync(predicate, CancellationToken.None);

        private async Task<IReadOnlyCollection<TKey>> GetKeysAsync(IPredicate predicate, CancellationToken cancellationToken)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            var pagingPredicate = UnwrapPagingPredicate(predicate);
            if (pagingPredicate != null)
            {
                pagingPredicate.IterationType = IterationType.Key;

                var pagingPredicateHolder = PagingPredicateHolder.Of(predicate, SerializationService);
                var requestMessage = MapKeySetWithPagingPredicateCodec.EncodeRequest(Name, pagingPredicateHolder);
                var responseMessage = await Cluster.Messaging.SendAsync(requestMessage, cancellationToken).CfAwait();
                var response = MapKeySetWithPagingPredicateCodec.DecodeResponse(responseMessage);
                pagingPredicate.UpdateAnchors(response.AnchorDataList.AsAnchorIterator(SerializationService));
                return new ReadOnlyLazyList<TKey>(response.Response, SerializationService);
            }

            {
                var requestMessage = MapKeySetWithPredicateCodec.EncodeRequest(Name, ToData(predicate));
                var responseMessage = await (predicate is PartitionPredicate pp
                    ? Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, SerializationService.ToData(pp.PartitionKey), cancellationToken)
                    : Cluster.Messaging.SendAsync(requestMessage, cancellationToken))
                    .CfAwait();
                var response = MapKeySetWithPredicateCodec.DecodeResponse(responseMessage).Response;
                return new ReadOnlyLazyList<TKey>(response, SerializationService);
            }
        }

        /// <inheritdoc />
        public Task<IReadOnlyCollection<TValue>> GetValuesAsync()
            => GetValuesAsync(CancellationToken.None);

        private async Task<IReadOnlyCollection<TValue>> GetValuesAsync(CancellationToken cancellationToken)
        {
            var requestMessage = MapValuesCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.Messaging.SendAsync(requestMessage, cancellationToken).CfAwait();
            var response = MapValuesCodec.DecodeResponse(responseMessage).Response;
            return new ReadOnlyLazyList<TValue>(response, SerializationService);
        }

        /// <inheritdoc />
        public Task<IReadOnlyCollection<TValue>> GetValuesAsync(IPredicate predicate)
            => GetValuesAsync(predicate, CancellationToken.None);

        private async Task<IReadOnlyCollection<TValue>> GetValuesAsync(IPredicate predicate, CancellationToken cancellationToken)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            var pagingPredicate = UnwrapPagingPredicate(predicate);
            if (pagingPredicate != null)
            {
                pagingPredicate.IterationType = IterationType.Value;

                var pagingPredicateHolder = PagingPredicateHolder.Of(predicate, SerializationService);
                var requestMessage = MapValuesWithPagingPredicateCodec.EncodeRequest(Name, pagingPredicateHolder);
                var responseMessage = await Cluster.Messaging.SendAsync(requestMessage, cancellationToken).CfAwait();
                var response = MapValuesWithPagingPredicateCodec.DecodeResponse(responseMessage);
                pagingPredicate.UpdateAnchors(response.AnchorDataList.AsAnchorIterator(SerializationService));
                return new ReadOnlyLazyList<TValue>(response.Response, SerializationService);
            }

            {
                var requestMessage = MapValuesWithPredicateCodec.EncodeRequest(Name, ToData(predicate));
                var responseMessage = await (predicate is PartitionPredicate pp
                    ? Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, SerializationService.ToData(pp.PartitionKey), cancellationToken)
                    : Cluster.Messaging.SendAsync(requestMessage, cancellationToken))
                    .CfAwait();
                var response = MapValuesWithPredicateCodec.DecodeResponse(responseMessage).Response;
                return new ReadOnlyLazyList<TValue>(response, SerializationService);
            }
        }

        /// <inheritdoc />
        public Task<int> GetSizeAsync()
            => CountAsync(CancellationToken.None);

        private async Task<int> CountAsync(CancellationToken cancellationToken)
        {
            var requestMessage = MapSizeCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.Messaging.SendAsync(requestMessage, cancellationToken).CfAwait();
            var response = MapSizeCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public Task<bool> IsEmptyAsync()
            => IsEmptyAsync(CancellationToken.None);

        private async Task<bool> IsEmptyAsync(CancellationToken cancellationToken)
        {
            var requestMessage = MapIsEmptyCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.Messaging.SendAsync(requestMessage, cancellationToken).CfAwait();
            var response = MapIsEmptyCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public Task<bool> ContainsKeyAsync(TKey key)
            => ContainsKeyAsync(key, CancellationToken.None);

        private
#if !HZ_OPTIMIZE_ASYNC
        async
#endif
        Task<bool> ContainsKeyAsync(TKey key, CancellationToken cancellationToken)
        {
            var task = ContainsKeyAsync(ToSafeData(key), cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CfAwait();
#endif
        }

        /// <summary>
        /// Determines whether this map contains an entry for a key.
        /// </summary>
        /// <param name="keyData">The key.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>True if the map contains an entry for the specified key; otherwise false.</returns>
        protected virtual async Task<bool> ContainsKeyAsync(IData keyData, CancellationToken cancellationToken)
        {
            var requestMessage = MapContainsKeyCodec.EncodeRequest(Name, keyData, ContextId);
            var responseMessage = await Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, keyData, cancellationToken).CfAwait();
            var response = MapContainsKeyCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public Task<bool> ContainsValueAsync(TValue value)
            => ContainsValueAsync(value, CancellationToken.None);

        private async Task<bool> ContainsValueAsync(TValue value, CancellationToken cancellationToken)
        {
            var valueData = ToSafeData(value);

            var requestMessage = MapContainsValueCodec.EncodeRequest(Name, valueData);
            var responseMessage = await Cluster.Messaging.SendAsync(requestMessage, cancellationToken).CfAwait();
            var response = MapContainsValueCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        private static PagingPredicate UnwrapPagingPredicate(IPredicate predicate)
        {
            return predicate switch
            {
                PagingPredicate paging => paging,
                PartitionPredicate partition => partition.Target as PagingPredicate,
                _ => null
            };
        }

        public async IAsyncEnumerator<KeyValuePair<TKey, TValue>> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            // all collections are async enumerable,
            // but by default we load the whole items set at once,
            // then iterate in memory
            var items = await GetEntriesAsync(cancellationToken).CfAwait();
            foreach (var item in items)
                yield return item;
        }
    }
}
