using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hazelcast.Client.Request.Map;
using Hazelcast.Client.Spi;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;
using Hazelcast.Logging;
using Hazelcast.Map;
using Hazelcast.Net.Ext;
using Hazelcast.Util;

namespace Hazelcast.Client.Proxy
{
    internal sealed class ClientMapProxy<K, V> : ClientProxy, IMap<K, V>
    {
        private readonly string name;
        private readonly AtomicBoolean nearCacheInitialized = new AtomicBoolean();

        private volatile ClientNearCache nearCache;

        private string nearCacheListenerId;

        public ClientMapProxy(string serviceName, string name) : base(serviceName, name)
        {
            this.name = name;
        }

        public bool ContainsKey(object key)
        {
            Data keyData = ToData(key);
            var request = new MapContainsKeyRequest(name, keyData);
            var result = Invoke<bool>(request, keyData);
            return result;
        }

        public bool ContainsValue(object value)
        {
            Data valueData = ToData(value);
            var request = new MapContainsValueRequest(name, valueData);
            var result = Invoke<bool>(request);
            return result;
        }

        public V Get(object key)
        {
            InitNearCache();
            Data keyData = ToData(key);
            if (nearCache != null)
            {
                object cached = nearCache.Get(keyData);
                if (cached != null)
                {
                    if (cached.Equals(ClientNearCache.NullObject))
                    {
                        return default(V);
                    }
                    return (V) cached;
                }
            }
            var request = new MapGetRequest(name, keyData);
            var result = Invoke<V>(request, keyData);
            if (nearCache != null)
            {
                nearCache.Put(keyData, result);
            }
            return result;
        }

        public V Put(K key, V value)
        {
            return Put(key, value, -1, TimeUnit.SECONDS);
        }

        public V Remove(object key)
        {
            Data keyData = ToData(key);
            var request = new MapRemoveRequest(name, keyData, ThreadUtil.GetThreadId());
            return Invoke<V>(request, keyData);
        }

        public bool Remove(object key, object value)
        {
            Data keyData = ToData(key);
            Data valueData = ToData(value);
            var request = new MapRemoveIfSameRequest(name, keyData, valueData, ThreadUtil.GetThreadId());
            var result = Invoke<bool>(request, keyData);
            return result;
        }

        public void Delete(object key)
        {
            Data keyData = ToData(key);
            var request = new MapDeleteRequest(name, keyData, ThreadUtil.GetThreadId());
            Invoke<object>(request, keyData);
        }

        public void Flush()
        {
            var request = new MapFlushRequest(name);
            Invoke<object>(request);
        }

        public Task<V> GetAsync(K key)
        {
            InitNearCache();
            Data keyData = ToData(key);
            if (nearCache != null)
            {
                object cached = nearCache.Get(keyData);
                if (cached != null)
                {
                    Task<V> task = Task.Factory.StartNew(() =>
                    {
                        if (cached.Equals(ClientNearCache.NullObject))
                        {
                            return default(V);
                        }
                        return (V)cached;
                    });
                    return task;
                }
            }

            var request = new MapGetRequest(name, keyData);
            try
            {
                return GetContext().GetInvocationService().InvokeOnKeyOwner<V>(request, key);
            }
            catch (Exception e)
            {
                throw ExceptionUtil.Rethrow(e);
            }  
        }

        public Task<V> PutAsync(K key, V value)
        {
            return PutAsync(key, value, -1, TimeUnit.SECONDS);
        }

        public Task<V> PutAsync(K key, V value, long ttl, TimeUnit timeunit)
        {
            Data keyData = ToData(key);
            Data valueData = ToData(value);
            var request = new MapPutRequest(name, keyData, valueData, ThreadUtil.GetThreadId(),GetTimeInMillis(ttl, timeunit));
            try {
                var task = GetContext().GetInvocationService().InvokeOnKeyOwner<V>(request, keyData);
                return task;
            } catch (Exception e) {
                throw ExceptionUtil.Rethrow(e);
            }
        }

        public Task<V> RemoveAsync(K key)
        {
            Data keyData = ToData(key);
            var request = new MapRemoveRequest(name, keyData, ThreadUtil.GetThreadId());
            try
            {
                var task = GetContext().GetInvocationService().InvokeOnKeyOwner<V>(request, keyData);
                return task;
            }
            catch (Exception e)
            {
                throw ExceptionUtil.Rethrow(e);
            }
        }

        public bool TryRemove(K key, long timeout, TimeUnit timeunit)
        {
            Data keyData = ToData(key);
            var request = new MapTryRemoveRequest(name, keyData, ThreadUtil.GetThreadId(), timeunit.ToMillis(timeout));
            var result = Invoke<bool>(request, keyData);
            return result;
        }

