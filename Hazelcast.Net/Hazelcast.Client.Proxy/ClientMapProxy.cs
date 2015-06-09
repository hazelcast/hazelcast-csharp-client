using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Client.Spi;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;
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

        internal ClientNearCache NearCache
        {
            get { return nearCache; }
        }

        public bool ContainsKey(object key)
        {
            var keyData = ToData(key);
            if (nearCache != null)
            {
                var cached = nearCache.Get(keyData);
                if (cached != null)
                {
                    if (cached.Equals(ClientNearCache.NullObject))
                    {
                        return false;
                    }
                    return true;
                }
            }
            var request = MapContainsKeyCodec.EncodeRequest(name, keyData, ThreadUtil.GetThreadId());
            var result = Invoke(request, keyData);
            return MapContainsKeyCodec.DecodeResponse(result).response;
        }

        public bool ContainsValue(object value)
        {
            var valueData = ToData(value);
            var request = MapContainsValueCodec.EncodeRequest(name, valueData);
            var result = Invoke(request);
            return MapContainsValueCodec.DecodeResponse(result).response;
        }

        public V Get(object key)
        {
            var keyData = ToData(key);
            if (nearCache != null)
            {
                var cached = nearCache.Get(keyData);
                if (cached != null)
                {
                    if (cached.Equals(ClientNearCache.NullObject))
                    {
                        return default(V);
                    }
                    return (V) cached;
                }
            }
            var request = MapGetCodec.EncodeRequest(name, keyData, ThreadUtil.GetThreadId());
            var result = Invoke(request, keyData);
            if (nearCache != null)
            {
                nearCache.Put(keyData, result);
            }
            return ToObject<V>(MapGetCodec.DecodeResponse(result).response);
        }

        public V Put(K key, V value)
        {
            return Put(key, value, -1, TimeUnit.SECONDS);
        }

        public V Remove(object key)
        {
            var keyData = ToData(key);
            var request = MapRemoveCodec.EncodeRequest(name, keyData, ThreadUtil.GetThreadId());
            InvalidateNearCacheEntry(keyData);
            var clientMessage = Invoke(request, keyData);
            return ToObject<V>(MapRemoveCodec.DecodeResponse(clientMessage).response);
        }

        public bool Remove(object key, object value)
        {
            var keyData = ToData(key);
            var valueData = ToData(value);
            var request = MapRemoveIfSameCodec.EncodeRequest(name, keyData, valueData, ThreadUtil.GetThreadId());
            InvalidateNearCacheEntry(keyData);
            var clientMessage = Invoke(request, keyData);
            return MapRemoveIfSameCodec.DecodeResponse(clientMessage).response;
        }

        public void Delete(object key)
        {
            var keyData = ToData(key);
            var request = MapDeleteCodec.EncodeRequest(name, keyData, ThreadUtil.GetThreadId());
            InvalidateNearCacheEntry(keyData);
            Invoke(request, keyData);
        }

        public void Flush()
        {
            var request = MapFlushCodec.EncodeRequest(name);
            Invoke(request);
        }

        public Task<V> GetAsync(K key)
        {
            var keyData = ToData(key);
            if (nearCache != null)
            {
                var cached = nearCache.Get(keyData);
                if (cached != null)
                {
                    var task = Task.Factory.StartNew(() =>
                    {
                        if (cached.Equals(ClientNearCache.NullObject))
                        {
                            return default(V);
                        }
                        return (V) cached;
                    });
                    return task;
                }
            }

            var request = MapGetAsyncCodec.EncodeRequest(name, keyData, ThreadUtil.GetThreadId());
            try
            {
                var task = GetContext().GetInvocationService().InvokeOnKeyOwner(request, key);
                var deserializeTask = task.ContinueWith(continueTask =>
                {
                    var responseMessage = ThreadUtil.GetResult(continueTask);
                    var result = MapGetAsyncCodec.DecodeResponse(responseMessage).response;
                    if (nearCache != null)
                    {
                        nearCache.Put(keyData, result);
                    }
                    return ToObject<V>(result);
                });
                return deserializeTask;
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
            var keyData = ToData(key);
            var valueData = ToData(value);
            var request = MapPutAsyncCodec.EncodeRequest(name, keyData, valueData, ThreadUtil.GetThreadId(),
                GetTimeInMillis(ttl, timeunit));
            try
            {
                var task = GetContext().GetInvocationService().InvokeOnKeyOwner(request, keyData);
                var deserializeTask = task.ContinueWith(continueTask =>
                {
                    InvalidateNearCacheEntry(keyData);
                    var clientMessage = ThreadUtil.GetResult(continueTask);
                    var responseParameters = MapPutAsyncCodec.DecodeResponse(clientMessage);
                    return ToObject<V>(responseParameters.response);
                });
                return deserializeTask;
            }
            catch (Exception e)
            {
                throw ExceptionUtil.Rethrow(e);
            }
        }

        public Task<V> RemoveAsync(K key)
        {
            var keyData = ToData(key);
            var request = MapRemoveAsyncCodec.EncodeRequest(name, keyData, ThreadUtil.GetThreadId());
            try
            {
                var task = GetContext().GetInvocationService().InvokeOnKeyOwner(request, keyData);
                var deserializeTask = task.ContinueWith(continueTask =>
                {
                    InvalidateNearCacheEntry(keyData);
                    var clientMessage = ThreadUtil.GetResult(continueTask);
                    var responseParameters = MapRemoveAsyncCodec.DecodeResponse(clientMessage);
                    return ToObject<V>(responseParameters.response);
                });
                return deserializeTask;
            }
            catch (Exception e)
            {
                throw ExceptionUtil.Rethrow(e);
            }
        }

        public bool TryRemove(K key, long timeout, TimeUnit timeunit)
        {
            var keyData = ToData(key);
            var request = MapTryRemoveCodec.EncodeRequest(name, keyData, ThreadUtil.GetThreadId(),
                timeunit.ToMillis(timeout));
            var result = Invoke(request, keyData);
            var response = MapTryRemoveCodec.DecodeResponse(result).response;
            if (response) InvalidateNearCacheEntry(keyData);
            return response;
        }

        public bool TryPut(K key, V value, long timeout, TimeUnit timeunit)
        {
            var keyData = ToData(key);
            var valueData = ToData(value);
            var request = MapTryPutCodec.EncodeRequest(name, keyData, valueData, ThreadUtil.GetThreadId(),
                timeunit.ToMillis(timeout));
            var result = Invoke(request, keyData);
            var response = MapTryPutCodec.DecodeResponse(result).response;
            if (response) InvalidateNearCacheEntry(keyData);
            return response;
        }

        public V Put(K key, V value, long ttl, TimeUnit timeunit)
        {
            var keyData = ToData(key);
            var valueData = ToData(value);
            var request = MapPutCodec.EncodeRequest(name, keyData, valueData, ThreadUtil.GetThreadId(),
                GetTimeInMillis(ttl, timeunit));
            InvalidateNearCacheEntry(keyData);
            var clientMessage = Invoke(request, keyData);
            var response = MapPutCodec.DecodeResponse(clientMessage).response;
            return ToObject<V>(response);
        }

        public void PutTransient(K key, V value, long ttl, TimeUnit timeunit)
        {
            var keyData = ToData(key);
            var valueData = ToData(value);
            var request = MapPutTransientCodec.EncodeRequest(name, keyData, valueData, ThreadUtil.GetThreadId(),
                GetTimeInMillis(ttl, timeunit));
            InvalidateNearCacheEntry(keyData);
            Invoke(request);
        }

        public V PutIfAbsent(K key, V value)
        {
            return PutIfAbsent(key, value, -1, TimeUnit.SECONDS);
        }

        public V PutIfAbsent(K key, V value, long ttl, TimeUnit timeunit)
        {
            var keyData = ToData(key);
            var valueData = ToData(value);
            var request = MapPutIfAbsentCodec.EncodeRequest(name, keyData, valueData, ThreadUtil.GetThreadId(),
                GetTimeInMillis(ttl, timeunit));
            var clientMessage = Invoke(request, keyData);
            var response = MapPutIfAbsentCodec.DecodeResponse(clientMessage).response;
            return ToObject<V>(response);
        }

        public bool Replace(K key, V oldValue, V newValue)
        {
            var keyData = ToData(key);
            var oldValueData = ToData(oldValue);
            var newValueData = ToData(newValue);
            var request = MapReplaceIfSameCodec.EncodeRequest(name, keyData, oldValueData, newValueData,
                ThreadUtil.GetThreadId());
            InvalidateNearCacheEntry(keyData);
            var clientMessage = Invoke(request, keyData);
            return MapReplaceIfSameCodec.DecodeResponse(clientMessage).response;
        }

        public V Replace(K key, V value)
        {
            var keyData = ToData(key);
            var valueData = ToData(value);
            var request = MapReplaceCodec.EncodeRequest(name, keyData, valueData, ThreadUtil.GetThreadId());
            InvalidateNearCacheEntry(keyData);
            var clientMessage = Invoke(request, keyData);
            var response = MapReplaceCodec.DecodeResponse(clientMessage).response;
            return ToObject<V>(response);
        }

        public void Set(K key, V value, long ttl, TimeUnit timeunit)
        {
            var keyData = ToData(key);
            var valueData = ToData(value);
            var request = MapSetCodec.EncodeRequest(name, keyData, valueData, ThreadUtil.GetThreadId(),
                GetTimeInMillis(ttl, timeunit));
            InvalidateNearCacheEntry(keyData);
            Invoke(request, keyData);
        }

        public void Lock(K key)
        {
            Lock(key, -1, TimeUnit.MILLISECONDS);
        }

        public void Lock(K key, long leaseTime, TimeUnit timeUnit)
        {
            var keyData = ToData(key);
            var request = MapLockCodec.EncodeRequest(name, keyData, ThreadUtil.GetThreadId(),
                GetTimeInMillis(leaseTime, timeUnit));
            Invoke(request, keyData);
        }

        public bool IsLocked(K key)
        {
            var keyData = ToData(key);
            var request = MapIsLockedCodec.EncodeRequest(name, keyData);
            var result = Invoke(request, keyData);
            return MapIsLockedCodec.DecodeResponse(result).response;
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
            var keyData = ToData(key);
            var request = MapTryLockCodec.EncodeRequest(name, keyData, ThreadUtil.GetThreadId(),
                GetTimeInMillis(time, timeunit));
            var response = Invoke(request, keyData);
            var resultParameters = MapTryLockCodec.DecodeResponse(response);
            return resultParameters.response;
        }

        public void Unlock(K key)
        {
            var keyData = ToData(key);
            var request = MapUnlockCodec.EncodeRequest(name, keyData, ThreadUtil.GetThreadId());
            Invoke(request, keyData);
        }

        public void ForceUnlock(K key)
        {
            var keyData = ToData(key);
            var request = MapForceUnlockCodec.EncodeRequest(name, keyData);
            Invoke(request, keyData);
        }

        public string AddInterceptor(IMapInterceptor interceptor)
        {
            var data = ToData(interceptor);
            var request = MapAddInterceptorCodec.EncodeRequest(name, data);
            var response = Invoke(request);
            var resultParameters = MapAddInterceptorCodec.DecodeResponse(response);
            return resultParameters.response;
        }

        public void RemoveInterceptor(string id)
        {
            var request = MapRemoveInterceptorCodec.EncodeRequest(name, id);
            Invoke(request);
        }

        public string AddEntryListener(IEntryListener<K, V> listener, bool includeValue)
        {
            var request = MapAddEntryListenerCodec.EncodeRequest(name, includeValue);
            DistributedEventHandler handler =
                eventData => MapAddEntryListenerCodec.AbstractEventHandler.Handle(eventData,
                    (key, value, oldValue, mergingValue, type, uuid, entries) =>
                    {
                        OnEntryEvent(key, value, oldValue, mergingValue, type, uuid, entries, includeValue, listener);
                    });

            return Listen(request, message => MapAddEntryListenerCodec.DecodeResponse(message).response, handler);
        }

        public bool RemoveEntryListener(string id)
        {
            var request = MapRemoveEntryListenerCodec.EncodeRequest(name, id);
            return StopListening(request, message => MapRemoveEntryListenerCodec.DecodeResponse(message).response, id);
        }

        public string AddEntryListener(IEntryListener<K, V> listener, K keyK, bool includeValue)
        {
            var keyData = ToData(keyK);
            var request = MapAddEntryListenerToKeyCodec.EncodeRequest(name, keyData, includeValue);
            DistributedEventHandler handler =
                eventData => MapAddEntryListenerCodec.AbstractEventHandler.Handle(eventData,
                    (key, value, oldValue, mergingValue, type, uuid, entries) =>
                    {
                        OnEntryEvent(key, value, oldValue, mergingValue, type, uuid, entries, includeValue, listener);
                    });

            return Listen(request, message => MapAddEntryListenerCodec.DecodeResponse(message).response, keyData,
                handler);
        }

        public IEntryView<K, V> GetEntryView(K key)
        {
            var keyData = ToData(key);
            var request = MapGetEntryViewCodec.EncodeRequest(name, keyData, ThreadUtil.GetThreadId());
            var response = Invoke(request, keyData);
            var parameters = MapGetEntryViewCodec.DecodeResponse(response);
            var entryView = new SimpleEntryView<K, V>();
            var dataEntryView = parameters.dataEntryView;
            if (dataEntryView == null)
            {
                return null;
            }
            entryView.SetKey(ToObject<K>(dataEntryView.GetKey()));
            entryView.SetValue(ToObject<V>(dataEntryView.GetValue()));
            entryView.SetCost(dataEntryView.GetCost());
            entryView.SetCreationTime(dataEntryView.GetCreationTime());
            entryView.SetExpirationTime(dataEntryView.GetExpirationTime());
            entryView.SetHits(dataEntryView.GetHits());
            entryView.SetLastAccessTime(dataEntryView.GetLastAccessTime());
            entryView.SetLastStoredTime(dataEntryView.GetLastStoredTime());
            entryView.SetLastUpdateTime(dataEntryView.GetLastUpdateTime());
            entryView.SetVersion(dataEntryView.GetVersion());
            entryView.SetEvictionCriteriaNumber(dataEntryView.GetEvictionCriteriaNumber());
            entryView.SetTtl(dataEntryView.GetTtl());
            //TODO putCache
            return entryView;
        }

        public bool Evict(K key)
        {
            var keyData = ToData(key);
            var request = MapEvictCodec.EncodeRequest(name, keyData, ThreadUtil.GetThreadId());
            var response = Invoke(request, keyData);
            var resultParameters = MapEvictCodec.DecodeResponse(response);
            return resultParameters.response;
        }

        public ISet<K> KeySet()
        {
            var request = MapKeySetCodec.EncodeRequest(name);
            var response = Invoke(request);
            var resultParameters = MapKeySetCodec.DecodeResponse(response);
            var result = resultParameters.list;
            ISet<K> keySet = new HashSet<K>();
            foreach (var data in result)
            {
                var key = ToObject<K>(data);
                keySet.Add(key);
            }
            return keySet;
        }

        public IDictionary<K, V> GetAll(ICollection<K> keys)
        {
            var keySet = new HashSet<IData>();
            IDictionary<K, V> result = new Dictionary<K, V>();
            foreach (object key in keys)
            {
                keySet.Add(ToData(key));
            }

            if (nearCache != null)
            {
                foreach (var keyData in keySet)
                {
                    var cached = nearCache.Get(keyData);
                    if (cached != null)
                    {
                        if (!cached.Equals(ClientNearCache.NullObject))
                        {
                            result.Add(ToObject<K>(keyData), (V) cached);
                            keySet.Remove(keyData);
                        }
                    }
                }
            }
            if (keySet.Count == 0)
            {
                return result;
            }

            var request = MapGetAllCodec.EncodeRequest(name, keySet);
            var response = Invoke(request);
            var resultParameters = MapGetAllCodec.DecodeResponse(response);
            foreach (var entry in resultParameters.map)
            {
                var value = ToObject<V>(entry.Value);
                var key_1 = ToObject<K>(entry.Key);
                result[key_1] = value;
                if (nearCache != null)
                {
                    nearCache.Put(entry.Key, value);
                }
            }
            return result;
        }

        public ICollection<V> Values()
        {
            var request = MapValuesCodec.EncodeRequest(name);
            var response = Invoke(request);
            var resultParameters = MapValuesCodec.DecodeResponse(response);
            var collectionData = resultParameters.list;
            ICollection<V> collection = new List<V>(collectionData.Count);
            foreach (var data in collectionData)
            {
                var value = ToObject<V>(data);
                collection.Add(value);
            }
            return collection;
        }

        public ISet<KeyValuePair<K, V>> EntrySet()
        {
            var request = MapEntrySetCodec.EncodeRequest(name);
            var response = Invoke(request);
            var resultParameters = MapEntrySetCodec.DecodeResponse(response);
            ISet<KeyValuePair<K, V>> entrySet = new HashSet<KeyValuePair<K, V>>();
            foreach (var entry in resultParameters.map)
            {
                var key = ToObject<K>(entry.Key);
                var value = ToObject<V>(entry.Value);
                entrySet.Add(new KeyValuePair<K, V>(key, value));
            }
            return entrySet;
        }

        public void AddIndex(string attribute, bool ordered)
        {
            var request = MapAddIndexCodec.EncodeRequest(name, attribute, ordered);
            Invoke(request);
        }

        public void Set(K key, V value)
        {
            Set(key, value, -1, TimeUnit.SECONDS);
        }

        public int Size()
        {
            var request = MapSizeCodec.EncodeRequest(name);
            var result = Invoke(request);
            return MapSizeCodec.DecodeResponse(result).response;
        }

        public bool IsEmpty()
        {
            return Size() == 0;
        }

        public void PutAll(IDictionary<K, V> m)
        {
            IDictionary<IData, IData> map = new Dictionary<IData, IData>();
            foreach (var entry in m)
            {
                var keyData = ToData(entry.Key);
                InvalidateNearCacheEntry(keyData);
                map[keyData] = ToData(entry.Value);
            }
            var request = MapPutAllCodec.EncodeRequest(name, map);
            Invoke(request);
        }

        public void Clear()
        {
            var request = MapClearCodec.EncodeRequest(name);
            Invoke(request);
            if (nearCache != null)
            {
                nearCache.InvalidateAll();
            }
        }

        public string AddEntryListener(IEntryListener<K, V> listener, IPredicate<K, V> predicate, K _key,
            bool includeValue)
        {
            var keyData = ToData(_key);
            var predicateData = ToData(predicate);
            var request = MapAddEntryListenerToKeyWithPredicateCodec.EncodeRequest(name, keyData, predicateData,
                includeValue);
            DistributedEventHandler handler =
                eventData => MapAddEntryListenerToKeyWithPredicateCodec.AbstractEventHandler.Handle(eventData,
                    (key, value, oldValue, mergingValue, type, uuid, entries) =>
                    {
                        OnEntryEvent(key, value, oldValue, mergingValue, type, uuid, entries, includeValue, listener);
                    });

            return Listen(request, message => MapAddEntryListenerCodec.DecodeResponse(message).response, keyData,
                handler);
        }

        public string AddEntryListener(IEntryListener<K, V> listener, IPredicate<K, V> predicate, bool includeValue)
        {
            var predicateData = ToData(predicate);
            var request = MapAddEntryListenerWithPredicateCodec.EncodeRequest(name, predicateData, includeValue);
            DistributedEventHandler handler =
                eventData => MapAddEntryListenerToKeyWithPredicateCodec.AbstractEventHandler.Handle(eventData,
                    (key, value, oldValue, mergingValue, type, uuid, entries) =>
                    {
                        OnEntryEvent(key, value, oldValue, mergingValue, type, uuid, entries, includeValue, listener);
                    });

            return Listen(request, message => MapAddEntryListenerCodec.DecodeResponse(message).response, null,
                handler);
        }

        public ISet<K> KeySet(IPredicate<K, V> predicate)
        {
            //TODO not supported yet
            //if (predicate is PagingPredicate)
            //{
            //    return KeySetWithPagingPredicate((PagingPredicate)predicate);
            //}
            var request = MapKeySetWithPredicateCodec.EncodeRequest(name, ToData(predicate));
            var response = Invoke(request);
            var resultParameters = MapKeySetWithPredicateCodec.DecodeResponse(response);
            var keySet = new HashSet<K>();
            foreach (var o in resultParameters.list)
            {
                var key = ToObject<K>(o);
                keySet.Add(key);
            }
            return keySet;
        }

        public ISet<KeyValuePair<K, V>> EntrySet(IPredicate<K, V> predicate)
        {
            var request = MapEntriesWithPredicateCodec.EncodeRequest(name, ToData(predicate));
            var response = Invoke(request);
            var result = MapEntriesWithPredicateCodec.DecodeResponse(response);
            ISet<KeyValuePair<K, V>> entrySet = new HashSet<KeyValuePair<K, V>>();
            foreach (var dataEntry in result.map)
            {
                var key = ToObject<K>(dataEntry.Key);
                var value = ToObject<V>(dataEntry.Value);
                entrySet.Add(new KeyValuePair<K, V>(key, value));
            }
            return entrySet;
        }

        public ICollection<V> Values(IPredicate<K, V> predicate)
        {
            var request = MapValuesWithPredicateCodec.EncodeRequest(name, ToData(predicate));
            var response = Invoke(request);
            var resultParameters = MapValuesWithPredicateCodec.DecodeResponse(response);
            var result = resultParameters.list;
            IList<V> values = new List<V>(result.Count);
            foreach (var data in result)
            {
                var value = ToObject<V>(data);
                values.Add(value);
            }
            return values;
        }

        protected override void OnDestroy()
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

        public void OnEntryEvent(IData keyData, IData valueData, IData oldValueData, IData mergingValue,
            int eventTypeInt, string uuid,
            int numberOfAffectedEntries, bool includeValue, IEntryListener<K, V> listener)
        {
            var value = default(V);
            var oldValue = default(V);
            if (includeValue)
            {
                value = ToObject<V>(valueData);
                oldValue = ToObject<V>(oldValueData);
            }
            var key = ToObject<K>(keyData);
            var member = GetContext().GetClusterService().GetMember(uuid);
            var eventType = (EntryEventType) eventTypeInt;
            switch (eventType)
            {
                case EntryEventType.Added:
                {
                    listener.EntryAdded(new EntryEvent<K, V>(name, member, eventType, key, oldValue, value));
                    break;
                }
                case EntryEventType.Removed:
                {
                    listener.EntryRemoved(new EntryEvent<K, V>(name, member, eventType, key, oldValue, value));
                    break;
                }
                case EntryEventType.Updated:
                {
                    listener.EntryUpdated(new EntryEvent<K, V>(name, member, eventType, key, oldValue, value));
                    break;
                }
                case EntryEventType.Evicted:
                {
                    listener.EntryEvicted(new EntryEvent<K, V>(name, member, eventType, key, oldValue, value));
                    break;
                }
                case EntryEventType.EvictAll:
                {
                    listener.MapEvicted(new MapEvent(name, member, eventType, numberOfAffectedEntries));
                    break;
                }
                case EntryEventType.ClearAll:
                {
                    listener.MapCleared(new MapEvent(name, member, eventType, numberOfAffectedEntries));
                    break;
                }
            }
        }

        internal override void PostInit()
        {
            if (nearCacheInitialized.CompareAndSet(false, true))
            {
                var nearCacheConfig = GetContext().GetClientConfig().GetNearCacheConfig(name);
                if (nearCacheConfig == null)
                {
                    return;
                }
                var _nearCache = new ClientNearCache(name, ClientNearCacheType.Map, GetContext(), nearCacheConfig);
                nearCache = _nearCache;
            }
        }

        private void InvalidateNearCacheEntry(IData key)
        {
            if (nearCache != null && nearCache.invalidateOnChange)
            {
                nearCache.Invalidate(key);
            }
        }
    }
}