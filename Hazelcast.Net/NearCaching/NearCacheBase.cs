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
    internal abstract class NearCacheBase
    {
        private const int EvictionPercentage = 20;
        private const int CleanupInterval = 5000;

        private int _canCleanUp;
        private int _canEvict;

        private readonly ConcurrentAsyncDictionary<IData, NearCacheEntry> _entries;
        private readonly EvictionPolicy _evictionPolicy;
        private readonly InMemoryFormat _inMemoryFormat;
        protected readonly bool InvalidateOnChange;
        protected readonly ILoggerFactory LoggerFactory;
        //private readonly ILogger _logger;
        private readonly long _maxIdleMilliseconds;

        private readonly int _maxSize;

        private readonly IComparer<NearCacheEntry> _comparer;
        private readonly long _timeToLiveMillis;

        private long _lastCleanup;
        protected Guid RegistrationId;

        protected NearCacheBase(string name, Cluster cluster, ISerializationService serializationService, ILoggerFactory loggerFactory, NearCacheNamedOptions nearCacheNamedOptions)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(name));
            Name = name;
            Cluster = cluster ?? throw new ArgumentNullException(nameof(cluster));
            SerializationService = serializationService ?? throw new ArgumentNullException(nameof(serializationService));

            _entries = new ConcurrentAsyncDictionary<IData, NearCacheEntry>();
            Statistics = new NearCacheStatistics();

            _canCleanUp = 1;
            _canEvict = 1;
            _lastCleanup = Clock.Milliseconds;

            _maxSize = nearCacheNamedOptions.MaxSize;
            _maxIdleMilliseconds = nearCacheNamedOptions.MaxIdleSeconds * 1000;
            _inMemoryFormat = nearCacheNamedOptions.InMemoryFormat;
            _timeToLiveMillis = nearCacheNamedOptions.TimeToLiveSeconds * 1000;
            _evictionPolicy = nearCacheNamedOptions.EvictionPolicy;
            _comparer = GetComparer(_evictionPolicy);
            InvalidateOnChange = nearCacheNamedOptions.InvalidateOnChange;

            LoggerFactory = loggerFactory;
            //_logger = loggerFactory.CreateLogger<NearCacheBase>();
        }

        protected Cluster Cluster { get; }

        protected ISerializationService SerializationService { get; }

        public string Name { get; }

        public NearCacheStatistics Statistics { get; }

        public void Clear()
        {
            _entries.Clear();
            Statistics.EntryCount = 0L;
        }

        public async ValueTask<bool> ContainsKey(IData keyData)
        {
            var (contains, _) = await TryGetValue(keyData).CAF();
            return contains;
        }

        public virtual async ValueTask DestroyAsync()
        {
            if (RegistrationId != default)
                await Cluster.RemoveSubscriptionAsync(RegistrationId, CancellationToken.None).CAF();

            _entries.Clear();
        }

        public abstract void Init();

        public void Invalidate(IData key)
        {
            if (_entries.TryRemove(key))
            {
                Statistics.DecrementEntryCount();
            }
        }

        public void InvalidateAll()
        {
            _entries.Clear();
        }

        public bool Remove(IData key)
        {
            if (_entries.TryRemove(key))
            {
                Statistics.DecrementEntryCount();
                return true;
            }
            return false;
        }

        public async Task<bool> TryAdd(IData keyData, object value)
        {
            // kick eviction policy if needed
            if (_evictionPolicy != EvictionPolicy.None && _entries.Count >= _maxSize)
                await TryCacheEvict().CAF();

            // cannot add if the cache is full
            if (_evictionPolicy == EvictionPolicy.None && _entries.Count >= _maxSize)
                return false;

            // FIXME why is the cache entry an AsyncLazy and not a plain Lazy of some sort?!
            // plus we keep creating the lazy which in many cases is not appropriate?!
            // also note that GetOrAdd does NOT guarantee that the factory runs only once!!

            ValueTask<NearCacheEntry> CreateCacheEntry(IData keyData)
            {
                // FIXME have to trust caller - uh?
                var ncValue = value; //ToEntryValue(value);
                var entry = CreateEntry(keyData, ncValue);
                Statistics.IncrementEntryCount();
                return new ValueTask<NearCacheEntry>(entry);
            }

            var (added, task) = _entries.TryAdd(keyData, CreateCacheEntry);
            if (!added) return false;

            try
            {
                var entry = await task.CAF();
                if (entry != null) return true;
            }
            catch
            {
                // ignore, remove from the cache
            }

            _entries.TryRemove(keyData);
            return false;
        }

        /// <summary>
        /// Tries to get or add a value in the cache.
        /// </summary>
        /// <param name="keyData">The key data.</param>
        /// <param name="valueFactory">A factory that accepts the key data and returns the value data.</param>
        /// <returns>A tuple containing the success or the call, and the resulting object.</returns>
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
                await TryCacheEvict().CAF();

            // if the cache is full, directly return the un-cached value
            if (_evictionPolicy == EvictionPolicy.None && _entries.Count >= _maxSize && !await _entries.ContainsKey(keyData).CAF())
                return Attempt.Fail(await valueFactory(keyData).CAF());

            async ValueTask<NearCacheEntry> CreateCacheEntry(IData keyData)
            {
                var valueData = await valueFactory(keyData).CAF();
                var cachedValue = ToEntryValue(valueData);
                var entry = CreateEntry(keyData, cachedValue);
                Statistics.IncrementEntryCount();
                return entry;
            }

            // insert into cache
            var (added, task) = _entries.TryAdd(keyData, CreateCacheEntry);
            if (added)
            {
                // we have inserted a new cache entry in the cache - but, if the factory
                // fails, it's not really there and will be removed from the cache - so we
                // need to get the value back and ensure it is ok

                // having doubts as to whether this all does what we want...

                NearCacheEntry entry;
                try
                {
                    entry = await task.CAF();
                }
                catch
                {
                    Invalidate(keyData);
                    throw;
                }

                if (entry.Value != null)
                    return entry.Value;

                Invalidate(keyData);
                return Attempt.Failed;
            }

            // failed to add, was added by another thread?
            (hasEntry, o) = await TryGetValue(keyData).CAF();
            if (hasEntry) return o;

            // otherwise, add the uncached value
            return Attempt.Fail(await valueFactory(keyData).CAF());
        }

        public async ValueTask<Attempt<object>> TryGetValue(IData keyData)
        {
            await TryTtlCleanup().CAF();

            var (hasEntry, entry) = await _entries.TryGetValue(keyData).CAF();
            if (!hasEntry || entry.Value == null) return Attempt.Failed;

            if (IsStaleRead(keyData, entry) || entry.Value == null)
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

        protected virtual NearCacheEntry CreateEntry(IData key, object value)
        {
            var created = Clock.Milliseconds;
            var expires = _timeToLiveMillis > 0 ? created + _timeToLiveMillis : Clock.Never;
            return new NearCacheEntry(key, value, created, expires);
        }

        protected virtual bool IsStaleRead(IData key, NearCacheEntry entry)
        {
            return false;
        }

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

        private async ValueTask TryCacheEvict()
        {
            try
            {
                // only one at a time please
                if (Interlocked.CompareExchange(ref _canEvict, 0, 1) == 0)
                    return;

                await DoCacheEvict().CAF();
            }
            finally
            {
                // make sure to release the lock
                Interlocked.Exchange(ref _canEvict, 1);
            }
        }

        private async ValueTask DoCacheEvict()
        {
            var entries = new SortedSet<NearCacheEntry>(_comparer);
            await foreach (var (_, value) in _entries)
                entries.Add(value);

            //var records = new SortedSet<AsyncLazy<NearCacheEntry>>(_entries.Values, _comparer);
            var evictCount = _entries.Count * EvictionPercentage / 100;

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

            // FIXME wtf?
            if (_entries.Count >= _maxSize)
            {
                await TryCacheEvict().CAF();
            }
        }

        private async ValueTask TryTtlCleanup()
        {
            // run when it is time to run
            if (Clock.Milliseconds < _lastCleanup + CleanupInterval)
                return;

            try
            {
                // only one at a time please
                if (Interlocked.CompareExchange(ref _canCleanUp, 0, 1) == 0)
                    return;

                _lastCleanup = Clock.Milliseconds;

                await DoTtlCleanup().CAF();
            }
            finally
            {
                // make sure to release the lock
                Interlocked.Exchange(ref _canCleanUp, 1);
            }
        }

        private async ValueTask DoTtlCleanup()
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
        private static IComparer<NearCacheEntry> GetComparer(EvictionPolicy policy)
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
    }
}
