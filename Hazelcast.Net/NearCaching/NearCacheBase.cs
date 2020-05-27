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
using System.Collections.Concurrent;
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

        private readonly EvictionPolicy _evictionPolicy;
        private readonly InMemoryFormat _inMemoryFormat;
        protected readonly bool InvalidateOnChange;
        protected readonly ILoggerFactory LoggerFactory;
        private readonly ILogger _logger;
        private readonly long _maxIdleMilliseconds;

        private readonly int _maxSize;

        private readonly IComparer<AsyncLazy<NearCacheEntry>> _comparer;
        private readonly long _timeToLiveMillis;

        private long _lastCleanup;
        protected Guid RegistrationId;

        protected NearCacheBase(string name, Cluster cluster, ISerializationService serializationService, ILoggerFactory loggerFactory, NearCacheConfiguration nearCacheConfiguration)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(name));
            Name = name;
            Cluster = cluster ?? throw new ArgumentNullException(nameof(cluster));
            SerializationService = serializationService ?? throw new ArgumentNullException(nameof(serializationService));

            Entries = new ConcurrentDictionary<IData, AsyncLazy<NearCacheEntry>>();
            Statistics = new NearCacheStatistics();

            _canCleanUp = 1;
            _canEvict = 1;
            _lastCleanup = Clock.Milliseconds;

            _maxSize = nearCacheConfiguration.MaxSize;
            _maxIdleMilliseconds = nearCacheConfiguration.MaxIdleSeconds * 1000;
            _inMemoryFormat = nearCacheConfiguration.InMemoryFormat;
            _timeToLiveMillis = nearCacheConfiguration.TimeToLiveSeconds * 1000;
            _evictionPolicy = nearCacheConfiguration.EvictionPolicy;
            _comparer = GetComparer(_evictionPolicy);
            InvalidateOnChange = nearCacheConfiguration.InvalidateOnChange;

            LoggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<NearCacheBase>();
        }

        protected Cluster Cluster { get; }

        protected ISerializationService SerializationService { get; }

        public ConcurrentDictionary<IData, AsyncLazy<NearCacheEntry>> Entries { get; }

        public string Name { get; }

        public NearCacheStatistics Statistics { get; }

        public void Clear()
        {
            Entries.Clear();
            Statistics.EntryCount = 0L;
        }

        public bool ContainsKey(IData keyData)
        {
            return TryGetValue(keyData, out _);
        }

        public virtual void Destroy()
        {
            if (RegistrationId != null)
            {
                Cluster.RemoveSubscriptionAsync(RegistrationId, CancellationToken.None).AsTask().Wait(); // FIXME ASYNC OOPS!
            }
            Entries.Clear();
        }

        public abstract void Init();

        public void Invalidate(IData key)
        {
            if (Entries.TryRemove(key, out _))
            {
                Statistics.DecrementEntryCount();
            }
        }

        public void InvalidateAll()
        {
            Entries.Clear();
        }

        public bool Remove(IData key)
        {
            if (Entries.TryRemove(key, out _))
            {
                Statistics.DecrementEntryCount();
                return true;
            }
            return false;
        }

        public async Task<bool> TryAdd(IData keyData, object value)
        {
            // kick eviction policy if needed
            if (_evictionPolicy != EvictionPolicy.None && Entries.Count >= _maxSize)
                TryCacheEvict();

            // cannot add if the cache is full
            if (_evictionPolicy == EvictionPolicy.None && Entries.Count >= _maxSize)
                return false;

            // prepare the cache entry
            var lazyEntry = new AsyncLazy<NearCacheEntry>(async () =>
            {
                // FIXME have to trust caller - uh?
                var ncValue = value; //ToEntryValue(value);
                var entry = CreateEntry(keyData, ncValue);
                Statistics.IncrementEntryCount();
                return entry;
            });

            if (!Entries.TryAdd(keyData, lazyEntry))
                return false;

            // if we added the entry, make sure to create its value too
            var value2 = (await lazyEntry.CreateValueAsync()).Value.CAF();

            if (value2 == null)
            {
                Entries.TryRemove(keyData, out _);
                return false;
            }

            return true;
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
            if (TryGetValue(keyData, out var o))
                return o;

            // count a miss
            Statistics.IncrementMiss();

            // kick eviction policy if needed
            if (_evictionPolicy != EvictionPolicy.None && Entries.Count >= _maxSize)
                TryCacheEvict();

            // if the cache is full, directly return the un-cached value
            if (_evictionPolicy == EvictionPolicy.None && Entries.Count >= _maxSize && !Entries.ContainsKey(keyData))
                return Attempt.Fail(await valueFactory(keyData)).CAF();

            // prepare the cache entry
            var lazyEntry = new AsyncLazy<NearCacheEntry>(async () =>
            {
                var valueData = await valueFactory(keyData).CAF();
                var cachedValue = ToEntryValue(valueData);
                var entry = CreateEntry(keyData, cachedValue);
                Statistics.IncrementEntryCount();
                return entry;
            });

            // insert into cache
            // concurrency is managed by the concurrent dictionary: there will be only one lazy record at a time,
            // and only once will TryAdd succeed and CreateValueAsync be invoked, and others will wait
            if (Entries.TryAdd(keyData, lazyEntry))
            {
                NearCacheEntry entry;
                try
                {
                    entry = await lazyEntry.CreateValueAsync().CAF();
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
            if (TryGetValue(keyData, out o))
                return o;

            // otherwise, add the uncached value
            return Attempt.Fail(await valueFactory(keyData)).CAF();
        }

        public bool TryGetValue(IData keyData, out object value)
        {
            TryTtlCleanup();

            value = null;

            if (!Entries.TryGetValue(keyData, out var lazyEntry) || lazyEntry == null)
                return false;

            var entry = lazyEntry.Value;

            if (IsStaleRead(keyData, entry) || entry.Value == null)
            {
                Invalidate(keyData);
                Statistics.IncrementMiss();
                return false;
            }

            if (IsRecordExpired(entry))
            {
                Invalidate(keyData);
                Statistics.IncrementExpiration();
                return false;
            }

            entry.NotifyHit();
            Statistics.IncrementHit();

            value = entry.Value;
            return true;
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

        private void TryCacheEvict()
        {
            try
            {
                // only one at a time please
                if (Interlocked.CompareExchange(ref _canEvict, 0, 1) == 0)
                    return;

                DoCacheEvict();
            }
            catch
            {
                // make sure to release the lock
                Interlocked.Exchange(ref _canEvict, 1);
                throw;
            }
        }

        private void DoCacheEvict()
        {
            var records = new SortedSet<AsyncLazy<NearCacheEntry>>(Entries.Values, _comparer);
            var evictCount = Entries.Count * EvictionPercentage / 100;

            var count = 0;

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var lazyRecord in records)
            {
                var record = lazyRecord.Value;

                if (!Entries.TryRemove(record.Key, out _))
                    continue;

                Statistics.DecrementEntryCount();
                Statistics.IncrementEviction();
                if (++count > evictCount)
                    break;
            }

            // FIXME wtf?
            if (Entries.Count >= _maxSize)
            {
                TryCacheEvict();
            }
        }

        private void TryTtlCleanup()
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

                DoTtlCleanup();
            }
            catch
            {
                // make sure to release the lock
                Interlocked.Exchange(ref _canCleanUp, 1);
                throw;
            }
        }

        private void DoTtlCleanup()
        {
            foreach (var (key, lazyRecord) in Entries)
            {
                var record = lazyRecord.Value;

                if (IsRecordExpired(record))
                {
                    Invalidate(key);
                    Statistics.IncrementExpiration();
                }
            }
        }

        /// <summary>
        /// Gets the record comparer corresponding to an eviction policy.
        /// </summary>
        /// <param name="policy">The eviction policy.</param>
        /// <returns>The record comparer corresponding to the specified eviction policy.</returns>
        private static IComparer<AsyncLazy<NearCacheEntry>> GetComparer(EvictionPolicy policy)
        {
            switch (policy)
            {
                case EvictionPolicy.Lfu:
                    return new LfuComparer();

                case EvictionPolicy.Lru:
                    return new LruComparer();

                case EvictionPolicy.None:
                    return new DefaultComparer();

                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Determines whether a record has expired.
        /// </summary>
        /// <param name="entry">The record.</param>
        /// <returns>true if the record has expired; false otherwise.</returns>
        private bool IsRecordExpired(NearCacheEntry entry)
        {
            var now = Clock.Milliseconds;
            return entry.IsExpiredAt(now) || entry.IsIdleAt(_maxIdleMilliseconds, now);
        }
    }
}
