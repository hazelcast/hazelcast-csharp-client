using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Core.Collections;
using Hazelcast.DistributedObjects.Implementation.Map;
using Hazelcast.NearCaching;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.DistributedObjects.Implementation.CachedMap
{
    /// <summary>
    /// Implements a caching version of <see cref="IMap{TKey,TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    internal class CachedMap<TKey, TValue> : Map<TKey, TValue>
    {
        private readonly NearCacheBase _cache;

        /// <summary>
        /// Initializes a new version of the <see="CacheMap{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="serviceName">the name of the service managing this object.</param>
        /// <param name="name">The unique name of the object.</param>
        /// <param name="cluster">A cluster.</param>
        /// <param name="serializationService">A serialization service.</param>
        /// <param name="lockReferenceIdSequence">A lock reference identifiers sequence.</param>
        /// <param name="cache">A cache.</param>
        /// <param name="loggerFactory">A logger factory.</param>
        public CachedMap(string serviceName, string name, Cluster cluster, ISerializationService serializationService, ISequence<long> lockReferenceIdSequence, NearCacheBase cache, ILoggerFactory loggerFactory)
            : base(serviceName, name, cluster, serializationService, lockReferenceIdSequence, loggerFactory)
        {
            _cache = cache;
        }

        // TODO: consider invalidating in a continuation?
        // TODO: not every methods are overriden, and then what?
        // TODO: OnInitialize, OnShutdown, PostDestroy and IDisposable?
        // TODO: refactor Map and CachedMap, so we don't need to serialize key to keyData all the time!
        //       generally, Map+NearCache can be greatly optimized, but we'll do that later

        // TODO: split regions

        #region Setting

        /// <inheritdoc />
        protected override async Task<TValue> AddOrReplaceWithValueAsync(IData keyData, IData valueData, TimeSpan timeToLive)
        {
            _cache.Invalidate(keyData);
            return await base.AddOrReplaceWithValueAsync(keyData, valueData, timeToLive);
        }

        /// <inheritdoc />
        protected override async Task AddOrReplaceAsync(Dictionary<Guid, Dictionary<int, List<KeyValuePair<IData, IData>>>> ownerEntries)
        {
            // see comments on the base Map class
            // this should be no different except for this entry invalidation method,
            // added as a continuation to each task that is being created

            void InvalidateEntries(IEnumerable<KeyValuePair<IData, IData>> list)
            {
                foreach (var (key, _) in list)
                    _cache.Invalidate(key);
            }

            var tasks = new List<Task>();
            foreach (var (ownerId, part) in ownerEntries)
            {
                foreach (var (partitionId, list) in part)
                {
                    if (list.Count == 0) continue;

                    var requestMessage = MapPutAllCodec.EncodeRequest(Name, list);
                    requestMessage.PartitionId = partitionId;
                    var task = Cluster.SendToMemberAsync(requestMessage, ownerId).AsTask()
                        .ContinueWith(_ => InvalidateEntries(list));
                    tasks.Add(task);
                }
            }

            await Task.WhenAll(tasks);
        }

        /// <inheritdoc />
        protected override async Task<bool> TryAddOrReplaceAsync(IData keyData, IData valueData, TimeSpan timeout)
        {
            var added = await base.TryAddOrReplaceAsync(keyData, valueData, timeout);
            if (added) _cache.Invalidate(keyData);
            return added;
        }

        /// <inheritdoc />
        protected override async Task<TValue> AddIfMissingAsync(IData keyData, IData valueData, TimeSpan timeToLive)
        {
            _cache.Invalidate(keyData);
            return await base.AddIfMissingAsync(keyData, valueData, timeToLive);
        }

        /// <inheritdoc />
        protected override async Task AddOrReplaceTransientAsync(IData keyData, IData valueData, TimeSpan timeToLive)
        {
            _cache.Invalidate(keyData);
            await base.AddOrReplaceTransientAsync(keyData, valueData, timeToLive);
        }

        #endregion

        #region Getting

        /// <inheritdoc />
        protected override async Task<object> GetAsync(IData keyData)
        {
            try
            {
                var attempt = await _cache.TryGetOrAddAsync(keyData, data => base.GetAsync(keyData));
                return attempt.ValueOr(default);
            }
            catch
            {
                _cache.Invalidate(keyData);
                throw;
            }
        }

        /// <inheritdoc />
        protected override async Task<ReadOnlyLazyDictionary<TKey, TValue>> GetAsync(Dictionary<Guid, Dictionary<int, List<IData>>> ownerKeys)
        {
            var cachedEntries = new Dictionary<IData, object>();

            foreach (var (_, part) in ownerKeys)
            {
                foreach (var (_, list) in part)
                {
                    var remove = new List<IData>();
                    foreach (var key in list)
                    {
                        if (_cache.TryGetValue(key, out var value))
                        {
                            remove.Add(key);
                            cachedEntries[key] = value;
                        }
                    }

                    foreach (var key in remove)
                        list.Remove(key);
                }
            }

            var entries = await base.GetAsync(ownerKeys);

            // _cache may contain either the value data (IData) or the
            // de-serialized object (TValue), depending on configuration

            // cache the retrieved entries
            // none of them have a value yet, and ...
            // FIXME what is it we want to put in the cache?
            foreach (var (key, entry) in entries.Entries)
                await _cache.TryAdd(key, entry.ValueObject);

            // add cached entries to the retrieved entries
            foreach (var (key, valueObject) in cachedEntries)
                entries.Add(key, valueObject);

            return entries;
        }

        /// <inheritdoc />
        protected override async Task<bool> ContainsKeyAsync(IData keyData)
        {
            return _cache.ContainsKey(keyData) || await base.ContainsKeyAsync(keyData);
        }

        #endregion

        #region Removing

        /// <inheritdoc />
        protected override async Task<bool> TryRemoveAsync(IData keyData, TimeSpan timeout)
        {
            var removed = await base.TryRemoveAsync(keyData, timeout);
            if (removed) _cache.Invalidate(keyData);
            return removed;
        }

        /// <inheritdoc />
        protected override async Task<TValue> RemoveAsync(IData keyData)
        {
            try
            {
                return await base.RemoveAsync(keyData);
            }
            finally
            {
                _cache.Invalidate(keyData);
            }
        }

        /// <inheritdoc />
        protected override async Task<bool> RemoveAsync(IData keyData, IData valueData)
        {
            try
            {
                return await base.RemoveAsync(keyData, valueData);
            }
            finally
            {
                _cache.Invalidate(keyData);
            }
        }

        /// <inheritdoc />
        protected override async Task DeleteAsync(IData keyData)
        {
            try
            {
                await base.RemoveAsync(keyData);
            }
            finally
            {
                _cache.Invalidate(keyData);
            }
        }

        /// <inheritdoc />
        public override async Task ClearAsync()
        {
            await base.ClearAsync().ContinueWith(_ => _cache.InvalidateAll()).ConfigureAwait(false);
        }

        #endregion

        #region Processing

        // <inheritdoc />
        protected override async Task<object> ExecuteAsync(IData processorData, IData keyData)
        {
            return await base.ExecuteAsync(processorData, keyData).ContinueWith(t =>
            {
                _cache.Invalidate(keyData);
                return t;
            });
        }

        // <inheritdoc />
        protected override async Task<object> ApplyAsync(IData processorData, IData keyData)
        {
            return await base.ApplyAsync(processorData, keyData).ContinueWith(t =>
            {
                _cache.Invalidate(keyData);
                return t;
            });
        }

        #endregion
    }
}
