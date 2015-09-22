using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Client.Spi;
using Hazelcast.Config;
using Hazelcast.IO.Serialization;
using Hazelcast.Logging;
using Hazelcast.Net.Ext;
using Hazelcast.Util;

namespace Hazelcast.Client
{
    public enum ClientNearCacheType
    {
        Map,
        ReplicatedMap
    }

    internal class ClientNearCache
    {
        internal const int evictionPercentage = 20;

        internal const int cleanupInterval = 5000;
        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof (ClientNearCache));
        public static readonly object NullObject = new object();
        internal readonly ConcurrentDictionary<IData, CacheRecord> cache;
        internal readonly ClientNearCacheType cacheType;
        internal readonly AtomicBoolean canCleanUp;

        internal readonly AtomicBoolean canEvict;
        internal readonly ClientContext context;

        internal readonly EvictionPolicy evictionPolicy;

        internal readonly InMemoryFormat inMemoryFormat;
        internal readonly bool invalidateOnChange;

        internal readonly string mapName;
        internal readonly long maxIdleMillis;
        internal readonly int maxSize;
        internal readonly long timeToLiveMillis;
        internal long lastCleanup;
        internal string registrationId = null;

        public ClientNearCache(string mapName, ClientNearCacheType cacheType, ClientContext context,
            NearCacheConfig nearCacheConfig)
        {
            this.mapName = mapName;
            this.cacheType = cacheType;
            this.context = context;
            maxSize = nearCacheConfig.GetMaxSize();
            maxIdleMillis = nearCacheConfig.GetMaxIdleSeconds()*1000;
            inMemoryFormat = nearCacheConfig.GetInMemoryFormat();
            timeToLiveMillis = nearCacheConfig.GetTimeToLiveSeconds()*1000;
            invalidateOnChange = nearCacheConfig.IsInvalidateOnChange();
            evictionPolicy = (EvictionPolicy) Enum.Parse(typeof (EvictionPolicy), nearCacheConfig.GetEvictionPolicy());
            cache = new ConcurrentDictionary<IData, CacheRecord>();
            canCleanUp = new AtomicBoolean(true);
            canEvict = new AtomicBoolean(true);
            lastCleanup = Clock.CurrentTimeMillis();
            if (invalidateOnChange)
            {
                AddInvalidateListener();
            }
        }


        private void AddInvalidateListener()
        {
            try
            {
                IClientMessage request = null;
                DistributedEventHandler handler = null;
                if (cacheType == ClientNearCacheType.Map)
                {
                    request = MapAddNearCacheEntryListenerCodec.EncodeRequest(mapName, false);

                    handler = message =>
                    {
                        MapAddNearCacheEntryListenerCodec.AbstractEventHandler.Handle(message,
                            (key, value, oldValue, mergingValue, type, uuid, entries) =>
                            {
                                CacheRecord removed;
                                cache.TryRemove(key, out removed);
                            });
                    };
                }
                else
                {
                    throw new NotImplementedException("Near cache is not available for this type of data structure");
                }

                registrationId = context.GetListenerService().StartListening(request,
                    handler, m => MapAddNearCacheEntryListenerCodec.DecodeResponse(m).response);
            }
            catch (Exception e)
            {
                Logger.Severe("-----------------\n Near Cache is not initialized!!! \n-----------------", e);
            }
        }


