using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Hazelcast.Clustering;
using Hazelcast.Configuration;
using Hazelcast.Core;
using Hazelcast.Logging;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.NearCaching
{
    // TODO: document and cleanup + the implementation may be broken (later)
    internal abstract class NearCacheBase
    {
        internal static readonly ILogger Logger = Services.Get.LoggerFactory().CreateLogger<NearCache>();

        internal const long TimeNotSet = -1;
        private const int EvictionPercentage = 20;
        private const int CleanupInterval = 5000;

        private int _canCleanUp;
        private int _canEvict;

        private readonly EvictionPolicy _evictionPolicy;
        private readonly InMemoryFormat _inMemoryFormat;
        protected readonly bool InvalidateOnChange;
        private readonly long _maxIdleMilliseconds;
        protected readonly Cluster _cluster; // TODO make this a proper property

        private readonly int _maxSize;
        private readonly string _name;

        private readonly ConcurrentDictionary<IData, Lazy<NearCacheRecord>> _records;

        private readonly IComparer<Lazy<NearCacheRecord>> _selectedComparer;
        private readonly NearCacheStatistics _stat;
        private readonly long _timeToLiveMillis;

        private long _lastCleanup;
        protected Guid RegistrationId;

        protected NearCacheBase(string name, Cluster cluster, ISerializationService serializationService, NearCacheConfig nearCacheConfig)
        {
            _name = name;
            _cluster = cluster;
            SerializationService = serializationService; // FIXME null check
            _maxSize = nearCacheConfig.GetMaxSize();
            _maxIdleMilliseconds = nearCacheConfig.GetMaxIdleSeconds() * 1000;
            _inMemoryFormat = nearCacheConfig.GetInMemoryFormat();
            _timeToLiveMillis = nearCacheConfig.GetTimeToLiveSeconds() * 1000;
            _evictionPolicy =
                (EvictionPolicy)Enum.Parse(typeof(EvictionPolicy), nearCacheConfig.GetEvictionPolicy(), true);
            _records = new ConcurrentDictionary<IData, Lazy<NearCacheRecord>>();
            _canCleanUp = 1;
            _canEvict = 1;
            _lastCleanup = Clock.Milliseconds;
            _selectedComparer = GetComparer(_evictionPolicy);
            _stat = new NearCacheStatistics();
            InvalidateOnChange = nearCacheConfig.IsInvalidateOnChange();
        }

        protected ISerializationService SerializationService { get; }

        public ConcurrentDictionary<IData, Lazy<NearCacheRecord>> Records => _records;

        public string Name => _name;

        public NearCacheStatistics NearCacheStatistics => _stat;

        public void Clear()
        {
            _records.Clear();
            _stat.EntryCount = 0L;
        }

        public bool ContainsKey(IData keyData)
        {
            return TryGetValue(keyData, out _);
        }

        public virtual void Destroy()
        {
            if (RegistrationId != null)
            {
                _cluster.UnsubscribeAsync(RegistrationId).Wait(); // FIXME ASYNC!
            }
            _records.Clear();
        }

        public abstract void Init();

        public void Invalidate(IData key)
        {
            if (_records.TryRemove(key, out _))
            {
                _stat.DecrementEntryCount();
            }
        }

        public void InvalidateAll()
        {
            _records.Clear();
        }

        public bool Remove(IData key)
        {
            Lazy<NearCacheRecord> removed;
            if (_records.TryRemove(key, out removed))
            {
                _stat.DecrementEntryCount();
                return true;
            }
            return false;
        }

        public bool TryAdd(IData keyData, object value)
        {
            //TODO this method is not thread safe yet
            var lazyValue = new Lazy<NearCacheRecord>(() =>
            {
                var ncValue = ConvertToRecordValue(value);
                var nearCacheRecord = CreateRecord(keyData, ncValue);
                _stat.IncrementEntryCount();
                return nearCacheRecord;
            }, LazyThreadSafetyMode.ExecutionAndPublication);

            if (_evictionPolicy == EvictionPolicy.None && _records.Count >= _maxSize)
            {
                return false;
            }
            if (_evictionPolicy != EvictionPolicy.None && _records.Count >= _maxSize)
            {
                FireEvictCache();
            }
            if (_records.TryAdd(keyData, lazyValue))
            {
                var record = lazyValue.Value;
                value = record.Value;
                if (value == null)
                {
                    _records.TryRemove(keyData, out lazyValue);
                    return false;
                }
                return true;
            }
            return false;
        }

        public bool TryGetOrAdd(IData keyData, Func<IData, object> remoteCall, out object value)
        {
            if (TryGetValue(keyData, out value))
            {
                return true;
            }
            _stat.IncrementMiss();
            var lazyValue = new Lazy<NearCacheRecord>(() =>
            {
                var valueData = remoteCall(keyData);
                var ncValue = ConvertToRecordValue(valueData);
                var nearCacheRecord = CreateRecord(keyData, ncValue);
                _stat.IncrementEntryCount();
                return nearCacheRecord;
            }, LazyThreadSafetyMode.ExecutionAndPublication);

            if (_evictionPolicy != EvictionPolicy.None && _records.Count >= _maxSize)
            {
                FireEvictCache();
            }
            if (_evictionPolicy == EvictionPolicy.None && _records.Count >= _maxSize && !_records.ContainsKey(keyData))
            {
                value = remoteCall(keyData);
                return false;
            }
            if (_records.TryAdd(keyData, lazyValue))
            {
                var record = lazyValue.Value;
                value = record.Value;
                if (value == null)
                {
                    Invalidate(keyData);
                    return false;
                }
                return true;
            }
            if (TryGetValue(keyData, out value))
            {
                return true;
            }
            value = remoteCall(keyData);
            return false;
        }

        public bool TryGetValue(IData keyData, out object value)
        {
            FireTtlCleanup();
            value = null;
            if (_records.TryGetValue(keyData, out var lazyRecord) && lazyRecord != null)
            {
                var record = lazyRecord.Value;
                if (IsStaleRead(keyData, record) || record.Value == null)
                {
                    Invalidate(keyData);
                    _stat.IncrementMiss();
                    return false;
                }
                if (IsRecordExpired(record))
                {
                    Invalidate(keyData);
                    _stat.IncrementExpiration();
                    return false;
                }
                record.NotifyHit();
                _stat.IncrementHit();
                value = record.Value;
                return true;
            }
            return false;
        }

        protected virtual NearCacheRecord CreateRecord(IData key, object value)
        {
            var now = Clock.Milliseconds;
            return new NearCacheRecord(key, value, now, _timeToLiveMillis > 0 ? now + _timeToLiveMillis : TimeNotSet);
        }

        protected virtual bool IsStaleRead(IData key, NearCacheRecord record)
        {
            return false;
        }

        private object ConvertToRecordValue(object o)
        {
            object value = _inMemoryFormat.Equals(InMemoryFormat.Binary)
                ? SerializationService.ToData(o)
                : SerializationService.ToObject<object>(o);
            return value;
        }

        private void FireEvictCache()
        {
            if (Interlocked.CompareExchange(ref _canEvict, 0, 1) == 1)
            {
                try
                {
                    // TODO: this was a weird way to hopefully control things, but was not implemented?
                    //Client.ExecutionService.Submit(FireEvictCacheFunc);
                    FireEvictCacheFunc();
                }
                catch
                {
                    Interlocked.Exchange(ref _canEvict, 1);
                    throw; // wrap in HazelcastException?
                }
            }
        }

        private void FireEvictCacheFunc()
        {
            try
            {
                var records = new SortedSet<Lazy<NearCacheRecord>>(_records.Values, _selectedComparer);
                var evictSize = _records.Count * EvictionPercentage / 100;
                var i = 0;
                foreach (var record in records)
                {
                    Lazy<NearCacheRecord> removed;
                    if (_records.TryRemove(record.Value.Key, out removed))
                    {
                        _stat.DecrementEntryCount();
                        _stat.IncrementEviction();
                        if (++i > evictSize)
                        {
                            break;
                        }
                    }
                }
            }
            finally
            {
                Interlocked.Exchange(ref _canEvict, 1);
            }

            if (_records.Count >= _maxSize)
            {
                FireEvictCache();
            }
        }

        private void FireTtlCleanup()
        {
            if (Clock.Milliseconds < _lastCleanup + CleanupInterval)
                return;

            if (Interlocked.CompareExchange(ref _canCleanUp, 0, 1) == 0)
                return;

            try
            {
                // TODO: this was a weird way to hopefully control things, but was not implemented?
                //Client.ExecutionService.Submit(FireTtlCleanupFunc);
                FireTtlCleanupFunc();
            }
            catch
            {
                Interlocked.Exchange(ref _canCleanUp, 1);
                throw; // wrap in HazelcastException?
            }
        }

        private void FireTtlCleanupFunc()
        {
            try
            {
                _lastCleanup = Clock.Milliseconds;
                foreach (var entry in _records)
                {
                    if (IsRecordExpired(entry.Value.Value))
                    {
                        Invalidate(entry.Key);
                        _stat.IncrementExpiration();
                    }
                }
            }
            finally
            {
                Interlocked.Exchange(ref _canCleanUp, 1);
            }
        }

        /// <summary>
        /// Gets the record comparer corresponding to an eviction policy.
        /// </summary>
        /// <param name="policy">The eviction policy.</param>
        /// <returns>The record comparer corresponding to the specified eviction policy.</returns>
        private static IComparer<Lazy<NearCacheRecord>> GetComparer(EvictionPolicy policy)
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
        /// <param name="record">The record.</param>
        /// <returns>true if the record has expired; false otherwise.</returns>
        private bool IsRecordExpired(NearCacheRecord record)
        {
            var now = Clock.Milliseconds;
            return record.IsExpiredAt(now) || record.IsIdleAt(_maxIdleMilliseconds, now);
        }
    }
}
