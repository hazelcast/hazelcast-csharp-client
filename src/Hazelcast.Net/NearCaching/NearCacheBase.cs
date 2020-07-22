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
using Hazelcast.Exceptions;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.NearCaching
{
    // TODO: document and cleanup + the implementation may be broken (later)
    /// <summary>
    /// Provides a base class for Near Caches.
    /// </summary>
    internal abstract class NearCacheBase
    {
        private const int EvictionPercentage = 20;
        private const int CleanupInterval = 5000;

        private readonly ConcurrentAsyncDictionary<IData, NearCacheEntry> _entries;
        private readonly EvictionPolicy _evictionPolicy;
        private readonly InMemoryFormat _inMemoryFormat;
        private readonly long _maxIdleMilliseconds;
        private readonly int _maxSize;
        private readonly IComparer<NearCacheEntry> _evictionComparer;
        private readonly long _timeToLiveMillis;

        private int _canExpire; // 0 when expiring
        private int _canEvict; // 0 when evicting
        private long _lastExpire; // last time expiration ran

        /// <summary>
        /// Initializes a new instance of the <see cref="NearCacheBase"/> class.
        /// </summary>
        /// <param name="name">The name of the cache.</param>
        /// <param name="cluster">The cluster.</param>
        /// <param name="serializationService">The localization service.</param>
        /// <param name="loggerFactory">A logger factory.</param>
        /// <param name="nearCacheNamedOptions">NearCache options.</param>
        protected NearCacheBase(string name, Cluster cluster, ISerializationService serializationService, ILoggerFactory loggerFactory, NearCacheNamedOptions nearCacheNamedOptions)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(name));
            Name = name;
            Cluster = cluster ?? throw new ArgumentNullException(nameof(cluster));
            SerializationService = serializationService ?? throw new ArgumentNullException(nameof(serializationService));
            LoggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            Options = nearCacheNamedOptions ?? throw new ArgumentNullException(nameof(nearCacheNamedOptions));

            _entries = new ConcurrentAsyncDictionary<IData, NearCacheEntry>();
            Statistics = new NearCacheStatistics();

            _canExpire = 1;
            _canEvict = 1;
            _lastExpire = Clock.Milliseconds;

            _maxSize = nearCacheNamedOptions.MaxSize;
            _maxIdleMilliseconds = nearCacheNamedOptions.MaxIdleSeconds * 1000;
            _inMemoryFormat = nearCacheNamedOptions.InMemoryFormat;
            _timeToLiveMillis = nearCacheNamedOptions.TimeToLiveSeconds * 1000;
            _evictionPolicy = nearCacheNamedOptions.EvictionPolicy;
            _evictionComparer = GetEvictionComparer(_evictionPolicy);
        }

        /// <summary>
        /// Gets the name of the cache.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the options for this cache.
        /// </summary>
        protected NearCacheNamedOptions Options { get; }

        /// <summary>
        /// Whether the cache is invalidating.
        /// </summary>
        public bool Invalidating { get; protected set; }

        /// <summary>
        /// Gets the cluster.
        /// </summary>
        protected Cluster Cluster { get; }

        /// <summary>
        /// Gets the serialization service.
        /// </summary>
        protected ISerializationService SerializationService { get; }

        /// <summary>
        /// Gets the logger factory.
        /// </summary>
        protected ILoggerFactory LoggerFactory { get; }

        /// <summary>
        /// Gets or sets the identifier of the invalidation event subscription.
        /// </summary>
        protected Guid SubscriptionId { get; set; }

        /// <summary>
        /// Gets statistics
        /// </summary>
        public NearCacheStatistics Statistics { get; }

        #region Initialize & Destroy

        /// <summary>
        /// Initializes the cache.
        /// </summary>
        public abstract ValueTask InitializeAsync();

        /// <summary>
        /// Destroys the cache.
        /// </summary>
        // TODO: consider IDisposable?
        public virtual async ValueTask DestroyAsync()
        {
            if (SubscriptionId != default)
                await Cluster.RemoveSubscriptionAsync(SubscriptionId, CancellationToken.None).CAF();

            _entries.Clear();
        }

        #endregion

        #region Invalidate

        // TODO: use Remove() and Clear() instead?!

        public void Invalidate(IData key)
        {
            if (_entries.TryRemove(key))
                Statistics.DecrementEntryCount();
        }

        public void InvalidateAll()
        {
            _entries.Clear();
        }

        #endregion

        #region Add, Get, Contains, Remove & Clear

        /// <summary>
        /// Tries to add a value to the cache.
        /// </summary>
        /// <param name="keyData">The key data.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public async Task<bool> TryAdd(IData keyData, object value)
        {
            // kick eviction policy if needed
            if (_evictionPolicy != EvictionPolicy.None && _entries.Count >= _maxSize)
                await EvictEntries().CAF();

            // cannot add if the cache is full
            if (_evictionPolicy == EvictionPolicy.None && _entries.Count >= _maxSize)
                return false;

            ValueTask<NearCacheEntry> CreateCacheEntry(IData _, CancellationToken __)
            {
                // trust that the caller send the proper internal cache value
                // and we don't have to ToEntryValue(value) again
                var e = CreateEntry(keyData, value);
                Statistics.IncrementEntryCount();
                return new ValueTask<NearCacheEntry>(e);
            }

            try
            {
                var added = await _entries.TryAddAsync(keyData, CreateCacheEntry, default).CAF();
                return added;
            }
            catch
            {
                // ignore, remove from the cache
                Invalidate(keyData);
                return false;
            }

            // TODO: validate that this works as expected
        }

        /// <summary>
        /// Tries to get a value from, or add a value to, the cache.
        /// </summary>
        /// <param name="keyData">The key data.</param>
        /// <param name="valueFactory">A factory that accepts the key data and returns the value data.</param>
        /// <returns>An attempt containing the resulting object.</returns>
        /// <remarks>
        /// <para>Depending on the cache configuration, the returned object can be in serialized form (i.e.
        /// an <see cref="IData"/> instance) or already de-serialized.</para>
        /// </remarks>
        public async Task<Attempt<object>> TryGetOrAddAsync(IData keyData, Func<IData, Task<object>> valueFactory)
        {
            // if it's in the cache already, return it
            var (hasEntry, o) = await TryGetValue(keyData).CAF();
            if (hasEntry) return o;

            // count a miss
            Statistics.IncrementMiss();

            // kick eviction policy if needed
            if (_evictionPolicy != EvictionPolicy.None && _entries.Count >= _maxSize)
                await EvictEntries().CAF();

            // if the cache is full, directly return the un-cached value
            if (_evictionPolicy == EvictionPolicy.None && _entries.Count >= _maxSize && !await _entries.ContainsKeyAsync(keyData).CAF())
                return Attempt.Fail(await valueFactory(keyData).CAF());

            async ValueTask<NearCacheEntry> CreateCacheEntry(IData _, CancellationToken __)
            {
                var valueData = await valueFactory(keyData).CAF();
                var cachedValue = ToEntryValue(valueData);
                var e = CreateEntry(keyData, cachedValue);
                Statistics.IncrementEntryCount();
                return e;
            }

            var ncEntry = await _entries.GetOrAddAsync(keyData, CreateCacheEntry, default).CAF();

            if (ncEntry.Value != null) return ncEntry.Value;
            Invalidate(keyData);
            return Attempt.Failed;

            // TODO: validate that this works as expected
        }

        /// <summary>
        /// Tries to get a value from the cache.
        /// </summary>
        /// <param name="keyData">The key data.</param>
        /// <returns>An attempt containing the resulting object.</returns>
        public async ValueTask<Attempt<object>> TryGetValue(IData keyData)
        {
            await ExpireEntries().CAF();

            var (hasEntry, entry) = await _entries.TryGetValueAsync(keyData).CAF();
            if (!hasEntry || entry.Value == null) return Attempt.Failed;

            if (IsStaleRead(entry) || entry.Value == null)
            {
                Invalidate(keyData);
                Statistics.IncrementMiss();
                return Attempt.Failed;
            }

            if (IsExpired(entry))
            {
                Invalidate(keyData);
                Statistics.IncrementExpiration();
                return Attempt.Failed;
            }

            entry.NotifyHit();
            Statistics.IncrementHit();

            return entry.Value;
        }

        /// <summary>
        /// Determines whether the cache contains an entry.
        /// </summary>
        /// <param name="keyData">The key data.</param>
        /// <returns>Whether the cache contains an entry with the specified key.</returns>
        public async ValueTask<bool> ContainsKey(IData keyData)
        {
            var (contains, _) = await TryGetValue(keyData).CAF();
            return contains;
        }

        /// <summary>
        /// Removes an entry from the cache.
        /// </summary>
        /// <param name="key">The key data.</param>
        /// <returns>Whether an entry was removed.</returns>
        public bool Remove(IData key)
        {
            if (!_entries.TryRemove(key))
                return false;

            Statistics.DecrementEntryCount();
            return true;
        }

        /// <summary>
        /// Clears the cache.
        /// </summary>
        public void Clear()
        {
            _entries.Clear();
            Statistics.EntryCount = 0;
        }

        #endregion

        protected virtual NearCacheEntry CreateEntry(IData key, object value)
        {
            var created = Clock.Milliseconds;
            var expires = _timeToLiveMillis > 0 ? created + _timeToLiveMillis : Clock.Never;
            return new NearCacheEntry(key, value, created, expires);
        }

        /// <summary>
        /// Determines whether a cached entry is stale.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <returns>Whether the entry is stale.</returns>
        protected virtual bool IsStaleRead(NearCacheEntry entry) => false;

        /// <summary>
        /// Converts a value object to the internal cached value.
        /// </summary>
        /// <param name="o">Value object.</param>
        /// <returns>Internal cached value.</returns>
        /// <remarks>
        /// <para>Depending on the <see cref="InMemoryFormat"/> configured for the cache,
        /// the internal cached value can be either <see cref="IData"/> (i.e. serialized),
        /// or the de-serialized value.</para>
        /// </remarks>
        private object ToEntryValue(object o)
        {
            return _inMemoryFormat.Equals(InMemoryFormat.Binary)
                ? SerializationService.ToData(o)
                : SerializationService.ToObject<object>(o);
        }

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
                if (Interlocked.CompareExchange(ref _canEvict, 0, 1) == 0)
                    return;

                await DoEvictEntries().CAF();
            }
            finally
            {
                // make sure to release the lock
                Interlocked.Exchange(ref _canEvict, 1);
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

            var evictCount = entries.Count * EvictionPercentage / 100;
            if (evictCount < 1)
                return;

            var count = 0;

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var entry in entries)
            {
                if (!_entries.TryRemove(entry.Key))
                    continue;

                Statistics.DecrementEntryCount();
                Statistics.IncrementEviction();

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
            if (Clock.Milliseconds < _lastExpire + CleanupInterval)
                return;

            try
            {
                // only one at a time please
                if (Interlocked.CompareExchange(ref _canExpire, 0, 1) == 0)
                    return;

                _lastExpire = Clock.Milliseconds;

                await DoExpireEntries().CAF();
            }
            finally
            {
                // make sure to release the lock
                Interlocked.Exchange(ref _canExpire, 1);
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

                Invalidate(key);
                Statistics.IncrementExpiration();
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