        public virtual void Put(IData key, object @object)
        {
            FireTtlCleanup();
            if (evictionPolicy == EvictionPolicy.None && cache.Count >= maxSize)
            {
                return;
            }
            if (evictionPolicy != EvictionPolicy.None && cache.Count >= maxSize)
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
                value = inMemoryFormat.Equals(InMemoryFormat.Binary)
                    ? context.GetSerializationService().ToData(@object)
                    : context.GetSerializationService().ToObject<object>(@object);
            }
            cache.TryAdd(key, new CacheRecord(this, key, value));
        }

        private void FireEvictCache()
        {
            if (canEvict.CompareAndSet(true, false))
            {
                try
                {
                    Task.Factory.StartNew(FireEvictCacheFunc);
                }
                catch (Exception e)
                {
                    canEvict.Set(true);
                    throw ExceptionUtil.Rethrow(e);
                }
            }
        }

        private void FireEvictCacheFunc()
        {
            try
            {
                var records = new SortedSet<CacheRecord>();
                int evictSize = cache.Count*evictionPercentage/100;
                int i = 0;
                foreach (CacheRecord record in records)
                {
                    CacheRecord removed;
                    cache.TryRemove(record.key, out removed);
                    if (++i > evictSize)
                    {
                        break;
                    }
                }
            }
            finally
            {
                canEvict.Set(true);
            }
        }

        private void FireTtlCleanup()
        {
            if (Clock.CurrentTimeMillis() < (lastCleanup + cleanupInterval))
            {
                return;
            }
            if (canCleanUp.CompareAndSet(true, false))
            {
                try
                {
                    Task.Factory.StartNew(FireTtlCleanupFunc);
                }
                catch (Exception e)
                {
                    canCleanUp.Set(true);
                    throw ExceptionUtil.Rethrow(e);
                }
            }
        }

        private void FireTtlCleanupFunc()
        {
            try
            {
                lastCleanup = Clock.CurrentTimeMillis();
                foreach (var entry in cache)
                {
                    if (entry.Value.Expired())
                    {
                        CacheRecord removed;
                        cache.TryRemove(entry.Key, out removed);
                    }
                }
            }
            finally
            {
                canCleanUp.Set(true);
            }
        }

        public virtual object Get(IData key)
        {
            FireTtlCleanup();
            CacheRecord record = null;
            cache.TryGetValue(key, out record);
            if (record != null)
            {
                record.Access();
                if (record.Expired())
                {
                    Invalidate(key);
                    return null;
                }
                if (record.value.Equals(NullObject))
                {
                    return NullObject;
                }
                return inMemoryFormat.Equals(InMemoryFormat.Binary)
                    ? context.GetSerializationService().ToObject<object>(record.value)
                    : record.value;
            }
            return null;
        }

        public virtual void Invalidate(IData key)
        {
            try
            {
                CacheRecord record = null;
                cache.TryRemove(key, out record);
            }
            catch (ArgumentNullException e)
            {
            }
        }
        public virtual void InvalidateAll()
        {
            cache.Clear();
        }

        public virtual void Destroy()
        {
            if (registrationId != null)
            {
                if (cacheType == ClientNearCacheType.Map)
                {
                    context.GetListenerService().StopListening(
                        s => MapRemoveEntryListenerCodec.EncodeRequest(mapName, s),
                        m => MapRemoveEntryListenerCodec.DecodeResponse(m).response, registrationId);
                }
                else if (cacheType == ClientNearCacheType.ReplicatedMap)
                {
                    //TODO REPLICATED MAP
                    throw new NotImplementedException();
                }
                else
                {
                    throw new NotImplementedException("Near cache is not available for this type of data structure");
                }
            }
            cache.Clear();
        }


        internal class CacheRecord : IComparable<CacheRecord>
        {
            private readonly ClientNearCache _enclosing;
            internal readonly long creationTime;

            internal readonly AtomicInteger hit;
            internal readonly IData key;

            internal readonly object value;

            //TODO volatile
            internal long lastAccessTime;

            internal CacheRecord(ClientNearCache _enclosing, IData key, object value)
            {
                this._enclosing = _enclosing;
                this.key = key;
                this.value = value;
                long time = Clock.CurrentTimeMillis();
                lastAccessTime = time;
                creationTime = time;
                hit = new AtomicInteger(0);
            }

            public virtual int CompareTo(CacheRecord o)
            {
                if (EvictionPolicy.Lru.Equals(_enclosing.evictionPolicy))
                {
                    return lastAccessTime.CompareTo((o.lastAccessTime));
                }
                if (EvictionPolicy.Lfu.Equals(_enclosing.evictionPolicy))
                {
                    return hit.Get().CompareTo((o.hit.Get()));
                }
                return 0;
            }

            internal virtual void Access()
            {
                hit.IncrementAndGet();
                lastAccessTime = Clock.CurrentTimeMillis();
            }

            internal virtual bool Expired()
            {
                long time = Clock.CurrentTimeMillis();
                return (_enclosing.maxIdleMillis > 0 && time > lastAccessTime + _enclosing.maxIdleMillis) ||
                       (_enclosing.timeToLiveMillis > 0 && time > creationTime + _enclosing.timeToLiveMillis);
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