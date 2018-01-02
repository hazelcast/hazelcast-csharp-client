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
    internal enum ClientNearCacheType
    {
        Map,
        ReplicatedMap
    }

    internal class ClientNearCache
    {
        internal const int EvictionPercentage = 20;
        internal const int CleanupInterval = 5000;
        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof (ClientNearCache));
        public static readonly object NullObject = new object();

        private readonly ClientNearCacheType _cacheType;
        private readonly AtomicBoolean _canCleanUp;
        private readonly AtomicBoolean _canEvict;
        private readonly ClientContext _context;
        private readonly EvictionPolicy _evictionPolicy;
        private readonly InMemoryFormat _inMemoryFormat;
        private readonly string _mapName;
        private readonly long _maxIdleMillis;
        private readonly int _maxSize;
        private readonly IComparer<CacheRecord> _selectedComparer;
        private readonly long _timeToLiveMillis;

        internal readonly ConcurrentDictionary<IData, CacheRecord> Cache;
        internal readonly bool InvalidateOnChange;

        private long _lastCleanup;
        private string _registrationId;

        public ClientNearCache(string mapName, ClientNearCacheType cacheType, ClientContext context,
            NearCacheConfig nearCacheConfig)
        {
            _mapName = mapName;
            _cacheType = cacheType;
            _context = context;
            _maxSize = nearCacheConfig.GetMaxSize();
            _maxIdleMillis = nearCacheConfig.GetMaxIdleSeconds()*1000;
            _inMemoryFormat = nearCacheConfig.GetInMemoryFormat();
            _timeToLiveMillis = nearCacheConfig.GetTimeToLiveSeconds()*1000;
            InvalidateOnChange = nearCacheConfig.IsInvalidateOnChange();
            _evictionPolicy = (EvictionPolicy) Enum.Parse(typeof (EvictionPolicy), nearCacheConfig.GetEvictionPolicy());
            Cache = new ConcurrentDictionary<IData, CacheRecord>();
            _canCleanUp = new AtomicBoolean(true);
            _canEvict = new AtomicBoolean(true);
            _lastCleanup = Clock.CurrentTimeMillis();
            _selectedComparer = GetComparer(_evictionPolicy);
            if (InvalidateOnChange)
            {
                AddInvalidateListener();
            }
        }

        public virtual void Destroy()
        {
            if (_registrationId != null)
            {
                if (_cacheType == ClientNearCacheType.Map)
                {
                    _context.GetListenerService().DeregisterListener(_registrationId,
                        s => MapRemoveEntryListenerCodec.EncodeRequest(_mapName, s)
                    );
                }
                else if (_cacheType == ClientNearCacheType.ReplicatedMap)
                {
                    //TODO REPLICATED MAP
                    throw new NotImplementedException();
                }
                else
                {
                    throw new NotImplementedException("Near cache is not available for this type of data structure");
                }
            }
            Cache.Clear();
        }

        public virtual object Get(IData key)
        {
            FireTtlCleanup();
            CacheRecord record;
            Cache.TryGetValue(key, out record);
            if (record != null)
            {
                record.Access();
                if (record.Expired())
                {
                    Invalidate(key);
                    return null;
                }
                if (record.Value.Equals(NullObject))
                {
                    return NullObject;
                }
                return _inMemoryFormat.Equals(InMemoryFormat.Binary)
                    ? _context.GetSerializationService().ToObject<object>(record.Value)
                    : record.Value;
            }
            return null;
        }

        public virtual void InvalidateAll()
        {
            Cache.Clear();
        }

        public virtual void Put(IData key, object @object)
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
                    ? _context.GetSerializationService().ToData(@object)
                    : _context.GetSerializationService().ToObject<object>(@object);
            }
            Cache.TryAdd(key, new CacheRecord(this, key, value));
        }

        private void AddInvalidateListener()
        {
            try
            {
                IClientMessage request;
                DistributedEventHandler handler;
                if (_cacheType == ClientNearCacheType.Map)
                {
                    request = MapAddNearCacheEntryListenerCodec.EncodeRequest(_mapName, (int) EntryEventType.Invalidation,
                        false);

                    handler = message
                        => MapAddNearCacheEntryListenerCodec.AbstractEventHandler.Handle(message, HandleIMapInvalidation, HandleIMapBatchInvalidation);
                }
                else
                {
                    throw new NotImplementedException("Near cache is not available for this type of data structure");
                }
                
                _registrationId = _context.GetListenerService()
                    .RegisterListener(request, message => MapAddNearCacheEntryListenerCodec.DecodeResponse(message).response,
                        id => MapRemoveEntryListenerCodec.EncodeRequest(_mapName, id), handler);
            }
            catch (Exception e)
            {
                Logger.Severe("-----------------\n Near Cache is not initialized!!! \n-----------------", e);
            }
        }

        private void HandleIMapBatchInvalidation(IList<IData> keys, IList<string> sourceUuids, IList<Guid> partitionUuids, IList<long> sequences)
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
                    _context.GetExecutionService().Submit(FireEvictCacheFunc);
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
                var evictSize = Cache.Count*EvictionPercentage/100;
                var i = 0;
                foreach (var record in records)
                {
                    CacheRecord removed;
                    Cache.TryRemove(record.Key, out removed);
                    if (++i > evictSize)
                    {
                        break;
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
                    _context.GetExecutionService().Submit(FireTtlCleanupFunc);
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
                    if (entry.Value.Expired())
                    {
                        CacheRecord removed;
                        Cache.TryRemove(entry.Key, out removed);
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
            Cache.TryRemove(key, out removed);
        }

        internal class CacheRecord
        {
            private readonly ClientNearCache _enclosing;
            internal readonly long CreationTime;
            internal readonly AtomicInteger Hit;
            internal readonly IData Key;
            internal readonly object Value;
            //TODO volatile
            internal long LastAccessTime;

            internal CacheRecord(ClientNearCache enclosing, IData key, object value)
            {
                _enclosing = enclosing;
                Key = key;
                Value = value;
                var time = Clock.CurrentTimeMillis();
                LastAccessTime = time;
                CreationTime = time;
                Hit = new AtomicInteger(0);
            }

            internal virtual void Access()
            {
                Hit.IncrementAndGet();
                LastAccessTime = Clock.CurrentTimeMillis();
            }

            internal virtual bool Expired()
            {
                var time = Clock.CurrentTimeMillis();
                return (_enclosing._maxIdleMillis > 0 && time > LastAccessTime + _enclosing._maxIdleMillis) ||
                       (_enclosing._timeToLiveMillis > 0 && time > CreationTime + _enclosing._timeToLiveMillis);
            }
        }

        internal class LruComparer : IComparer<CacheRecord>
        {
            public int Compare(CacheRecord x, CacheRecord y)
            {
                var c = x.LastAccessTime.CompareTo(y.LastAccessTime);
                if (c != 0) return c;

                return x.Key.GetHashCode().CompareTo(y.Key.GetHashCode());
            }
        }

        internal class LfuComparer : IComparer<CacheRecord>
        {
            public int Compare(CacheRecord x, CacheRecord y)
            {
                var c = x.Hit.Get().CompareTo(y.Hit.Get());
                if (c != 0) return c;

                return x.Key.GetHashCode().CompareTo(y.Key.GetHashCode());
            }
        }

        internal class DefaultComparer : IComparer<CacheRecord>
        {
            public int Compare(CacheRecord x, CacheRecord y)
            {
                return x.Key.GetHashCode().CompareTo(y.Key.GetHashCode());
            }
        }

        internal enum EvictionPolicy
        {
            None,
            Lru,
            Lfu
        }
    }
}