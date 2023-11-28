// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Exceptions;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.NearCaching
{
    /// <summary>
    /// Provides a base class for Near Caches.
    /// </summary>
    internal abstract class NearCacheBase : IAsyncDisposable
    {
        private readonly int _evictionPercentage;
        private readonly int _cleanupInterval;

        private readonly ConcurrentAsyncDictionary<IData, NearCacheEntry> _entries;
        private readonly EvictionPolicy _evictionPolicy;
        private readonly long _maxIdleMilliseconds;
        private readonly int _maxSize;
        private readonly long _timeToLive;
        private readonly IComparer<NearCacheEntry> _evictionComparer;

        private int _expiring;
        private int _evicting;
        private long _lastExpire; // last time expiration ran

        /// <summary>
        /// Initializes a new instance of the <see cref="NearCacheBase"/> class.
        /// </summary>
        /// <param name="name">The name of the cache.</param>
        /// <param name="cluster">The cluster.</param>
        /// <param name="serializationService">The localization service.</param>
        /// <param name="loggerFactory">A logger factory.</param>
        /// <param name="nearCacheOptions">NearCache options.</param>
        protected NearCacheBase(string name, Cluster cluster, SerializationService serializationService, ILoggerFactory loggerFactory, NearCacheOptions nearCacheOptions)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(name));
            Name = name;
            Cluster = cluster ?? throw new ArgumentNullException(nameof(cluster));
            SerializationService = serializationService ?? throw new ArgumentNullException(nameof(serializationService));
            LoggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            Options = nearCacheOptions ?? throw new ArgumentNullException(nameof(nearCacheOptions));

            _entries = new ConcurrentAsyncDictionary<IData, NearCacheEntry>();
            Statistics = new NearCacheStatistics(name);

            _lastExpire = Clock.Never;

            _maxIdleMilliseconds = nearCacheOptions.MaxIdleSeconds * 1000;
            InMemoryFormat = nearCacheOptions.InMemoryFormat;
            _timeToLive = nearCacheOptions.TimeToLiveSeconds * 1000;
            _evictionComparer = GetEvictionComparer(_evictionPolicy);

            _maxSize = nearCacheOptions.Eviction.Size; // nearCacheOptions.MaxSize;
            _evictionPolicy = nearCacheOptions.Eviction.EvictionPolicy; // nearCacheOptions.EvictionPolicy;
            _evictionPercentage = nearCacheOptions.EvictionPercentage;
            _cleanupInterval = nearCacheOptions.CleanupPeriodSeconds * 1000;

            cluster.State.Failover.ClusterChanged += (options) =>
            {
                 Clear();
            };
        }

        /// <summary>
        /// Gets the name of the cache.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the options for this cache.
        /// </summary>
        protected NearCacheOptions Options { get; }

        /// <summary>
        /// Gets the cluster.
        /// </summary>
        protected Cluster Cluster { get; }

        /// <summary>
        /// Gets the in-memory format.
        /// </summary>
        public InMemoryFormat InMemoryFormat { get; }

        /// <summary>
        /// Gets the serialization service.
        /// </summary>
        public SerializationService SerializationService { get; }

        /// <summary>
        /// Gets the logger factory.
        /// </summary>
        protected ILoggerFactory LoggerFactory { get; }

        /// <summary>
        /// Gets statistics
        /// </summary>
        public NearCacheStatistics Statistics { get; }

        /// <summary>
        /// Gets the raw entries count.
        /// </summary>
        public int Count => _entries.Count;

        /// <summary>
        /// (internal for tests only)
        /// Gets a snapshot of the cache entries.
        /// </summary>
        internal async Task<List<NearCacheEntry>> SnapshotEntriesAsync()
        {
            var list = new List<NearCacheEntry>();
            await foreach (var e in _entries)
                list.Add(e.Value);
            return list;
        }

        #region Initialize & Destroy

        /// <summary>
        /// Initializes the cache.
        /// </summary>
        public abstract ValueTask InitializeAsync();

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore().CfAwait();
            _entries.Clear();
        }

        /// <summary>
        /// Performs <see cref="DisposeAsync"/> in inherited classes.
        /// </summary>
        /// <returns></returns>
        protected virtual ValueTask DisposeAsyncCore() => default;

        #endregion

        #region Add, Get, Contains, Remove & Clear

        /// <summary>
        /// Tries to add a value to the cache.
        /// </summary>
        /// <param name="keyData">The key data.</param>
        /// <param name="valueData">The value data.</param>
        /// <returns><c>true</c> if the value could be added; otherwise <c>false</c>.</returns>
        public async Task<bool> TryAddAsync(IData keyData, IData valueData)
        {
            // kick eviction policy if needed
            if (_evictionPolicy != EvictionPolicy.None && _entries.Count >= _maxSize)
                await EvictEntries().CfAwait();

            // cannot add if the cache is full
            if (_evictionPolicy == EvictionPolicy.None && _entries.Count >= _maxSize)
                return false;

            ValueTask<NearCacheEntry> CreateEntry(IData _, CancellationToken __)
            {
                return new ValueTask<NearCacheEntry>(CreateCacheEntry(keyData, ToCachedValue(valueData)));
            }

            // if we put an async entry in an async dictionary and the factory throws,
            // then the entry will be removed from the dictionary - and also, when
            // TryAddAsync completes, the value has been added (the factory has completed
            // too) - so, there is no need to remove anything from the cache in case
            // of an exception
            //
            // likewise, the dictionary treats null values as invalid and CreateCacheEntry
            // returns null if the cached value (ValueObject) is null - all in all, safe

            try
            {
                var added = await _entries.TryAddAsync(keyData, CreateEntry).CfAwait();
                if (added) Statistics.NotifyEntryAdded();
                return added;
            }
            catch
            {
                // ignore - should we log?
                return false;
            }
        }

        /// <summary>
        /// Tries to get a value from, or add a value to, the cache.
        /// </summary>
        /// <param name="keyData">The key data.</param>
        /// <param name="valueFactory">A factory that accepts the key data and returns the value data.</param>
        /// <returns>An attempt at getting or adding a value to the cache.</returns>
        public async Task<Attempt<object>> TryGetOrAddAsync(IData keyData, Func<IData, Task<IData>> valueFactory)
        {
            // if it's in the cache already, return it
            // (and TryGetAsync counts a hit)
            // (otherwise, TryGetAsync counts a miss, so we don't have to do it here)
            var (hasEntry, valueObject) = await TryGetAsync(keyData).CfAwait();
            if (hasEntry) return valueObject;

            // kick eviction policy if needed
            if (_evictionPolicy != EvictionPolicy.None && _entries.Count >= _maxSize)
                await EvictEntries().CfAwait();

            // if the cache is full, directly return the un-cached value
            if (_evictionPolicy == EvictionPolicy.None && _entries.Count >= _maxSize && !await _entries.ContainsKeyAsync(keyData).CfAwait())
                return Attempt.Fail(ToCachedValue(await valueFactory(keyData).CfAwait()));

            async ValueTask<NearCacheEntry> CreateEntry(IData _, CancellationToken __)
            {
                var valueData = await valueFactory(keyData).CfAwait();
                var cachedValue = ToCachedValue(valueData);

                return CreateCacheEntry(keyData, cachedValue); // null if cachedValue is null
            }

            var entry = await _entries.GetOrAddAsync(keyData, CreateEntry).CfAwait();
            if (entry != null) // null if ValueObject would have been null
            {

                Statistics.NotifyEntryAdded();
                return entry.ValueObject;
            }

            // the entry will not stick in _entries
            // and we haven't notified statistics

            return Attempt.Failed;
        }

        /// <summary>
        /// Tries to get a value from the cache.
        /// </summary>
        /// <param name="keyData">The key data.</param>
        /// <param name="hit">Whether to hit the entry or not.</param>
        /// <returns>An attempt at getting the value for the specified key.</returns>
        public async ValueTask<Attempt<object>> TryGetAsync(IData keyData, bool hit = true)
        {
            await ExpireEntries().CfAwait();

            // it is not possible to get a null entry, nor an entry with a null ValueObject
            var (hasEntry, entry) = await _entries.TryGetAsync(keyData).CfAwait();

            if (!hasEntry)
            {
                Statistics.NotifyMiss();
                return Attempt.Failed;
            }

            if (IsStaleRead(entry))
            {
                Remove(keyData);
                Statistics.NotifyMiss();
                Statistics.NotifyStaleRead();
                return Attempt.Failed;
            }

            if (IsExpired(entry))
            {
                Remove(keyData);
                Statistics.NotifyMiss();
                Statistics.NotifyExpiration();
                return Attempt.Failed;
            }

            if (hit)
            {
                entry.NotifyHit();
                Statistics.NotifyHit();
            }

            return entry.ValueObject;
        }

        /// <summary>
        /// Determines whether the cache contains an entry.
        /// </summary>
        /// <param name="keyData">The key data.</param>
        /// <param name="hit">Whether to hit the entry or not.</param>
        /// <returns>Whether the cache contains an entry with the specified key.</returns>
        public async ValueTask<bool> ContainsKeyAsync(IData keyData, bool hit = true)
        {
            var (contains, _) = await TryGetAsync(keyData, hit).CfAwait();
            return contains;
        }

        /// <summary>
        /// Removes an entry from the cache.
        /// </summary>
        /// <param name="keyData">The key data.</param>
        /// <returns>Whether an entry was removed.</returns>
        public bool Remove(IData keyData)
        {
            if (!_entries.TryRemove(keyData))
                return false;

            Statistics.NotifyEntryRemoved();
            return true;
        }

        /// <summary>
        /// Clears the cache.
        /// </summary>
        public void Clear()
        {
            _entries.Clear();
            Statistics.ResetEntryCount();
        }

        /// <summary>
        /// Creates a new cache entry.
        /// </summary>
        /// <param name="keyData">The key data.</param>
        /// <param name="valueObject">The value object, which is either <see cref="IData"/> or the actual <see cref="object"/>.</param>
        /// <returns>A new cache entry, if <paramref name="valueObject"/> is not <c>null</c>, otherwise <c>null</c>.</returns>
        protected virtual NearCacheEntry CreateCacheEntry(IData keyData, object valueObject)
        {
            return valueObject == null ? null : new NearCacheEntry(keyData, valueObject, _timeToLive);
        }

        /// <summary>
        /// Determines whether a cached entry is stale.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <returns>Whether the entry is stale.</returns>
        protected virtual bool IsStaleRead(NearCacheEntry entry) => false;

        /// <summary>
        /// Converts a value <see cref="IData"/> to the internal cached value format.
        /// </summary>
        /// <param name="valueData">Value data.</param>
        /// <returns>Internal cached value.</returns>
        protected virtual object ToCachedValue(IData valueData)
        {
            return InMemoryFormat.Equals(InMemoryFormat.Binary)
                ? valueData
                : SerializationService.ToObject<object>(valueData);
        }

        #endregion

        #region Evict & Expire

        // entries are evicted on each add (TryAdd, TryGetOrAdd)
        // entries are expired on each get (TryGetValue)

        /// <summary>
        /// Evicts entries if not already evicting.
        /// </summary>
        private async ValueTask EvictEntries()
        {
            try
            {
                // only one at a time please
                if (Interlocked.CompareExchange(ref _evicting, 1, 0) == 1)
                    return;

                await DoEvictEntries().CfAwait();
            }
            finally
            {
                // make sure to release the lock
                Interlocked.Exchange(ref _evicting, 0);
            }
        }

        /// <summary>
        /// Evicts entries.
        /// </summary>
        /// <returns></returns>
        private async ValueTask DoEvictEntries()
        {
            if (_evictionPolicy == EvictionPolicy.None || _entries.Count < _maxSize)
                return;

            var entries = new SortedSet<NearCacheEntry>(_evictionComparer);
            await foreach (var (_, value) in _entries)
                entries.Add(value);

            var evictCount = entries.Count * _evictionPercentage / 100;
            if (evictCount < 1)
                return;

            var count = 0;

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var entry in entries)
            {
                if (!_entries.TryRemove(entry.KeyData))
                    continue;

                Statistics.NotifyEntryRemoved();
                Statistics.NotifyEviction();

                if (++count > evictCount)
                    break;
            }

            // original code would repeat if (_entries.Count >= _maxSize)
            // but that can potentially lead to endless loops - removing
        }

        /// <summary>
        /// Expires entries if not already expiring.
        /// </summary>
        private async ValueTask ExpireEntries()
        {
            // run when it is time to run
            if (Clock.Milliseconds < _lastExpire + _cleanupInterval)
                return;

            try
            {
                // only one at a time please
                if (Interlocked.CompareExchange(ref _expiring, 1, 0) == 1)
                    return;

                // double check
                if (Clock.Milliseconds < _lastExpire + _cleanupInterval)
                    return;

                _lastExpire = Clock.Milliseconds;

                await DoExpireEntries().CfAwait();
            }
            finally
            {
                // make sure to release the lock
                Interlocked.Exchange(ref _expiring, 0);
            }
        }

        /// <summary>
        /// Expire entries.
        /// </summary>
        private async ValueTask DoExpireEntries()
        {
            await foreach (var (key, entry) in _entries)
            {
                if (!IsExpired(entry)) continue;

                Remove(key);
                Statistics.NotifyExpiration();
            }
        }

        /// <summary>
        /// Gets the record comparer corresponding to an eviction policy.
        /// </summary>
        /// <param name="policy">The eviction policy.</param>
        /// <returns>The record comparer corresponding to the specified eviction policy.</returns>
        private static IComparer<NearCacheEntry> GetEvictionComparer(EvictionPolicy policy)
        {
            return policy switch
            {
                EvictionPolicy.Lfu => new LfuComparer(),
                EvictionPolicy.Lru => new LruComparer(),
                EvictionPolicy.None => new DefaultComparer(),
                EvictionPolicy.Random => new RandomComparer(),
                _ => throw new NotSupportedException()
            };
        }

        /// <summary>
        /// Determines whether a record has expired.
        /// </summary>
        /// <param name="entry">The record.</param>
        /// <returns>true if the record has expired; false otherwise.</returns>
        private bool IsExpired(NearCacheEntry entry)
        {
            var now = Clock.Milliseconds;
            return entry.IsExpiredAt(now) || entry.IsIdleAt(_maxIdleMilliseconds, now);
        }

        #endregion
    }
}