        public bool TryPut(K key, V value, long timeout, TimeUnit timeunit)
        {
            Data keyData = ToData(key);
            Data valueData = ToData(value);
            var request = new MapTryPutRequest(name, keyData, valueData, ThreadUtil.GetThreadId(),
                timeunit.ToMillis(timeout));
            var result = Invoke<bool>(request, keyData);
            return result;
        }

        public V Put(K key, V value, long ttl, TimeUnit timeunit)
        {
            Data keyData = ToData(key);
            Data valueData = ToData(value);
            var request = new MapPutRequest(name, keyData, valueData, ThreadUtil.GetThreadId(),
                GetTimeInMillis(ttl, timeunit));
            return Invoke<V>(request, keyData);
        }

        public void PutTransient(K key, V value, long ttl, TimeUnit timeunit)
        {
            Data keyData = ToData(key);
            Data valueData = ToData(value);
            var request = new MapPutTransientRequest(name, keyData, valueData, ThreadUtil.GetThreadId(),
                GetTimeInMillis(ttl, timeunit));
            Invoke<object>(request);
        }

        public V PutIfAbsent(K key, V value)
        {
            return PutIfAbsent(key, value, -1, TimeUnit.SECONDS);
        }

        public V PutIfAbsent(K key, V value, long ttl, TimeUnit timeunit)
        {
            Data keyData = ToData(key);
            Data valueData = ToData(value);
            var request = new MapPutIfAbsentRequest(name, keyData, valueData, ThreadUtil.GetThreadId(),
                GetTimeInMillis(ttl, timeunit));
            return Invoke<V>(request, keyData);
        }

        public bool Replace(K key, V oldValue, V newValue)
        {
            Data keyData = ToData(key);
            Data oldValueData = ToData(oldValue);
            Data newValueData = ToData(newValue);
            var request = new MapReplaceIfSameRequest(name, keyData, oldValueData, newValueData,
                ThreadUtil.GetThreadId());
            var result = Invoke<bool>(request, keyData);
            return result;
        }

        public V Replace(K key, V value)
        {
            Data keyData = ToData(key);
            Data valueData = ToData(value);
            var request = new MapReplaceRequest(name, keyData, valueData, ThreadUtil.GetThreadId());
            return Invoke<V>(request, keyData);
        }

        public void Set(K key, V value, long ttl, TimeUnit timeunit)
        {
            Data keyData = ToData(key);
            Data valueData = ToData(value);
            var request = new MapSetRequest(name, keyData, valueData, ThreadUtil.GetThreadId(),
                GetTimeInMillis(ttl, timeunit));
            Invoke<object>(request, keyData);
        }

        public void Lock(K key)
        {
            Data keyData = ToData(key);
            var request = new MapLockRequest(name, keyData, ThreadUtil.GetThreadId());
            Invoke<object>(request, keyData);
        }

        public void Lock(K key, long leaseTime, TimeUnit timeUnit)
        {
            Data keyData = ToData(key);
            var request = new MapLockRequest(name, keyData, ThreadUtil.GetThreadId(),
                GetTimeInMillis(leaseTime, timeUnit), -1);
            Invoke<object>(request, keyData);
        }

        public bool IsLocked(K key)
        {
            Data keyData = ToData(key);
            var request = new MapIsLockedRequest(name, keyData);
            var result = Invoke<bool>(request, keyData);
            return result;
        }

