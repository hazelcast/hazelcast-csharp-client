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
        /// <summary>
        /// Initializes a new instance of the <see cref="Map{TKey,TValue}"/> class.
        /// </summary>
        /// <param name="serviceName">the name of the service managing this object.</param>
        /// <param name="name">The unique name of the object.</param>
        /// <param name="cluster">A cluster.</param>
        /// <param name="serializationService">A serialization service.</param>
        public Map(string serviceName, string name, Cluster cluster, ISerializationService serializationService)
            : base(serviceName, name, cluster, serializationService)
        { }

        // TODO no timeout management or CancellationToken anywhere?!

        #region Setting

        /// <inheritdoc />
        public async Task<TValue> AddAsync(TKey key, TValue value)
            => await AddAsync(key, value, Timeout.InfiniteTimeSpan);

        /// <inheritdoc />
        public async Task<TValue> AddAsync(TKey key, TValue value, TimeSpan timeToLive)
        {
            var (keyData, valueData) = ToSafeData(key, value);

            var requestMessage = MapPutCodec.EncodeRequest(Name, keyData, valueData, ThreadId, (long) timeToLive.TotalMilliseconds);
            var responseMessage = await Cluster.SendAsync(requestMessage, keyData);
            var response = MapPutCodec.DecodeResponse(responseMessage).Response;
            return ToObject<TValue>(response);
        }

        /// <inheritdoc />
        public async Task AddAsync(IDictionary<TKey, TValue> entries)
        {
            // TODO: is this transactional? can some entries be created and others be missing?

            var ownerEntries = new Dictionary<Guid, List<KeyValuePair<IData, IData>>>();

            // verify entries + group by owner
            foreach (var (key, value) in entries)
            {
                var (keyData, valueData) = ToSafeData(key, value);

                var ownerId = Cluster.Partitioner.GetPartitionOwner(keyData);
                if (!ownerEntries.TryGetValue(ownerId, out var list))
                    list = ownerEntries[ownerId] = new List<KeyValuePair<IData, IData>>();
                list.Add(new KeyValuePair<IData, IData>(keyData, valueData));
            }

            // create parallel tasks to fire a request for each owner
            var tasks = new List<Task>();
            foreach (var (ownerId, list) in ownerEntries)
            {
                if (list.Count == 0) continue;

                var requestMessage = MapPutAllCodec.EncodeRequest(Name, list);
                var task = Cluster.SendAsync(requestMessage, ownerId).AsTask();
                task.Start();
                tasks.Add(task);
            }

            // and wait on all tasks, ignoring the responses
            await Task.WhenAll(tasks);
        }

        /// <inheritdoc />
        public async Task<TValue> AddIfMissingAsync(TKey key, TValue value)
            => await AddIfMissingAsync(key, value, Timeout.InfiniteTimeSpan);

        /// <inheritdoc />
        public async Task<TValue> AddIfMissingAsync(TKey key, TValue value, TimeSpan timeToLive)
        {
            var (keyData, valueData) = ToSafeData(key, value);

            var requestMessage = MapPutIfAbsentCodec.EncodeRequest(Name, keyData, valueData, ThreadId, (long) timeToLive.TotalMilliseconds);
            var responseMessage = await Cluster.SendAsync(requestMessage, keyData);
            var response = MapPutIfAbsentCodec.DecodeResponse(responseMessage).Response;
            return ToObject<TValue>(response);
        }

        /// <inheritdoc />
        public async Task AddTransientAsync(TKey key, TValue value, TimeSpan timeToLive)
        {
            var (keyData, valueData) = ToSafeData(key, value);

            // FIXME uh?
            var requestMessage = MapPutTransientCodec.EncodeRequest(Name, keyData, valueData, ThreadId, (long) timeToLive.TotalMilliseconds);
            var responseMessage = await Cluster.SendAsync(requestMessage, keyData);
        }

        /// <inheritdoc />
        public async Task<TValue> ReplaceAsync(TKey key, TValue value)
        {
            var (keyData, valueData) = ToSafeData(key, value);

            var requestMessage = MapReplaceCodec.EncodeRequest(Name, keyData, valueData, ThreadId);
            var responseMessage = await Cluster.SendAsync(requestMessage, keyData);
            var response = MapReplaceCodec.DecodeResponse(responseMessage).Response;
            return ToObject<TValue>(response);
        }

        /// <inheritdoc />
        public async Task<bool> ReplaceAsync(TKey key, TValue expectedValue, TValue newValue)
        {
            var (keyData, expectedData, newData) = ToSafeData(key, expectedValue, newValue);

            var requestMessage = MapReplaceIfSameCodec.EncodeRequest(Name, keyData, expectedData, newData, ThreadId);
            var responseMessage = await Cluster.SendAsync(requestMessage, keyData);
            var response = MapReplaceIfSameCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public async Task SetAsync(TKey key, TValue value)
            => await SetAsync(key, value, Timeout.InfiniteTimeSpan);

        /// <inheritdoc />
        public async Task SetAsync(TKey key, TValue value, TimeSpan timeToLive)
        {
            var (keyData, valueData) = ToSafeData(key, value);

            var requestMessage = MapSetCodec.EncodeRequest(Name, keyData, valueData, ThreadId, (long)timeToLive.TotalMilliseconds);
            await Cluster.SendAsync(requestMessage, keyData);
        }

        /// <inheritdoc />
        public async Task<bool> TryPutAsync(TKey key, TValue value, TimeSpan timeout)
        {
            var (keyData, valueData) = ToSafeData(key, value);

            var requestMessage = MapTryPutCodec.EncodeRequest(Name, keyData, valueData, ThreadId, (int) timeout.TotalMilliseconds);
            var responseMessage = await Cluster.SendAsync(requestMessage, keyData);
            var response = MapTryPutCodec.DecodeResponse(responseMessage).Response;
            return response;

        }

        #endregion

        #region Getting

        /// <inheritdoc />
        public async Task<TValue> GetAsync(TKey key)
        {
            var keyData = ToSafeData(key);

            var requestMessage = MapGetCodec.EncodeRequest(Name, keyData, ThreadId);
            var responseMessage = await Cluster.SendAsync(requestMessage, keyData);
            var response = MapGetCodec.DecodeResponse(responseMessage).Response;
            return ToObject<TValue>(response);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyDictionary<TKey, TValue>> GetAsync(ICollection<TKey> keys)
        {
            var ownerKeys = new Dictionary<Guid, List<IData>>();

            // verify keys + group by owner
            foreach (var key in keys)
            {
                var keyData = ToSafeData(key);

                var ownerId = Cluster.Partitioner.GetPartitionOwner(keyData);
                if (!ownerKeys.TryGetValue(ownerId, out var list))
                    list = ownerKeys[ownerId] = new List<IData>();
                list.Add(keyData);
            }

            // create parallel tasks to fire a request for each owner
            var tasks = new List<Task<ClientMessage>>();
            foreach (var (ownerId, list) in ownerKeys)
            {
                if (list.Count == 0) continue;

                var requestMessage = MapGetAllCodec.EncodeRequest(Name, list);
                var task = Cluster.SendAsync(requestMessage, ownerId).AsTask();
                task.Start();
                tasks.Add(task);
            }

            // and wait on all tasks, gathering the responses
            await Task.WhenAll(tasks);

            // decode all responses, in 1 thread: this is CPU-bound
            // (we may want to introduce some parallelism, though, depending on # of cores)
            var result = new ReadOnlyLazyDictionary<TKey, TValue, IData>(SerializationService);
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
        public async Task<IEntryView<TKey, TValue>> GetEntryViewAsync(TKey key)
        {
            var keyData = ToSafeData(key);

            var requestMessage = MapGetEntryViewCodec.EncodeRequest(Name, keyData, ThreadId);
            var responseMessage = await Cluster.SendAsync(requestMessage, keyData);
            var response = MapGetEntryViewCodec.DecodeResponse(responseMessage).Response;

            if (response == null) return null;

            return new SimpleEntryView<TKey, TValue>
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

        private static PagingPredicate UnwrapPagingPredicate(IPredicate predicate)
            => predicate as PagingPredicate ?? (predicate as PartitionPredicate)?.GetTarget() as PagingPredicate;

        /// <inheritdoc />
        public async Task<IReadOnlyDictionary<TKey, TValue>> GetAsync(IPredicate predicate = null)
        {
            if (predicate == null)
            {
                var requestMessage = MapEntrySetCodec.EncodeRequest(Name);
                var responseMessage = await Cluster.SendAsync(requestMessage);
                var response = MapEntrySetCodec.DecodeResponse(responseMessage).Response;
                return new ReadOnlyLazyDictionary<TKey, TValue, IData>(SerializationService) { response };
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
                return new ReadOnlyLazyDictionary<TKey, TValue, IData>(SerializationService) { response.Response };
            }

            {
                var requestMessage = MapEntriesWithPredicateCodec.EncodeRequest(Name, ToData(predicate));
                var responseMessage = await (predicate is PartitionPredicate pp
                    ? Cluster.SendAsync(requestMessage, pp.GetPartitionKey())
                    : Cluster.SendAsync(requestMessage));
                var response = MapEntriesWithPredicateCodec.DecodeResponse(responseMessage).Response;
                return new ReadOnlyLazyDictionary<TKey, TValue, IData>(SerializationService) { response };
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
                return new ReadOnlyLazyList<TKey, IData>(response, SerializationService);
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
                return new ReadOnlyLazyList<TKey, IData>(response.Response, SerializationService);
            }

            {
                var requestMessage = MapKeySetWithPredicateCodec.EncodeRequest(Name, ToData(predicate));
                var responseMessage = await (predicate is PartitionPredicate pp
                    ? Cluster.SendAsync(requestMessage, pp.GetPartitionKey())
                    : Cluster.SendAsync(requestMessage));
                var response = MapKeySetWithPredicateCodec.DecodeResponse(responseMessage).Response;
                return new ReadOnlyLazyList<TKey, IData>(response, SerializationService);
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
                return new ReadOnlyLazyList<TValue, IData>(response, SerializationService);
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
                return new ReadOnlyLazyList<TValue, IData>(response.Response, SerializationService);
            }

            {
                var requestMessage = MapValuesWithPredicateCodec.EncodeRequest(Name, ToData(predicate));
                var responseMessage = await (predicate is PartitionPredicate pp
                    ? Cluster.SendAsync(requestMessage, pp.GetPartitionKey())
                    : Cluster.SendAsync(requestMessage));
                var response = MapValuesWithPredicateCodec.DecodeResponse(responseMessage).Response;
                return new ReadOnlyLazyList<TValue, IData>(response, SerializationService);
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
        {
            var keyData = ToSafeData(key);

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

        #endregion

        #region Removing

        /// <inheritdoc />
        public async Task<bool> TryRemoveAsync(TKey key, TimeSpan timeout)
        {
            var keyData = ToSafeData(key);

            var requestMessage = MapTryRemoveCodec.EncodeRequest(Name, keyData, ThreadId, (int) timeout.TotalMilliseconds);
            var responseMessage = await Cluster.SendAsync(requestMessage, keyData);
            var response = MapTryRemoveCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public async Task<TValue> RemoveAsync(TKey key)
        {
            var keyData = ToSafeData(key);

            var requestMessage = MapRemoveCodec.EncodeRequest(Name, keyData, ThreadId);
            var responseMessage = await Cluster.SendAsync(requestMessage, keyData);
            var response = MapRemoveCodec.DecodeResponse(responseMessage).Response;
            return ToObject<TValue>(response);
        }

        /// <inheritdoc />
        public async Task<bool> RemoveAsync(TKey key, TValue value)
        {
            var (keyData, valueData) = ToSafeData(key, value);

            var requestMessage = MapRemoveIfSameCodec.EncodeRequest(Name, keyData, valueData, ThreadId);
            var responseMessage = await Cluster.SendAsync(requestMessage, keyData);
            var response = MapRemoveIfSameCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public async Task DeleteAsync(TKey key)
        {
            var keyData = ToSafeData(key);

            var requestMessage = MapDeleteCodec.EncodeRequest(Name, keyData, ThreadId);
            await Cluster.SendAsync(requestMessage, keyData);
        }

        /// <inheritdoc />
        public async Task ClearAsync()
        {
            var requestMessage = MapClearCodec.EncodeRequest(Name);
            await Cluster.SendAsync(requestMessage);
        }

        #endregion

        #region Caching

        public bool Evict(TKey key)
        {
            throw new NotImplementedException();
        }

        public void EvictAll()
        {
            throw new NotImplementedException();
        }

        public void Flush()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Processing

        public object ExecuteOnKey(TKey key, IEntryProcessor processor)
        {
            throw new NotImplementedException();
        }

        public IDictionary<TKey, object> ExecuteOnKeys(ISet<TKey> keys, IEntryProcessor processor)
        {
            throw new NotImplementedException();
        }

        public IDictionary<TKey, object> ExecuteOnEntries(IEntryProcessor processor)
        {
            throw new NotImplementedException();
        }

        public Task<object> SubmitToKey(TKey key, IEntryProcessor processor)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Locking

        public void Lock(TKey key)
        {
            throw new NotImplementedException();
        }

        public void Lock(TKey key, TimeSpan leaseTime)
        {
            throw new NotImplementedException();
        }

        public bool TryLock(TKey key)
        {
            throw new NotImplementedException();
        }

        public bool TryLock(TKey key, TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        public bool TryLock(TKey key, TimeSpan timeout, TimeSpan leaseTime)
        {
            throw new NotImplementedException();
        }

        public bool IsLocked(TKey key)
        {
            throw new NotImplementedException();
        }

        public void ForceUnlock(TKey key)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Indexing

        public void AddIndex(IndexType type, params string[] attributes)
        {
            throw new NotImplementedException();
        }

        public void AddIndex(IndexConfig indexConfig)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Aggregating and Projecting

        public TResult Aggregate<TResult>(IAggregator<TResult> aggregator, IPredicate predicate = null)
        {
            throw new NotImplementedException();
        }

        public ICollection<TResult> Project<TResult>(IProjection projection, IPredicate predicate = null)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Intercepting

        public void RemoveInterceptor(string id)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Events

        public bool RemoveEntryListener(Guid id)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
