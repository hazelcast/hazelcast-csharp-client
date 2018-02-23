// Copyright (c) 2008-2017, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Client.Spi;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;
using Hazelcast.Logging;
using Hazelcast.Net.Ext;
using Hazelcast.Util;

namespace Hazelcast.Client
{
    internal class NearCache
    {
        internal const long TimeNotSet = -1;
        internal const int EvictionPercentage = 20;
        internal const int CleanupInterval = 5000;
        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof(NearCache));
        public static readonly object NullObject = new object();

        private readonly string _name;
        private readonly HazelcastClient _client;

        private readonly AtomicBoolean _canCleanUp;
        private readonly AtomicBoolean _canEvict;

        private readonly EvictionPolicy _evictionPolicy;
        private readonly InMemoryFormat _inMemoryFormat;
        private readonly long _maxIdleMillis;

        private readonly int _maxSize;

        private readonly IComparer<CacheRecord> _selectedComparer;
        private readonly long _timeToLiveMillis;

        internal readonly ConcurrentDictionary<IData, CacheRecord> Cache;
        internal readonly bool InvalidateOnChange;

        private long _lastCleanup;
        private string _registrationId;
        private readonly NearCacheStatistics _stat;

        public NearCache(string name, HazelcastClient client, NearCacheConfig nearCacheConfig)
        {
            _name = name;
            _client = client;
            _maxSize = nearCacheConfig.GetMaxSize();
            _maxIdleMillis = nearCacheConfig.GetMaxIdleSeconds() * 1000;
            _inMemoryFormat = nearCacheConfig.GetInMemoryFormat();
            _timeToLiveMillis = nearCacheConfig.GetTimeToLiveSeconds() * 1000;
            InvalidateOnChange = nearCacheConfig.IsInvalidateOnChange();
            _evictionPolicy = (EvictionPolicy) Enum.Parse(typeof(EvictionPolicy), nearCacheConfig.GetEvictionPolicy(), true);
            Cache = new ConcurrentDictionary<IData, CacheRecord>();
            _canCleanUp = new AtomicBoolean(true);
            _canEvict = new AtomicBoolean(true);
            _lastCleanup = Clock.CurrentTimeMillis();
            _selectedComparer = GetComparer(_evictionPolicy);
            _stat = new NearCacheStatistics();
            if (InvalidateOnChange)
            {
                AddInvalidateListener();
            }
        }
        
        public string Name
        {
            get { return _name; }
        }

        public NearCacheStatistics NearCacheStatistics
        {
            get { return _stat; }
        }

        public void Destroy()
        {
            if (_registrationId != null)
            {
                _client.GetListenerService().DeregisterListener(_registrationId,
                    s => MapRemoveEntryListenerCodec.EncodeRequest(_name, s)
                );
            }

            Cache.Clear();
        }

        public object Get(IData key)
        {
            FireTtlCleanup();
            CacheRecord record;
            if(Cache.TryGetValue(key, out record) && record != null)
            {
                record.Access();
                _stat.IncrementHit();
                if (IsRecordExpired(record))
                {
                    Invalidate(key);
                    _stat.IncrementExpiration();
                    return null;
                }

                if (record.Value.Equals(NullObject))
                {
                    return NullObject;
                }

                return _inMemoryFormat.Equals(InMemoryFormat.Binary)
                    ? _client.GetSerializationService().ToObject<object>(record.Value)
                    : record.Value;
            }
            _stat.IncrementMiss();
            return null;
        }

        public void InvalidateAll()
        {
            Cache.Clear();
        }

        public void Put(IData key, object @object)
        {
            FireTtlCleanup();
            if (_evictionPolicy == EvictionPolicy.None && Cache.Count >= _maxSize)
            {
                return;
            }

            if (_evictionPolicy != EvictionPolicy.None && Cache.Count >= _maxSize)
            {
                FireEvictCache();
            }

            object value;
            if (@object == null)
            {
                value = NullObject;
            }
            else
            {
                value = _inMemoryFormat.Equals(InMemoryFormat.Binary)
                    ? _client.GetSerializationService().ToData(@object)
                    : _client.GetSerializationService().ToObject<object>(@object);
            }

            var newRecord = CreateRecord(key, value);
            if (Cache.TryAdd(key, newRecord))
            {
                _stat.IncrementOwnedEntryCount();
            }
            else
            {
                Cache[key] = newRecord;
            }
        }

        private CacheRecord CreateRecord(IData key, object value)
        {
            var now = Clock.CurrentTimeMillis();
            return new CacheRecord(key, value, now, _timeToLiveMillis > 0 ? now + _timeToLiveMillis : TimeNotSet);
        }