        public bool TryLock(K key)
        {
            try
            {
                return TryLock(key, 0, TimeUnit.SECONDS);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <exception cref="System.Exception"></exception>
        public bool TryLock(K key, long time, TimeUnit timeunit)
        {
            Data keyData = ToData(key);
            var request = new MapLockRequest(name, keyData, ThreadUtil.GetThreadId(), long.MaxValue,
                GetTimeInMillis(time, timeunit));
            var result = Invoke<bool>(request, keyData);
            return result;
        }

        public void Unlock(K key)
        {
            Data keyData = ToData(key);
            var request = new MapUnlockRequest(name, keyData, ThreadUtil.GetThreadId(), false);
            Invoke<object>(request, keyData);
        }

        public void ForceUnlock(K key)
        {
            Data keyData = ToData(key);
            var request = new MapUnlockRequest(name, keyData, ThreadUtil.GetThreadId(), true);
            Invoke<object>(request, keyData);
        }

        public string AddLocalEntryListener(IEntryListener<K, V> listener)
        {
            throw new NotSupportedException("Locality is ambiguous for client!!!");
        }

        public string AddInterceptor(MapInterceptor interceptor)
        {
            var request = new MapAddInterceptorRequest(name, interceptor);
            return Invoke<string>(request);
        }

        public void RemoveInterceptor(string id)
        {
            var request = new MapRemoveInterceptorRequest(name, id);
            Invoke<object>(request);
        }

        public string AddEntryListener(IEntryListener<K, V> listener, bool includeValue)
        {
            var request = new MapAddEntryListenerRequest<K, V>(name, includeValue);
            DistributedEventHandler handler = (_event) => OnEntryEvent((PortableEntryEvent) _event, includeValue, listener);
            return Listen(request, handler);
        }

        public bool RemoveEntryListener(string id)
        {
            var request = new MapRemoveEntryListenerRequest(name, id);
            return StopListening(request,id);
        }

        public string AddEntryListener(IEntryListener<K, V> listener, K key, bool includeValue)
        {
            Data keyData = ToData(key);
            var request = new MapAddEntryListenerRequest<K, V>(name, keyData, includeValue);
            DistributedEventHandler handler = (_event) => OnEntryEvent((PortableEntryEvent)_event, includeValue, listener);
            return Listen(request, keyData, handler);
        }

        public IEntryView<K, V> GetEntryView(K key)
        {
            throw new NotSupportedException("EntryView not implemented");
            //Data keyData = ToData(key);
            //MapGetEntryViewRequest request = new MapGetEntryViewRequest(name, keyData);
            //SimpleEntryView<K,V> entryView = Invoke<SimpleEntryView<K,V>>(request, keyData);
            //if (entryView == null)
            //{
            //    return null;
            //}
            //Data value = (Data)entryView.GetValue();
            //entryView.SetKey(key);
            //entryView.SetValue(ToObject<V>(value));
            //return entryView;
        }

        public bool Evict(K key)
        {
            Data keyData = ToData(key);
            var request = new MapEvictRequest(name, keyData, ThreadUtil.GetThreadId());
            var result = Invoke<bool>(request);
            return result;
        }

        public ISet<K> KeySet()
        {
            var request = new MapKeySetRequest(name);
            var mapKeySet = Invoke<MapKeySet>(request);
            ICollection<Data> keySetData = mapKeySet.GetKeySet();
            ISet<K> keySet = new HashSet<K>();
            foreach (Data data in keySetData)
            {
                var key = ToObject<K>(data);
                keySet.Add(key);
            }
            return keySet;
        }

        public IDictionary<K, V> GetAll(ICollection<K> keys)
        {
            var keySet = new HashSet<Data>();
            foreach (object key in keys)
            {
                keySet.Add(ToData(key));
            }
            var request = new MapGetAllRequest(name, keySet);
            var mapEntrySet = Invoke<MapEntrySet>(request);
            IDictionary<K, V> result = new Dictionary<K, V>();
            ICollection<KeyValuePair<Data, Data>> entrySet = mapEntrySet.GetEntrySet();
            foreach (var dataEntry in entrySet)
            {
                result.Add(ToObject<K>(dataEntry.Key), ToObject<V>(dataEntry.Value));
            }
            return result;
        }

        public ICollection<V> Values()
        {
            var request = new MapValuesRequest(name);
            var mapValueCollection = Invoke<MapValueCollection>(request);
            ICollection<Data> collectionData = mapValueCollection.GetValues();
            ICollection<V> collection = new List<V>(collectionData.Count);
            foreach (Data data in collectionData)
            {
                var value = ToObject<V>(data);
                collection.Add(value);
            }
            return collection;
        }

        public ICollection<KeyValuePair<K, V>> EntrySet()
        {
            var request = new MapEntrySetRequest(name);
            var result = Invoke<MapEntrySet>(request);
            ICollection<KeyValuePair<K, V>> entrySet = new HashSet<KeyValuePair<K, V>>();
            ICollection<KeyValuePair<Data, Data>> entries = result.GetEntrySet();
            foreach (KeyValuePair<Data, Data> dataEntry in entries)
            {
                Data keyData = dataEntry.Key;
                Data valueData = dataEntry.Value;
                K key = ToObject<K>(keyData);
                V value = ToObject<V>(valueData);
                entrySet.Add(new KeyValuePair<K, V>(key, value));
            }
            return entrySet;
        }

        public void AddIndex(string attribute, bool ordered)
        {
            var request = new MapAddIndexRequest(name, attribute, ordered);
            Invoke<object>(request);
        }

         public void Set(K key, V value)
        {
            Set(key, value, -1, TimeUnit.SECONDS);
        }

        public int Size()
        {
            var request = new MapSizeRequest(name);
            var result = Invoke<int>(request);
            return result;
        }

        public bool IsEmpty()
        {
            return Size() == 0;
        }

        public void PutAll<K>(IDictionary<K, V> m)
        {
            var entrySet = new MapEntrySet();
            foreach (var entry in m)
            {
                entrySet.Add(new KeyValuePair<Data, Data>(ToData(entry.Key), ToData(entry.Value)));
            }
            var request = new MapPutAllRequest(name, entrySet);
            Invoke<object>(request);
        }

        public void Clear()
        {
            var request = new MapClearRequest(name);
            Invoke<object>(request);
        }

        public string AddEntryListener(IEntryListener<K, V> listener, IPredicate<K, V> predicate, K key,
            bool includeValue)
        {
            Data keyData = ToData(key);
            MapAddEntryListenerRequest<K, V> request = new MapAddEntryListenerRequest<K, V>(name, keyData, includeValue, predicate);
            DistributedEventHandler handler = (_event) => OnEntryEvent((PortableEntryEvent)_event, includeValue, listener);
            return Listen(request, keyData, handler);
        }

        public string AddEntryListener(IEntryListener<K, V> listener, IPredicate<K, V> predicate, bool includeValue)
        {
            MapAddEntryListenerRequest<K, V> request = new MapAddEntryListenerRequest<K, V>(name, null, includeValue, predicate);
            DistributedEventHandler handler = (_event) => OnEntryEvent((PortableEntryEvent)_event, includeValue, listener); 
            return Listen(request, null, handler);
        }

        public ICollection<K> KeySet(IPredicate<K, V> predicate)
        {
            var request = new MapQueryRequest<K, V>(name, predicate, IterationType.Key);
            var result = Invoke<QueryResultSet>(request);
            ICollection<K> keySet = new HashSet<K>();
            foreach (object data in result)
            {
                K key = ToObject<K>((Data)data);
                keySet.Add(key);
            }
            return keySet;
        }

        public ICollection<KeyValuePair<K, V>> EntrySet(IPredicate<K, V> predicate)
        {
            var request = new MapQueryRequest<K, V>(name, predicate, IterationType.Entry);
            var result = Invoke<QueryResultSet>(request);
            ICollection<KeyValuePair<K, V>> entrySet = new HashSet<KeyValuePair<K, V>>();
            foreach (IQueryResultEntry dataEntry in result.entries)
            {
                K key = ToObject<K>(dataEntry.GetKeyData());
                V value = ToObject<V>(dataEntry.GetValueData());
                entrySet.Add(new KeyValuePair<K, V>(key, value));
            }
            return entrySet;
        }

        public ICollection<V> Values(IPredicate<K, V> predicate)
        {
            var request = new MapQueryRequest<K, V>(name, predicate, IterationType.Value);
            var result = Invoke<QueryResultSet>(request);
            ICollection<V> values = new List<V>(result.Count);
            foreach (object data in result)
            {
                var value = ToObject<V>((Data)data);
                values.Add(value);
            }
            return values;
        }

        protected  override void OnDestroy()
        {
            if (nearCache != null)
            {
                nearCache.Destroy();
            }
        }

        protected internal long GetTimeInMillis(long time, TimeUnit timeunit)
        {
            return timeunit != null ? timeunit.ToMillis(time) : time;
        }

        public void OnEntryEvent(PortableEntryEvent _event, bool includeValue, IEntryListener<K, V> listener)
        {
            V value = default(V);
            V oldValue = default(V);
            if (includeValue)
            {
                value = ToObject<V>(_event.GetValue());
                oldValue = ToObject<V>(_event.GetOldValue());
            }
            var key = ToObject<K>(_event.GetKey());
            IMember member = GetContext().GetClusterService().GetMember(_event.GetUuid());
            var entryEvent = new EntryEvent<K, V>(name, member, _event.GetEventType(), key, oldValue, value);
            switch (_event.GetEventType())
            {
                case EntryEventType.Added:
                {
                    listener.EntryAdded(entryEvent);
                    break;
                }

                case EntryEventType.Removed:
                {
                    listener.EntryRemoved(entryEvent);
                    break;
                }

                case EntryEventType.Updated:
                {
                    listener.EntryUpdated(entryEvent);
                    break;
                }

                case EntryEventType.Evicted:
                {
                    listener.EntryEvicted(entryEvent);
                    break;
                }
            }
        }

        private void InitNearCache()
        {
            if (nearCacheInitialized.CompareAndSet(false, true))
            {
                NearCacheConfig nearCacheConfig = GetContext().GetClientConfig().GetNearCacheConfig(name);
                if (nearCacheConfig == null)
                {
                    return;
                }
                var _nearCache = new ClientNearCache(name,ClientNearCacheType.Map, GetContext(), nearCacheConfig);
                nearCache = _nearCache;
            }
        }


    }
}