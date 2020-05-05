using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Aggregators;
using Hazelcast.Clustering;
using Hazelcast.Configuration;
using Hazelcast.Core;
using Hazelcast.Core.Collections;
using Hazelcast.Data.Map;
using Hazelcast.Exceptions;
using Hazelcast.Messaging;
using Hazelcast.Predicates;
using Hazelcast.Projections;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Protocol.Data;
using Hazelcast.Serialization;

namespace Hazelcast.DistributedObjects.Implementation
{
    /// <summary>
    /// Implements <see cref="IMap{TKey,TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    internal class Map<TKey, TValue> : DistributedObjectBase, IMap<TKey, TValue>
    {
        private readonly ISequence<long> _lockReferenceIdSequence;

        /// <summary>
        /// Initializes a new instance of the <see cref="Map{TKey,TValue}"/> class.
        /// </summary>
        /// <param name="serviceName">the name of the service managing this object.</param>
        /// <param name="name">The unique name of the object.</param>
        /// <param name="cluster">A cluster.</param>
        /// <param name="serializationService">A serialization service.</param>
        /// <param name="lockReferenceIdSequence">A lock reference identifiers sequence.</param>
        public Map(string serviceName, string name, Cluster cluster, ISerializationService serializationService, ISequence<long> lockReferenceIdSequence)
            : base(serviceName, name, cluster, serializationService)
        {
            _lockReferenceIdSequence = lockReferenceIdSequence;
        }

        // TODO no timeout management or CancellationToken anywhere?!

        #region Setting

        /// <inheritdoc />
        public async Task<TValue> AddOrReplaceWithValueAsync(TKey key, TValue value)
            => await AddOrReplaceWithValueAsync(key, value, Timeout.InfiniteTimeSpan);

        /// <inheritdoc />
        public async Task AddOrReplaceAsync(TKey key, TValue value)
            => await AddOrReplaceAsync(key, value, Timeout.InfiniteTimeSpan);

        /// <inheritdoc />
        public async Task<TValue> AddOrReplaceWithValueAsync(TKey key, TValue value, TimeSpan timeToLive)
        {
            var (keyData, valueData) = ToSafeData(key, value);
            return await AddOrReplaceWithValueAsync(keyData, valueData, timeToLive);
        }

        /// <summary>
        /// Adds or replaces an entry with a time-to-live and returns the previous value.
        /// </summary>
        /// <param name="keyData">A key.</param>
        /// <param name="valueData">A value.</param>
        /// <param name="timeToLive">A time to live.</param>
        /// <returns>The value previously associated with the key in the map, if any; otherwise default(<typeparamref name="TValue"/>).</returns>
        /// <remarks>
        /// <para>The value is automatically expired, evicted and removed after the <paramref name="timeToLive"/> has elapsed..</para>
        /// <para>If the <paramref name="timeToLive"/> is <see cref="Timeout.InfiniteTimeSpan"/>, the entry lives forever.</para>
        /// </remarks>
        protected virtual async Task<TValue> AddOrReplaceWithValueAsync(IData keyData, IData valueData, TimeSpan timeToLive)
        {
            var timeToLiveMs = timeToLive.CodecMilliseconds(-1000);

            var requestMessage = MapPutCodec.EncodeRequest(Name, keyData, valueData, ThreadId, timeToLiveMs);
            var responseMessage = await Cluster.SendAsync(requestMessage, keyData);
            var response = MapPutCodec.DecodeResponse(responseMessage).Response;
            return ToObject<TValue>(response);
        }

        /// <inheritdoc />
        public async Task AddOrReplaceAsync(TKey key, TValue value, TimeSpan timeToLive)
        {
            var (keyData, valueData) = ToSafeData(key, value);
            await AddOrReplaceAsync(keyData, valueData, timeToLive);
        }

        /// <summary>
        /// Adds or replaces an entry with a time-to-live.
        /// </summary>
        /// <param name="keyData">A key.</param>
        /// <param name="valueData">A value.</param>
        /// <param name="timeToLive">A time to live.</param>
        /// <returns>Nothing.</returns>
        /// <remarks>
        /// <para>The value is automatically expired, evicted and removed after the <paramref name="timeToLive"/> has elapsed..</para>
        /// <para>If the <paramref name="timeToLive"/> is <see cref="Timeout.InfiniteTimeSpan"/>, the entry lives forever.</para>
        /// </remarks>
        protected virtual async Task AddOrReplaceAsync(IData keyData, IData valueData, TimeSpan timeToLive)
        {
            var timeToLiveMs = timeToLive.CodecMilliseconds(-1000);

            var requestMessage = MapSetCodec.EncodeRequest(Name, keyData, valueData, ThreadId, timeToLiveMs);
            await Cluster.SendAsync(requestMessage, keyData);
        }

        /// <inheritdoc />
        public async Task AddOrReplaceAsync(IDictionary<TKey, TValue> entries)
        {
            // TODO: is this transactional? can some entries be created and others be missing?

            var ownerEntries = new Dictionary<Guid, Dictionary<int, List<KeyValuePair<IData, IData>>>>();

            // verify entries + group by owner and partitions
            foreach (var (key, value) in entries)
            {
                var (keyData, valueData) = ToSafeData(key, value);

                var partitionId = Cluster.Partitioner.GetPartitionId(keyData);
                var ownerId = Cluster.Partitioner.GetPartitionOwner(partitionId);
                if (!ownerEntries.TryGetValue(ownerId, out var part))
                    part = ownerEntries[ownerId] = new Dictionary<int, List<KeyValuePair<IData, IData>>>();
                if (!part.TryGetValue(partitionId, out var list))
                    list = part[partitionId] = new List<KeyValuePair<IData, IData>>();
                list.Add(new KeyValuePair<IData, IData>(keyData, valueData));
            }

            await AddOrReplaceAsync(ownerEntries);
        }

        /// <summary>
        /// Adds or replaces entries.
        /// </summary>
        /// <param name="ownerEntries">Entries.</param>
        /// <returns>Nothing.</returns>
        protected virtual async Task AddOrReplaceAsync(Dictionary<Guid, Dictionary<int, List<KeyValuePair<IData, IData>>>> ownerEntries)
        {
            // TODO: add a SendAsync(...) to Cluster/Client
            // that can send multiple messages and use one single completion source
            // cannot inherit from TaskCompletionSource: it's not sealed but nothing is virtual

            // create parallel tasks to fire requests for each owner (each network client)
            // for each owner, serialize requests for each partition, because each message
            // needs to have its own partition id
            var tasks = new List<Task>();
            foreach (var (ownerId, part) in ownerEntries)
            {
                foreach (var (partitionId, list) in part)
                {
                    if (list.Count == 0) continue;

                    var requestMessage = MapPutAllCodec.EncodeRequest(Name, list);
                    requestMessage.PartitionId = partitionId;
                    var task = Cluster.SendAsync(requestMessage, ownerId).AsTask();
                    tasks.Add(task);
                }
            }

            // and wait on all tasks, ignoring the responses
            await Task.WhenAll(tasks);
        }

        /// <inheritdoc />
        public async Task<TValue> ReplaceAsync(TKey key, TValue newValue)
        {
            var (keyData, valueData) = ToSafeData(key, newValue);

            var requestMessage = MapReplaceCodec.EncodeRequest(Name, keyData, valueData, ThreadId);
            var responseMessage = await Cluster.SendAsync(requestMessage, keyData);
            var response = MapReplaceCodec.DecodeResponse(responseMessage).Response;
            return ToObject<TValue>(response);
        }

        /// <inheritdoc />
        public async Task<bool> ReplaceAsync(TKey key, TValue expectedValue, TValue newValue)
        {
            var (keyData, expectedData, newData) = ToSafeData(key, expectedValue, newValue);
            return await ReplaceAsync(keyData, expectedData, newData);
        }

        /// <summary>
        /// Replaces an existing entry.
        /// </summary>
        /// <param name="keyData">A key.</param>
        /// <param name="expectedData">The expected value.</param>
        /// <param name="newData">The new value.</param>
        /// <returns>true if the entry was replaced; otherwise false.</returns>
        protected async Task<bool> ReplaceAsync(IData keyData, IData expectedData, IData newData)
        {
            var requestMessage = MapReplaceIfSameCodec.EncodeRequest(Name, keyData, expectedData, newData, ThreadId);
            var responseMessage = await Cluster.SendAsync(requestMessage, keyData);
            var response = MapReplaceIfSameCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public async Task<bool> TryAddOrReplaceAsync(TKey key, TValue value, TimeSpan timeout)
        {
            var (keyData, valueData) = ToSafeData(key, value);
            return await TryAddOrReplaceAsync(keyData, valueData, timeout);
        }

        /// <summary>
        /// Tries to set an entry within a timeout.
        /// </summary>
        /// <param name="keyData">A key.</param>
        /// <param name="valueData">A value.</param>
        /// <param name="timeout">A timeout.</param>
        /// <returns>true if the entry was set; otherwise false.</returns>
        /// <remarks>
        /// <para>This method returns false when no lock on the key could be
        /// acquired within the timeout.</para>
        /// </remarks>
        protected virtual async Task<bool> TryAddOrReplaceAsync(IData keyData, IData valueData, TimeSpan timeout)
        {
            var timeoutMs = timeout.CodecMilliseconds(0);

            var requestMessage = MapTryPutCodec.EncodeRequest(Name, keyData, valueData, ThreadId, timeoutMs);
            var responseMessage = await Cluster.SendAsync(requestMessage, keyData);
            var response = MapTryPutCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public async Task<TValue> AddIfMissingAsync(TKey key, TValue value)
            => await AddIfMissingAsync(key, value, Timeout.InfiniteTimeSpan);

        /// <inheritdoc />
        public async Task<TValue> AddIfMissingAsync(TKey key, TValue value, TimeSpan timeToLive)
        {
            var (keyData, valueData) = ToSafeData(key, value);
            return await AddIfMissingAsync(keyData, valueData, timeToLive);
        }

        /// <summary>
        /// Adds an entry with a time-to-live, if no entry with the key exists.
        /// </summary>
        /// <param name="keyData">A key.</param>
        /// <param name="valueData">The value.</param>
        /// <param name="timeToLive">A time to live.</param>
        /// <returns>The existing value, if any; otherwise the default value.</returns>
        /// <remarks>
        /// <para>The value is automatically expired, evicted and removed after the <paramref name="timeToLive"/> has elapsed..</para>
        /// <para>If the <paramref name="timeToLive"/> is <see cref="Timeout.InfiniteTimeSpan"/>, the entry lives forever.</para>
        /// </remarks>
        protected virtual async Task<TValue> AddIfMissingAsync(IData keyData, IData valueData, TimeSpan timeToLive)
        {
            var timeToLiveMs = timeToLive.CodecMilliseconds(-1000);

            var requestMessage = MapPutIfAbsentCodec.EncodeRequest(Name, keyData, valueData, ThreadId, timeToLiveMs);
            var responseMessage = await Cluster.SendAsync(requestMessage, keyData);
            var response = MapPutIfAbsentCodec.DecodeResponse(responseMessage).Response;
            return ToObject<TValue>(response);
        }

        /// <inheritdoc />
        public async Task AddTransientAsync(TKey key, TValue value, TimeSpan timeToLive)
        {
            var (keyData, valueData) = ToSafeData(key, value);
            await AddTransientAsync(keyData, valueData, timeToLive);
        }

        /// TODO: document?!
        protected virtual async Task AddTransientAsync(IData keyData, IData valueData, TimeSpan timeToLive)
        {
            var timeToLiveMs = timeToLive.CodecMilliseconds(-1000);

            // FIXME uh?
            var requestMessage = MapPutTransientCodec.EncodeRequest(Name, keyData, valueData, ThreadId, timeToLiveMs);
            var responseMessage = await Cluster.SendAsync(requestMessage, keyData);
        }

        #endregion

        #region Getting

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
            var responseMessage = await Cluster.SendAsync(requestMessage, keyData);
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
                    var task = Cluster.SendAsync(requestMessage, ownerId).AsTask();
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
            var responseMessage = await Cluster.SendAsync(requestMessage, keyData);
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
                    ? Cluster.SendAsync(requestMessage, SerializationService.ToData(pp.GetPartitionKey()))
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
                    ? Cluster.SendAsync(requestMessage, SerializationService.ToData(pp.GetPartitionKey()))
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
                    ? Cluster.SendAsync(requestMessage, SerializationService.ToData(pp.GetPartitionKey()))
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
            var responseMessage = await Cluster.SendAsync(requestMessage, keyData);
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

        #endregion

        #region Removing

        /// <inheritdoc />
        public async Task<bool> TryRemoveAsync(TKey key, TimeSpan timeout)
            => await TryRemoveAsync(ToSafeData(key), timeout);

        /// <summary>
        /// Tries to remove an entry from the map within a timeout.
        /// </summary>
        /// <param name="keyData">A key.</param>
        /// <param name="timeout">A timeout.</param>
        /// <returns>true if the entry was removed; otherwise false.</returns>
        /// <remarks>
        /// <para>This method returns false when no lock on the key could be
        /// acquired within the timeout.</para>
        /// TODO or when there was no value with that key?
        /// </remarks>
        protected virtual async Task<bool> TryRemoveAsync(IData keyData, TimeSpan timeout)
        {
            var timeoutMs = timeout.CodecMilliseconds(0);

            var requestMessage = MapTryRemoveCodec.EncodeRequest(Name, keyData, ThreadId, timeoutMs);
            var responseMessage = await Cluster.SendAsync(requestMessage, keyData);
            var response = MapTryRemoveCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public async Task<TValue> RemoveAsync(TKey key)
            => await RemoveAsync(ToSafeData(key));

        /// <summary>
        /// Removes an entry from this map, and returns the corresponding value if any.
        /// </summary>
        /// <param name="keyData">The key.</param>
        /// <returns>The value, if any, or default(TValue).</returns>
        protected virtual async Task<TValue> RemoveAsync(IData keyData)
        {
            var requestMessage = MapRemoveCodec.EncodeRequest(Name, keyData, ThreadId);
            var responseMessage = await Cluster.SendAsync(requestMessage, keyData);
            var response = MapRemoveCodec.DecodeResponse(responseMessage).Response;
            return ToObject<TValue>(response);
        }

        /// <inheritdoc />
        public async Task<bool> RemoveAsync(TKey key, TValue value)
        {
            var (keyData, valueData) = ToSafeData(key, value);
            return await RemoveAsync(keyData, valueData);
        }

        /// <summary>
        /// Removes an entry from this map.
        /// </summary>
        /// <param name="keyData">The key.</param>
        /// <param name="valueData">The value.</param>
        /// <returns>The value, if any, or default(TValue).</returns>
        /// <remarks>
        /// <para>This method removes an entry if the key and the value both match the
        /// specified key and value.</para>
        /// </remarks>
        protected virtual async Task<bool> RemoveAsync(IData keyData, IData valueData)
        {
            var requestMessage = MapRemoveIfSameCodec.EncodeRequest(Name, keyData, valueData, ThreadId);
            var responseMessage = await Cluster.SendAsync(requestMessage, keyData);
            var response = MapRemoveIfSameCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public async Task DeleteAsync(TKey key)
            => await DeleteAsync(ToSafeData(key));

        /// <summary>
        /// Removes an entry from this map.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <remarks>
        /// <para>For performance reasons, this method does not return the value. Prefer
        /// <see cref="RemoveAsync(TKey)"/> if the value is required.</para>
        /// </remarks>
        protected virtual async Task DeleteAsync(IData keyData)
        {
            var requestMessage = MapDeleteCodec.EncodeRequest(Name, keyData, ThreadId);
            await Cluster.SendAsync(requestMessage, keyData);
        }

        /// <inheritdoc />
        public virtual async Task ClearAsync()
        {
            var requestMessage = MapClearCodec.EncodeRequest(Name);
            await Cluster.SendAsync(requestMessage);
        }

        #endregion

        #region Caching

        /// <inheritdoc />
        public async Task<bool> EvictAsync(TKey key)
        {
            var keyData = ToSafeData(key);

            var requestMessage = MapEvictCodec.EncodeRequest(Name, keyData, ThreadId);
            var responseMessage = await Cluster.SendAsync(requestMessage, keyData);
            var response = MapEvictCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public async Task EvictAllAsync()
        {
            var requestMessage = MapEvictAllCodec.EncodeRequest(Name);
            await Cluster.SendAsync(requestMessage);
        }

        /// <inheritdoc />
        public async Task FlushAsync()
        {
            var requestMessage = MapFlushCodec.EncodeRequest(Name);
            await Cluster.SendAsync(requestMessage);
        }

        #endregion

        #region Processing

        /// <inheritdoc />
        public async Task<object> ExecuteAsync(IEntryProcessor processor, TKey key)
        {
            var (keyData, processorData) = ToSafeData(key, processor);
            return await ExecuteAsync(keyData, processorData);
        }

        /// <summary>
        /// Processes an entry.
        /// </summary>
        /// <param name="keyData">The key.</param>
        /// <param name="processorData">An entry processor.</param>
        /// <returns>The result of the process.</returns>
        /// <remarks>
        /// <para>The <paramref name="processorData"/> must have a counterpart on the server.</para>
        /// </remarks>
        protected virtual async Task<object> ExecuteAsync(IData processorData, IData keyData)
        {
            var requestMessage = MapExecuteOnKeyCodec.EncodeRequest(Name, processorData, keyData, ThreadId);
            var responseMessage = await Cluster.SendAsync(requestMessage, keyData);
            var response = MapExecuteOnKeyCodec.DecodeResponse(responseMessage).Response;
            return ToObject<object>(response);
        }

        /// <inheritdoc />
        public async Task<IDictionary<TKey, object>> ExecuteAsync(IEntryProcessor processor, IEnumerable<TKey> keys)
        {
            if (keys == null) throw new ArgumentNullException(nameof(keys));

            var keysmap = keys.ToDictionary(x => ToSafeData(x), x => x);
            if (keysmap.Count == 0) return new Dictionary<TKey, object>();
            var processorData = ToSafeData(processor);

            var requestMessage = MapExecuteOnKeysCodec.EncodeRequest(Name, processorData, keysmap.Keys);
            var responseMessage = await Cluster.SendAsync(requestMessage);
            var response = MapExecuteOnKeysCodec.DecodeResponse(responseMessage).Response;

            var result = new Dictionary<TKey, object>();
            foreach (var (keyData, valueData) in response)
            {
                if (!keysmap.TryGetValue(keyData, out var key))
                    throw new InvalidOperationException("Server returned an unexpected key.");
                result[key] = ToObject<object>(valueData);
            }
            return result;
        }

        /// <inheritdoc />
        public async Task<IDictionary<TKey, object>> ExecuteAsync(IEntryProcessor processor)
        {
            var processorData = ToSafeData(processor);

            var requestMessage = MapExecuteOnAllKeysCodec.EncodeRequest(Name, processorData);
            var responseMessage = await Cluster.SendAsync(requestMessage);
            var response = MapExecuteOnAllKeysCodec.DecodeResponse(responseMessage).Response;

            var result = new Dictionary<TKey, object>();
            foreach (var (keyData, valueData) in response)
                result[ToObject<TKey>(keyData)] = ToObject<object>(valueData);
            return result;

        }

        /// <inheritdoc />
        public async Task<object> ApplyAsync(IEntryProcessor processor, TKey key)
        {
            var (keyData, processorData) = ToSafeData(key, processor);
            return await ApplyAsync(processorData, keyData);
        }

        // FIXME: do we want this?
        protected virtual async Task<object> ApplyAsync(IData processorData, IData keyData)
        {
            var requestMessage = MapSubmitToKeyCodec.EncodeRequest(Name, processorData, keyData, ThreadId);
            var responseMessage = await Cluster.SendAsync(requestMessage, keyData);
            var response = MapSubmitToKeyCodec.DecodeResponse(responseMessage).Response;
            return ToObject<object>(response);
        }

        #endregion

        #region Locking

        /// <inheritdoc />
        public async Task LockAsync(TKey key)
            => await LockAsync(key, Timeout.InfiniteTimeSpan);

        /// <inheritdoc />
        public async Task LockAsync(TKey key, TimeSpan leaseTime)
            => await TryLockAsync(key, leaseTime, Timeout.InfiniteTimeSpan);

        /// <inheritdoc />
        public async Task<bool> TryLockAsync(TKey key)
            => await TryLockAsync(key, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

        /// <inheritdoc />
        public async Task<bool> TryLockAsync(TKey key, TimeSpan timeout)
            => await TryLockAsync(key, Timeout.InfiniteTimeSpan, timeout);

        /// <inheritdoc />
        public async Task<bool> TryLockAsync(TKey key, TimeSpan leaseTime, TimeSpan timeout)
        {
            var keyData = ToSafeData(key);

            var refId = _lockReferenceIdSequence.Next;
            var leaseTimeMs = leaseTime.CodecMilliseconds(long.MaxValue);
            var timeoutMs = timeout.CodecMilliseconds(0);

            var requestMessage = MapTryLockCodec.EncodeRequest(Name, keyData, ThreadId, leaseTimeMs, timeoutMs, refId);
            var responseMessage = await Cluster.SendAsync(requestMessage, keyData);
            var response = MapTryLockCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public async Task<bool> IsLockedAsync(TKey key)
        {
            var keyData = ToSafeData(key);

            var requestMessage = MapIsLockedCodec.EncodeRequest(Name, keyData);
            var responseMessage = await Cluster.SendAsync(requestMessage, keyData);
            var response = MapIsLockedCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public async Task UnlockAsync(TKey key)
        {
            var keyData = ToSafeData(key);

            var refId = _lockReferenceIdSequence.Next;

            var requestMessage = MapUnlockCodec.EncodeRequest(Name, keyData, ThreadId, refId);
            await Cluster.SendAsync(requestMessage, keyData);
        }

        /// <inheritdoc />
        public async Task ForceUnlockAsync(TKey key)
        {
            var keyData = ToSafeData(key);

            var refId = _lockReferenceIdSequence.Next;

            var requestMessage = MapForceUnlockCodec.EncodeRequest(Name, keyData, refId);
            await Cluster.SendAsync(requestMessage, keyData);
        }

        #endregion

        #region Indexing

        /// <inheritdoc />
        public async Task AddIndexAsync(IndexType type, params string[] attributes)
        {
            if (attributes == null) throw new ArgumentNullException(nameof(attributes));
            await AddIndexAsync(new IndexConfig { Type = type, Attributes = attributes });
        }

        /// <inheritdoc />
        public async Task AddIndexAsync(IndexConfig indexConfig)
        {
            if (indexConfig == null) throw new ArgumentNullException(nameof(indexConfig));

            var requestMessage = MapAddIndexCodec.EncodeRequest(Name, indexConfig.ValidateAndNormalize(Name));
            await Cluster.SendAsync(requestMessage);
        }

        #endregion

        #region Aggregating and Projecting

        /// <inheritdoc />
        public async Task<TResult> AggregateAsync<TResult>(IAggregator<TResult> aggregator, IPredicate predicate = null)
        {
            var aggregatorData = ToSafeData(aggregator);

            if (predicate == null)
            {
                var requestMessage = MapAggregateCodec.EncodeRequest(Name, aggregatorData);
                var responseMessage = await Cluster.SendAsync(requestMessage);
                var response = MapAggregateCodec.DecodeResponse(responseMessage).Response;
                return ToObject<TResult>(response);
            }

            {
                var predicateData = ToData(predicate);

                var requestMessage = MapAggregateWithPredicateCodec.EncodeRequest(Name, aggregatorData, predicateData);
                var responseMessage = await Cluster.SendAsync(requestMessage);
                var response = MapAggregateWithPredicateCodec.DecodeResponse(responseMessage).Response;
                return ToObject<TResult>(response);
            }
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<TResult>> ProjectAsync<TResult>(IProjection projection, IPredicate predicate = null)
        {
            var projectionData = ToSafeData(projection);

            if (predicate == null)
            {
                var requestMessage = MapProjectCodec.EncodeRequest(Name, projectionData);
                var responseMessage = await Cluster.SendAsync(requestMessage);
                var response = MapProjectCodec.DecodeResponse(responseMessage).Response;
                return new ReadOnlyLazyList<TResult>(response, SerializationService);
            }

            {
                var predicateData = ToData(predicate);

                var requestMessage = MapProjectWithPredicateCodec.EncodeRequest(Name, projectionData, predicateData);
                var responseMessage = await Cluster.SendAsync(requestMessage);
                var response = MapProjectWithPredicateCodec.DecodeResponse(responseMessage).Response;
                return new ReadOnlyLazyList<TResult>(response, SerializationService);
            }
        }

        #endregion

        #region Intercepting

        /// <inheritdoc />
        public async Task<string> AddInterceptorAsync(IMapInterceptor interceptor)
        {
            var interceptorData = ToSafeData(interceptor);

            var requestMessage = MapAddInterceptorCodec.EncodeRequest(Name, interceptorData);
            var responseMessage = await Cluster.SendAsync(requestMessage);
            var response = MapAddInterceptorCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public async Task RemoveInterceptorAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(id));

            var requestMessage = MapRemoveInterceptorCodec.EncodeRequest(Name, id);
            await Cluster.SendAsync(requestMessage);
        }

        #endregion

        #region Events

        // TODO: could any of these events be ASYNC?!

        // TODO: group all event-related in MapEntryAddedEvent.cs file

        private async Task<Guid> SubscribeAsync(bool includeValues, IPredicate predicate, bool hasPredicate, TKey key, bool hasKey, Action<MapEvents<TKey, TValue>> on)
        {
            if (hasKey && key == null) throw new ArgumentNullException(nameof(key));
            if (hasPredicate && predicate == null) throw new ArgumentNullException(nameof(predicate));

            var subscriber = new MapEvents<TKey, TValue>();
            on(subscriber);

            var flags = MapEventType.Nothing;
            foreach (var handler in subscriber.Handlers)
                flags |= handler.EventType;

            // FIXME wtf
            var localOnly = false;

            // 0: no entryKey, no predicate
            // 1: entryKey, no predicate
            // 2: no entryKey, predicate
            // 3: entryKey, predicate
            var mode = (hasKey ? 1 : 0) + (hasPredicate ? 2 : 0);

            ClientMessage subscribeRequest;
            switch (mode)
            {
                case 0:
                    subscribeRequest = MapAddEntryListenerCodec.EncodeRequest(Name, includeValues, (int)flags, localOnly);
                    break;
                case 1:
                    subscribeRequest = MapAddEntryListenerToKeyCodec.EncodeRequest(Name, ToData(key), includeValues, (int)flags, localOnly);
                    break;
                case 2:
                    subscribeRequest = MapAddEntryListenerWithPredicateCodec.EncodeRequest(Name, ToData(predicate), includeValues, (int)flags, localOnly);
                    break;
                case 3:
                    subscribeRequest = MapAddEntryListenerToKeyWithPredicateCodec.EncodeRequest(Name, ToData(key), ToData(predicate), includeValues, (int)flags, localOnly);
                    break;
                default:
                    throw new Exception();
            }

            var subscription = new ClusterEventSubscription(
                subscribeRequest,
                HandleSubscribeResponse,
                CreateUnsubscribeRequest,
                HandleEvent,
                new SubscriptionState(mode, Name, subscriber.Handlers));

            // the cluster stores the subscription it unsubscribes
            await Cluster.SubscribeAsync(subscription);

            return subscription.Id;
        }

        public Task<Guid> SubscribeAsync(Action<MapEvents<TKey, TValue>> on)
            => SubscribeAsync(true, default, false, default, false, on);

        public Task<Guid> SubscribeAsync(bool includeValues, Action<MapEvents<TKey, TValue>> on)
            => SubscribeAsync(includeValues, default, false, default, false, on);

        public Task<Guid> SubscribeAsync(TKey key, Action<MapEvents<TKey, TValue>> on)
            => SubscribeAsync(true, default, false, key, true, on);

        public Task<Guid> SubscribeAsync(bool includeValues, TKey key, Action<MapEvents<TKey, TValue>> on)
            => SubscribeAsync(includeValues, default, false, key, true, on);

        public Task<Guid> SubscribeAsync(IPredicate predicate, Action<MapEvents<TKey, TValue>> on)
            => SubscribeAsync(true, predicate, true, default, false, on);

        public Task<Guid> SubscribeAsync(bool includeValues, IPredicate predicate, Action<MapEvents<TKey, TValue>> on)
            => SubscribeAsync(includeValues, predicate, true, default, false, on);

        public Task<Guid> SubscribeAsync(TKey key, IPredicate predicate, Action<MapEvents<TKey, TValue>> on)
            => SubscribeAsync(true, predicate, true, key, true, on);

        public Task<Guid> SubscribeAsync(bool includeValues, TKey key, IPredicate predicate, Action<MapEvents<TKey, TValue>> on)
            => SubscribeAsync(includeValues, predicate, true, key, true, on);


        private class SubscriptionState
        {
            public SubscriptionState(int mode, string name, List<IMapEventHandlerBase<TKey, TValue>> handlers)
            {
                Mode = mode;
                Name = name;
                Handlers = handlers;
            }

            public int Mode { get; }

            public string Name { get;}

            public List<IMapEventHandlerBase<TKey, TValue>> Handlers { get; }
        }

        private static SubscriptionState ToSafeState(object state)
        {
            if (state is SubscriptionState sstate) return sstate;
            throw new Exception();
        }

        void HandleEvent(ClientMessage eventMessage, object state)
        {
            var sstate = ToSafeState(state);

            void HandleEntryEvent(IData keyData, IData valueData, IData oldValueData, IData mergingValueData, int eventTypeData, Guid memberId, int numberOfAffectedEntries)
            {
                var eventType = (MapEventType)eventTypeData;
                if (eventType == MapEventType.Nothing) return;

                Lazy<T> LazyArg<T>(IData source) => source == null ? null : new Lazy<T>(() => ToObject<T>(source));

                var member = Cluster.GetMember(memberId);

                // TODO: could this be optimized?
                var key = LazyArg<TKey>(keyData);
                var value = LazyArg<TValue>(valueData);
                var oldValue = LazyArg<TValue>(oldValueData);
                var mergingValue = LazyArg<TValue>(mergingValueData);

                // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                foreach (var handler in sstate.Handlers)
                {
                    if (handler.EventType.HasFlag(eventType)) // FIXME has any or...
                    {
                        switch (handler)
                        {
                            case IMapEntryEventHandler<TKey, TValue> entryHandler:
                                entryHandler.Handle(this, member, key, value, oldValue, mergingValue, eventType, numberOfAffectedEntries);
                                break;
                            case IMapEventHandler<TKey, TValue> mapHandler:
                                mapHandler.Handle(this, member, numberOfAffectedEntries);
                                break;
                            default:
                                throw new NotSupportedException();
                        }
                    }
                }
            }

            switch (sstate.Mode)
            {
                case 0:
                    MapAddEntryListenerCodec.EventHandler.HandleEvent(eventMessage, HandleEntryEvent);
                    break;
                case 1:
                    MapAddEntryListenerToKeyCodec.EventHandler.HandleEvent(eventMessage, HandleEntryEvent);
                    break;
                case 2:
                    MapAddEntryListenerWithPredicateCodec.EventHandler.HandleEvent(eventMessage, HandleEntryEvent);
                    break;
                case 3:
                    MapAddEntryListenerToKeyWithPredicateCodec.EventHandler.HandleEvent(eventMessage, HandleEntryEvent);
                    break;
                default:
                    throw new Exception();
            }
        }

        private static ClientMessage CreateUnsubscribeRequest(Guid subscriptionId, object state)
        {
            var sstate = ToSafeState(state);
            return MapRemoveEntryListenerCodec.EncodeRequest(sstate.Name, subscriptionId);
        }

        private static Guid HandleSubscribeResponse(ClientMessage responseMessage, object state)
        {
            var sstate = ToSafeState(state);

            switch (sstate.Mode)
            {
                case 0:
                    return MapAddEntryListenerCodec.DecodeResponse(responseMessage).Response;
                case 1:
                    return MapAddEntryListenerToKeyCodec.DecodeResponse(responseMessage).Response;
                case 2:
                    return MapAddEntryListenerWithPredicateCodec.DecodeResponse(responseMessage).Response;
                case 3:
                    return MapAddEntryListenerToKeyWithPredicateCodec.DecodeResponse(responseMessage).Response;
                default:
                    throw new Exception();
            }
        }

        /// <inheritdoc />
        public async Task<bool> UnsubscribeAsync(Guid subscriptionId)
        {
            // FIXME why would it return a bool?
            await Cluster.UnsubscribeAsync(subscriptionId);
            return true;
        }

        #endregion
    }
}
