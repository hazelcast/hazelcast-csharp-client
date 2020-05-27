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
        /// <param name="name">The unique name of the object.</param>
        /// <param name="cluster">A cluster.</param>
        /// <param name="serializationService">A serialization service.</param>
        /// <param name="lockReferenceIdSequence">A lock reference identifiers sequence.</param>
        /// <param name="cache">A cache.</param>
        /// <param name="loggerFactory">A logger factory.</param>
        public CachedMap(string name, Cluster cluster, ISerializationService serializationService, ISequence<long> lockReferenceIdSequence, NearCacheBase cache, ILoggerFactory loggerFactory)
            : base(name, cluster, serializationService, lockReferenceIdSequence, loggerFactory)
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
        protected override
#if !OPTIMIZE_ASYNC
            async
#endif
        Task<TValue> AddOrReplaceWithValueAsync(IData keyData, IData valueData, TimeSpan timeToLive, CancellationToken cancellationToken)
        {
            _cache.Invalidate(keyData);
            var task = base.AddOrReplaceWithValueAsync(keyData, valueData, timeToLive, cancellationToken);

#if OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <inheritdoc />
        protected override
#if !OPTIMIZE_ASYNC
            async
#endif
        Task AddOrReplaceAsync(Dictionary<Guid, Dictionary<int, List<KeyValuePair<IData, IData>>>> ownerEntries, CancellationToken cancellationToken)
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
                    var ownerTask = Cluster.SendToMemberAsync(requestMessage, ownerId, cancellationToken)
                        .ContinueWith(_ => InvalidateEntries(list), CancellationToken.None);
                    tasks.Add(ownerTask);
                }
            }

            var task = Task.WhenAll(tasks);


#if OPTIMIZE_ASYNC
            return task;
#else
            await task.CAF();
#endif
        }

        /// <inheritdoc />
        protected override async Task<bool> TryAddOrReplaceAsync(IData keyData, IData valueData, TimeSpan serverTimeout, CancellationToken cancellationToken)
        {
            var added = await base.TryAddOrReplaceAsync(keyData, valueData, serverTimeout, cancellationToken).CAF();
            if (added) _cache.Invalidate(keyData);
            return added;
        }

        /// <inheritdoc />
        protected override
#if !OPTIMIZE_ASYNC
            async
#endif
            Task<TValue> AddIfMissingAsync(IData keyData, IData valueData, TimeSpan timeToLive, CancellationToken cancellationToken)
        {
            _cache.Invalidate(keyData);
            var task = base.AddIfMissingAsync(keyData, valueData, timeToLive, cancellationToken);

#if OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <inheritdoc />
        protected override
#if !OPTIMIZE_ASYNC
            async
#endif
        Task AddOrReplaceTransientAsync(IData keyData, IData valueData, TimeSpan timeToLive, CancellationToken cancellationToken)
        {
            _cache.Invalidate(keyData);
            var task = base.AddOrReplaceTransientAsync(keyData, valueData, timeToLive, cancellationToken);

#if OPTIMIZE_ASYNC
            return task;
#else
            await task.CAF();
#endif
        }

        #endregion

        #region Getting

        /// <inheritdoc />
        protected override async Task<IData> GetAsync(IData keyData, CancellationToken cancellationToken)
        {
            async Task<object> BaseGetAsync(IData data, CancellationToken token)
                => await base.GetAsync(data, token).CAF();

            try
            {
                var attempt = await _cache.TryGetOrAddAsync(keyData, data => BaseGetAsync(keyData, cancellationToken)).CAF();
                return (IData) attempt.ValueOr(default);
            }
            catch
            {
                _cache.Invalidate(keyData);
                throw;
            }
        }

        /// <inheritdoc />
        protected override async Task<ReadOnlyLazyDictionary<TKey, TValue>> GetAsync(Dictionary<Guid, Dictionary<int, List<IData>>> ownerKeys, CancellationToken cancellationToken)
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

            var entries = await base.GetAsync(ownerKeys, cancellationToken).CAF();

            // _cache may contain either the value data (IData) or the
            // de-serialized object (TValue), depending on configuration

            // cache the retrieved entries
            // none of them have a value yet, and ...
            // FIXME what is it we want to put in the cache?
            foreach (var (key, entry) in entries.Entries)
                await _cache.TryAdd(key, entry.ValueObject).CAF();

            // add cached entries to the retrieved entries
            foreach (var (key, valueObject) in cachedEntries)
                entries.Add(key, valueObject);

            return entries;
        }

        /// <inheritdoc />
        protected override async Task<bool> ContainsKeyAsync(IData keyData, CancellationToken cancellationToken)
        {
            return _cache.ContainsKey(keyData) || await base.ContainsKeyAsync(keyData, cancellationToken).CAF();
        }

        #endregion

        #region Removing

        /// <inheritdoc />
        protected override async Task<bool> TryRemoveAsync(IData keyData, TimeSpan serverTimeout, CancellationToken cancellationToken)
        {
            var removed = await base.TryRemoveAsync(keyData, serverTimeout, cancellationToken).CAF();
            if (removed) _cache.Invalidate(keyData);
            return removed;
        }

        /// <inheritdoc />
        protected override async Task<TValue> RemoveAsync(IData keyData, CancellationToken cancellationToken)
        {
            try
            {
                return await base.RemoveAsync(keyData, cancellationToken).CAF();
            }
            finally
            {
                _cache.Invalidate(keyData);
            }
        }

        /// <inheritdoc />
        protected override async Task<bool> RemoveAsync(IData keyData, IData valueData, CancellationToken cancellationToken)
        {
            try
            {
                return await base.RemoveAsync(keyData, valueData, cancellationToken).CAF();
            }
            finally
            {
                _cache.Invalidate(keyData);
            }
        }

        /// <inheritdoc />
        protected override async Task DeleteAsync(IData keyData, CancellationToken cancellationToken)
        {
            try
            {
                await base.RemoveAsync(keyData, cancellationToken).CAF();
            }
            finally
            {
                _cache.Invalidate(keyData);
            }
        }

        /// <inheritdoc />
        public override async Task ClearAsync(CancellationToken cancellationToken)
        {
            await base.ClearAsync(cancellationToken)
                .ContinueWith(_ => _cache.InvalidateAll(), CancellationToken.None)
                .CAF();
        }

        #endregion

        #region Processing

        // <inheritdoc />
        protected override
#if !OPTIMIZE_ASYNC
            async
#endif
        Task<object> ExecuteAsync(IData processorData, IData keyData, CancellationToken cancellationToken)
        {
            var task = base.ExecuteAsync(processorData, keyData, cancellationToken).ContinueWith(t =>
            {
                _cache.Invalidate(keyData);
                return t;
            }, CancellationToken.None);

#if OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        // <inheritdoc />
        protected override
#if !OPTIMIZE_ASYNC
            async
#endif
        Task<object> ApplyAsync(IData processorData, IData keyData, CancellationToken cancellationToken)
        {
            var task = base.ApplyAsync(processorData, keyData, cancellationToken).ContinueWith(t =>
            {
                _cache.Invalidate(keyData);
                return t;
            }, CancellationToken.None);

#if OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        #endregion
    }
}
