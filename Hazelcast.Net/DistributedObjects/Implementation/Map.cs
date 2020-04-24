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
using Hazelcast.Data;
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
        // TODO return IEnumerable or IReadOnlyList?

        #region Setting

        /// <inheritdoc />
        public async Task<TValue> AddAsync(TKey key, TValue value)
            => await AddAsync(key, value, Timeout.InfiniteTimeSpan);

        /// <inheritdoc />
        public async Task<TValue> AddAsync(TKey key, TValue value, TimeSpan timeToLive)
        {
            var (keyData, valueData) = ToSafeData(key, value);

            var timeToLiveMs = timeToLive.CodecMilliseconds(-1000);

            var requestMessage = MapPutCodec.EncodeRequest(Name, keyData, valueData, ThreadId, timeToLiveMs);
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

            var timeToLiveMs = timeToLive.CodecMilliseconds(-1000);

            // FIXME uh?
            var requestMessage = MapPutTransientCodec.EncodeRequest(Name, keyData, valueData, ThreadId, timeToLiveMs);
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

            var timeToLiveMs = timeToLive.CodecMilliseconds(-1000);

            var requestMessage = MapSetCodec.EncodeRequest(Name, keyData, valueData, ThreadId, timeToLiveMs);
            await Cluster.SendAsync(requestMessage, keyData);
        }

        /// <inheritdoc />
        public async Task<bool> TryPutAsync(TKey key, TValue value, TimeSpan timeout)
        {
            var (keyData, valueData) = ToSafeData(key, value);

            var timeoutMs = timeout.CodecMilliseconds(0);

            var requestMessage = MapTryPutCodec.EncodeRequest(Name, keyData, valueData, ThreadId, timeoutMs);
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

        private static PagingPredicate UnwrapPagingPredicate(IPredicate predicate)
            => predicate as PagingPredicate ?? (predicate as PartitionPredicate)?.GetTarget() as PagingPredicate;

        #endregion

        #region Removing

        /// <inheritdoc />
        public async Task<bool> TryRemoveAsync(TKey key, TimeSpan timeout)
        {
            var keyData = ToSafeData(key);

            var timeoutMs = timeout.CodecMilliseconds(0);

            var requestMessage = MapTryRemoveCodec.EncodeRequest(Name, keyData, ThreadId, timeoutMs);
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
            var keyData = ToSafeData(key);
            var processorData = ToSafeData(processor);

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
            var keyData = ToSafeData(key);
            var processorData = ToSafeData(processor);

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
                return new ReadOnlyLazyList<TResult, IData>(response, SerializationService);
            }

            {
                var predicateData = ToData(predicate);

                var requestMessage = MapProjectWithPredicateCodec.EncodeRequest(Name, projectionData, predicateData);
                var responseMessage = await Cluster.SendAsync(requestMessage);
                var response = MapProjectWithPredicateCodec.DecodeResponse(responseMessage).Response;
                return new ReadOnlyLazyList<TResult, IData>(response, SerializationService);
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

        // TODO: do we need state if we have sender?
        // TODO: need overrides with key, predicate, etc

        /// <inheritdoc />
        public async Task<Guid> SubscribeAsync(
            Action<IMap<TKey, TValue>, EntryAddedEventArgs<TKey, TValue>> entryAdded = null,
            Action<IMap<TKey, TValue>, EntryRemovedEventArgs<TKey, TValue>> entryRemoved = null)
        {
            var flags = (EntryEventType) 0;
            if (entryAdded != null) flags |= EntryEventType.Added;
            if (entryRemoved != null) flags |= EntryEventType.Removed;

            // FIXME
            var includeValue = true;
            var localOnly = false;

            var subscribeRequest = MapAddEntryListenerCodec.EncodeRequest(Name, includeValue, (int) flags, localOnly);

            void HandleEntryEvent(IData keyData, IData valueData, IData oldValueData, IData mergingValueData, int eventType, Guid memberId, int numberOfAffectedEntries)
            {
                if (((int)flags & eventType) == 0) return;

                var member = Cluster.GetMember(memberId);

                // TODO or maybe we could initialize the args with Lazy<TKey> ?
                // but then the args would indirectly capture the serialization service

                var key = ToObject<TKey>(keyData);
                var value = ToObject<TValue>(valueData);

                if (((EntryEventType)eventType).HasFlag(EntryEventType.Added))
                    entryAdded?.Invoke(this, new EntryAddedEventArgs<TKey, TValue>(key, value));
                if (((EntryEventType)eventType).HasFlag(EntryEventType.Removed))
                    entryRemoved?.Invoke(this, new EntryRemovedEventArgs<TKey, TValue>(key, value));
                // TODO and many more
            }

            var subscription = new ClusterEventSubscription(
                subscribeRequest,
                responseMessage => MapAddEntryListenerCodec.DecodeResponse(responseMessage).Response,
                subscriptionId => MapRemoveEntryListenerCodec.EncodeRequest(Name, subscriptionId),
                eventMessage => MapAddEntryListenerCodec.EventHandler.HandleEvent(eventMessage, HandleEntryEvent));

            await Cluster.SubscribeAsync(subscription);

            return subscription.Id;
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

    public abstract class EventArgsBase
    {
        //protected EventArgsBase(object source) // fixme this is a sender and does not belong here!
        //{
        //    Source = source;
        //}

        //public object Source { get; }
    }

    public class EntryAddedEventArgs<TKey, TValue> : EventArgsBase
    {
        public EntryAddedEventArgs(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }

        public MemberInfo Member { get; }

        public string Name { get; } // name of the map, why?

        public EntryEventType EventType { get; } // why?

        public TKey Key { get; }

        public TValue Value { get; }
    }

    public class EntryRemovedEventArgs<TKey, TValue> : EventArgsBase
    {
        public EntryRemovedEventArgs(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }

        public MemberInfo Member { get; }

        public string Name { get; } // name of the map, why?

        public EntryEventType EventType { get; } // why?

        public TKey Key { get; }

        public TValue Value { get; }
    }
}
