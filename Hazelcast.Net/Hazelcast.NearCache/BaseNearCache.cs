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
using Hazelcast.Client;
using Hazelcast.Config;
using Hazelcast.IO.Serialization;
using Hazelcast.Logging;
using Hazelcast.Net.Ext;
using Hazelcast.Util;

namespace Hazelcast.NearCache
{
    internal abstract class BaseNearCache
    {
        internal static readonly ILogger Logger = Logging.Logger.GetLogger(typeof(NearCache));
        
        internal const long TimeNotSet = -1;
        private const int EvictionPercentage = 20;
        private const int CleanupInterval = 5000;

        private readonly AtomicBoolean _canCleanUp;
        private readonly AtomicBoolean _canEvict;
        protected readonly HazelcastClient Client;

        private readonly EvictionPolicy _evictionPolicy;
        private readonly InMemoryFormat _inMemoryFormat;
        protected readonly bool InvalidateOnChange;
        private readonly long _maxIdleMillis;

        private readonly int _maxSize;
        private readonly string _name;

        private readonly ConcurrentDictionary<IData, Lazy<NearCacheRecord>> _records;

        private readonly IComparer<Lazy<NearCacheRecord>> _selectedComparer;
        private readonly NearCacheStatistics _stat;
        private readonly long _timeToLiveMillis;

        private long _lastCleanup;
        protected Guid RegistrationId;

        protected BaseNearCache(string name, HazelcastClient client, NearCacheConfig nearCacheConfig)
        {
            _name = name;
            Client = client;
            _maxSize = nearCacheConfig.MaxSize;
            _maxIdleMillis = nearCacheConfig.MaxIdleSeconds * 1000;
            _inMemoryFormat = nearCacheConfig.InMemoryFormat;
            _timeToLiveMillis = nearCacheConfig.TimeToLiveSeconds * 1000;
            _evictionPolicy = nearCacheConfig.EvictionPolicy;
            _records = new ConcurrentDictionary<IData, Lazy<NearCacheRecord>>();
            _canCleanUp = new AtomicBoolean(true);
            _canEvict = new AtomicBoolean(true);
            _lastCleanup = Clock.CurrentTimeMillis();
            _selectedComparer = GetComparer(_evictionPolicy);
            _stat = new NearCacheStatistics();
            InvalidateOnChange = nearCacheConfig.InvalidateOnChange;
        }

        public ConcurrentDictionary<IData, Lazy<NearCacheRecord>> Records
        {
            get { return _records; }
        }

        public string Name
        {
            get { return _name; }
        }

        public NearCacheStatistics NearCacheStatistics
        {
            get { return _stat; }
        }

        public void Clear()
        {
            _records.Clear();
            _stat.OwnedEntryCount = 0L;
        }

        public bool ContainsKey(IData keyData)
        {
            object ignored;
            return TryGetValue(keyData, out ignored);
        }

        public virtual void Destroy()
        {
            if (RegistrationId != null)
            {
                Client.ListenerService.DeregisterListener(RegistrationId);
            }
            _records.Clear();
        }

        public abstract void Init();

        public void Invalidate(IData key)
        {
            if (_records.TryRemove(key, out _))
            {
                _stat.DecrementOwnedEntryCount();
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
                _stat.DecrementOwnedEntryCount();
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
                _stat.IncrementOwnedEntryCount();
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
                _stat.IncrementOwnedEntryCount();
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
                record.Access();
                _stat.IncrementHit();
                value = record.Value;
                return true;
            }
            return false;
        }

        protected virtual NearCacheRecord CreateRecord(IData key, object value)
        {
            var now = Clock.CurrentTimeMillis();
            return new NearCacheRecord(key, value, now, _timeToLiveMillis > 0 ? now + _timeToLiveMillis : TimeNotSet);
        }

        protected virtual bool IsStaleRead(IData key, NearCacheRecord record)
        {
            return false;
        }

        private object ConvertToRecordValue(object Object)
        {
            object value = _inMemoryFormat.Equals(InMemoryFormat.Binary)
                ? Client.SerializationService.ToData(Object)
                : Client.SerializationService.ToObject<object>(Object);
            return value;
        }


        private void FireEvictCache()
        {
            if (_canEvict.CompareAndSet(true, false))
            {
                try
                {
                    Client.ExecutionService.Submit(FireEvictCacheFunc);
                }
                catch (Exception e)
                {
                    _canEvict.Set(true);
                    throw ExceptionUtil.Rethrow(e);
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
                        _stat.DecrementOwnedEntryCount();
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
                _canEvict.Set(true);
            }

            if (_records.Count >= _maxSize)
            {
                FireEvictCache();
            }
        }

        private void FireTtlCleanup()
        {
            if (Clock.CurrentTimeMillis() < (_lastCleanup + CleanupInterval))
            {
                return;
            }

            if (_canCleanUp.CompareAndSet(true, false))
            {
                try
                {
                    Client.ExecutionService.Submit(FireTtlCleanupFunc);
                }
                catch (Exception e)
                {
                    _canCleanUp.Set(true);
                    throw ExceptionUtil.Rethrow(e);
                }
            }
        }

        private void FireTtlCleanupFunc()
        {
            try
            {
                _lastCleanup = Clock.CurrentTimeMillis();
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
                _canCleanUp.Set(true);
            }
        }

        private IComparer<Lazy<NearCacheRecord>> GetComparer(EvictionPolicy policy)
        {
            switch (policy)
            {
                case EvictionPolicy.Lfu:
                    return new LfuComparer();
                case EvictionPolicy.Lru:
                    return new LruComparer();
            }

            return new DefaultComparer();
        }

        private bool IsRecordExpired(NearCacheRecord record)
        {
            var now = Clock.CurrentTimeMillis();
            return record.IsExpiredAt(now) || record.IsIdleAt(_maxIdleMillis, now);
        }

        private class LruComparer : IComparer<Lazy<NearCacheRecord>>
        {
            public int Compare(Lazy<NearCacheRecord> x, Lazy<NearCacheRecord> y)
            {
                var c = x.Value.LastAccessTime.CompareTo(y.Value.LastAccessTime);
                if (c != 0) return c;

                return x.Value.Key.GetHashCode().CompareTo(y.Value.Key.GetHashCode());
            }
        }

        private class LfuComparer : IComparer<Lazy<NearCacheRecord>>
        {
            public int Compare(Lazy<NearCacheRecord> x, Lazy<NearCacheRecord> y)
            {
                var c = x.Value.Hit.Get().CompareTo(y.Value.Hit.Get());
                if (c != 0) return c;

                return x.Value.Key.GetHashCode().CompareTo(y.Value.Key.GetHashCode());
            }
        }

        private class DefaultComparer : IComparer<Lazy<NearCacheRecord>>
        {
            public int Compare(Lazy<NearCacheRecord> x, Lazy<NearCacheRecord> y)
            {
                return x.Value.Key.GetHashCode().CompareTo(y.Value.Key.GetHashCode());
            }
        }
    }
}