        private void AddInvalidateListener()
        {
            try
            {
                IClientMessage request;
                DistributedEventHandler handler;

                request = MapAddNearCacheEntryListenerCodec.EncodeRequest(_name,
                    (int) EntryEventType.Invalidation,
                    false);

                handler = message
                    => MapAddNearCacheEntryListenerCodec.AbstractEventHandler.Handle(message,
                        HandleIMapInvalidation, HandleIMapBatchInvalidation);

                _registrationId = _client.GetListenerService()
                    .RegisterListener(request,
                        message => MapAddNearCacheEntryListenerCodec.DecodeResponse(message).response,
                        id => MapRemoveEntryListenerCodec.EncodeRequest(_name, id), handler);
            }
            catch (Exception e)
            {
                Logger.Severe("-----------------\n Near Cache is not initialized!!! \n-----------------", e);
            }
        }

        private void HandleIMapBatchInvalidation(IList<IData> keys, IList<string> sourceUuids,
            IList<Guid> partitionUuids, IList<long> sequences)
        {
            foreach (var data in keys)
            {
                Invalidate(data);
            }
        }

        private void HandleIMapInvalidation(IData key, string sourceUuid, Guid? partitionUuid, long? sequence)
        {
            if (key == null)
            {
                InvalidateAll();
            }
            else
            {
                Invalidate(key);
            }
        }

        private void FireEvictCache()
        {
            if (_canEvict.CompareAndSet(true, false))
            {
                try
                {
                    _client.GetClientExecutionService().Submit(FireEvictCacheFunc);
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
                var records = new SortedSet<CacheRecord>(Cache.Values, _selectedComparer);
                var evictSize = Cache.Count * EvictionPercentage / 100;
                var i = 0;
                foreach (var record in records)
                {
                    CacheRecord removed;
                    if (Cache.TryRemove(record.Key, out removed))
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

            if (Cache.Count >= _maxSize)
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
                    _client.GetClientExecutionService().Submit(FireTtlCleanupFunc);
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
                foreach (var entry in Cache)
                {
                    if (IsRecordExpired(entry.Value))
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

        private IComparer<CacheRecord> GetComparer(EvictionPolicy policy)
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

        public void Invalidate(IData key)
        {
            CacheRecord removed;
            if (Cache.TryRemove(key, out removed))
            {
                _stat.DecrementOwnedEntryCount();
            }
        }

        private bool IsRecordExpired(CacheRecord record)
        {
            var now = Clock.CurrentTimeMillis();
            return record.IsExpiredAt(now) || record.IsIdleAt(_maxIdleMillis, now);
        }

        internal class CacheRecord
        {
            private readonly long _creationTime;
            private readonly long _expirationTime;
            private long _lastAccessTime;
            internal readonly AtomicInteger Hit;
            
            internal readonly IData Key;
            internal readonly object Value;

            internal CacheRecord(IData key, object value, long creationTime, long expirationTime)
            {
                Key = key;
                Value = value;
                _lastAccessTime = creationTime;
                _creationTime = creationTime;
                _expirationTime = expirationTime;
                Hit = new AtomicInteger(0);
            }

            internal long LastAccessTime
            {
                get { return Interlocked.Read(ref _lastAccessTime); }
            }

            internal void Access()
            {
                Hit.IncrementAndGet();
                Interlocked.Exchange(ref _lastAccessTime, Clock.CurrentTimeMillis());
            }

            internal bool IsIdleAt(long maxIdleMillis, long now)
            {
                if (maxIdleMillis <= 0) return false;
                return LastAccessTime > TimeNotSet
                    ? LastAccessTime + maxIdleMillis < now
                    : _creationTime + maxIdleMillis < now;
            }

            internal bool IsExpiredAt(long now)
            {
                return _expirationTime > TimeNotSet && _expirationTime <= now;
            }
        }

        private class LruComparer : IComparer<CacheRecord>
        {
            public int Compare(CacheRecord x, CacheRecord y)
            {
                var c = x.LastAccessTime.CompareTo(y.LastAccessTime);
                if (c != 0) return c;

                return x.Key.GetHashCode().CompareTo(y.Key.GetHashCode());
            }
        }

        private class LfuComparer : IComparer<CacheRecord>
        {
            public int Compare(CacheRecord x, CacheRecord y)
            {
                var c = x.Hit.Get().CompareTo(y.Hit.Get());
                if (c != 0) return c;

                return x.Key.GetHashCode().CompareTo(y.Key.GetHashCode());
            }
        }

        private class DefaultComparer : IComparer<CacheRecord>
        {
            public int Compare(CacheRecord x, CacheRecord y)
            {
                return x.Key.GetHashCode().CompareTo(y.Key.GetHashCode());
            }
        }

        private enum EvictionPolicy
        {
            None,
            Lru,
            Lfu
        }
    }
}