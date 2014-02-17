using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hazelcast.Client.Request.Base;
using Hazelcast.Client.Request.Map;
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
        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof(ClientNearCache));

        internal const int evictionPercentage = 20;

        internal const int cleanupInterval = 5000;
        public static readonly object NullObject = new object();
        internal readonly ConcurrentDictionary<Data, CacheRecord> cache;
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

        internal readonly ClientNearCacheType cacheType;

        public ClientNearCache(string mapName,ClientNearCacheType cacheType, ClientContext context, NearCacheConfig nearCacheConfig)
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
            cache = new ConcurrentDictionary<Data, CacheRecord>();
            canCleanUp = new AtomicBoolean(true);
            canEvict = new AtomicBoolean(true);
            lastCleanup = Clock.CurrentTimeMillis();
            if (invalidateOnChange)
            {
                AddInvalidateListener();
            }
        }

        
    private void AddInvalidateListener(){
        try {
            ClientRequest request = null;
            DistributedEventHandler handler = null;
            if (cacheType == ClientNearCacheType.Map) 
            {
                request = new MapAddEntryListenerRequest<object,object>(mapName, false);

                handler = _event =>
                {
                    var e = _event as PortableEntryEvent;
                    CacheRecord removed;
                    cache.TryRemove(e.GetKey(),out removed);
                };
                

            } 
            else if (cacheType == ClientNearCacheType.ReplicatedMap) 
            {
                //TODO REPLICATED NEARCACHE

                //request = new ClientReplicatedMapAddEntryListenerRequest(mapName, null, null);
                //handler = new EventHandler<PortableEntryEvent>() {
                //    public void handle(PortableEntryEvent event) {
                //        cache.remove(event.getKey());
                //    }
                //};
            } else {
                throw new NotImplementedException("Near cache is not available for this type of data structure");
            }
            registrationId = ListenerUtil.Listen(context, request, null, handler);
        } catch (Exception e) {
            Logger.Severe("-----------------\n Near Cache is not initialized!!! \n-----------------", e);
        }

    }


        public virtual void Put(Data key, object @object)
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
                    : @object;
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
                var evictSize = cache.Count * evictionPercentage / 100;
                int i=0;
                foreach (var record in records)
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
           try {
                lastCleanup = Clock.CurrentTimeMillis();
                foreach (var entry in cache) {
                    if (entry.Value.Expired())
                    {
                        CacheRecord removed;
                        cache.TryRemove(entry.Key,out removed);
                    }
                }
            } finally {
                canCleanUp.Set(true);
            } 
        }

        public virtual object Get(Data key)
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

        public virtual void Invalidate(Data key)
        {
            CacheRecord record = null;
            try
            {
                cache.TryRemove(key, out record);
            }
            catch (ArgumentNullException e)
            {
            }
        }

        public virtual void Destroy()
        {
            if (registrationId != null)
            {
                ClientRequest request;
                if (cacheType == ClientNearCacheType.Map)
                {
                    request = new MapRemoveEntryListenerRequest(mapName, registrationId);
                }
                else if (cacheType == ClientNearCacheType.ReplicatedMap)
                {
                    //TODO REPLICATED MAP
                    throw new NotImplementedException();
                    //request = new ClientReplicatedMapRemoveEntryListenerRequest(mapName, registrationId);
                }
                else
                {
                    throw new NotImplementedException("Near cache is not available for this type of data structure");
                }
                ListenerUtil.StopListening(context, request, registrationId);
            }
            cache.Clear();
        }




        internal class CacheRecord : IComparable<CacheRecord>
        {
            private readonly ClientNearCache _enclosing;
            internal readonly long creationTime;

            internal readonly AtomicInteger hit;
            internal readonly Data key;

            internal readonly object value;

            //TODO volatile
            internal long lastAccessTime;

            internal CacheRecord(ClientNearCache _enclosing, Data key, object value)
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