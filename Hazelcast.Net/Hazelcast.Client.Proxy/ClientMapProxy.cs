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
        private readonly AtomicBoolean _nearCacheInitialized = new AtomicBoolean();
        private volatile ClientNearCache _nearCache;
        private string _nearCacheListenerId;

        public ClientMapProxy(string serviceName, string name) : base(serviceName, name)
        {
        }

        internal ClientNearCache NearCache
        {
            get { return _nearCache; }
        }

        public bool ContainsKey(object key)
        {
            var keyData = ToData(key);
            if (_nearCache != null)
            {
                var cached = _nearCache.Get(keyData);
                if (cached != null)
                {
                    if (cached.Equals(ClientNearCache.NullObject))
                    {
                        return false;
                    }
                    return true;
                }
            }
            var request = MapContainsKeyCodec.EncodeRequest(GetName(), keyData, ThreadUtil.GetThreadId());
            return Invoke(request, keyData, m => MapContainsKeyCodec.DecodeResponse(m).response);
        }

        public bool ContainsValue(object value)
        {
            var valueData = ToData(value);
            var request = MapContainsValueCodec.EncodeRequest(GetName(), valueData);
            var result = Invoke(request);
            return MapContainsValueCodec.DecodeResponse(result).response;
        }

        public V Get(object key)
        {
            var keyData = ToData(key);
            if (_nearCache != null)
            {
                var cached = _nearCache.Get(keyData);
                if (cached != null)
                {
                    if (cached.Equals(ClientNearCache.NullObject))
                    {
                        return default(V);
                    }
                    return (V) cached;
                }
            }
            var request = MapGetCodec.EncodeRequest(GetName(), keyData, ThreadUtil.GetThreadId());
            var result = Invoke(request, keyData);
            if (_nearCache != null)
            {
                _nearCache.Put(keyData, result);
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
            var request = MapRemoveCodec.EncodeRequest(GetName(), keyData, ThreadUtil.GetThreadId());
            InvalidateNearCacheEntry(keyData);
            var clientMessage = Invoke(request, keyData);
            return ToObject<V>(MapRemoveCodec.DecodeResponse(clientMessage).response);
        }

        public bool Remove(object key, object value)
        {
            var keyData = ToData(key);
            var valueData = ToData(value);
            var request = MapRemoveIfSameCodec.EncodeRequest(GetName(), keyData, valueData, ThreadUtil.GetThreadId());
            InvalidateNearCacheEntry(keyData);
            var clientMessage = Invoke(request, keyData);
            return MapRemoveIfSameCodec.DecodeResponse(clientMessage).response;
        }

        public void Delete(object key)
        {
            var keyData = ToData(key);
            var request = MapDeleteCodec.EncodeRequest(GetName(), keyData, ThreadUtil.GetThreadId());
            InvalidateNearCacheEntry(keyData);
            Invoke(request, keyData);
        }

        public void Flush()
        {
            var request = MapFlushCodec.EncodeRequest(GetName());
            Invoke(request);
        }

        public Task<V> GetAsync(K key)
        {
            var keyData = ToData(key);
            if (_nearCache != null)
            {
                var cached = _nearCache.Get(keyData);
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

            var request = MapGetAsyncCodec.EncodeRequest(GetName(), keyData, ThreadUtil.GetThreadId());
            try
            {
                var task = GetContext().GetInvocationService().InvokeOnKeyOwner(request, key);
                var deserializeTask = task.ContinueWith(continueTask =>
                {
                    var responseMessage = ThreadUtil.GetResult(continueTask);
                    var result = MapGetAsyncCodec.DecodeResponse(responseMessage).response;
                    if (_nearCache != null)
                    {
                        _nearCache.Put(keyData, result);
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
            var request = MapPutAsyncCodec.EncodeRequest(GetName(), keyData, valueData, ThreadUtil.GetThreadId(),
                timeunit.ToMillis(ttl));
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
            var request = MapRemoveAsyncCodec.EncodeRequest(GetName(), keyData, ThreadUtil.GetThreadId());
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
            var request = MapTryRemoveCodec.EncodeRequest(GetName(), keyData, ThreadUtil.GetThreadId(),
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
            var request = MapTryPutCodec.EncodeRequest(GetName(), keyData, valueData, ThreadUtil.GetThreadId(),
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
            var request = MapPutCodec.EncodeRequest(GetName(), keyData, valueData, ThreadUtil.GetThreadId(),
                timeunit.ToMillis(ttl));
            InvalidateNearCacheEntry(keyData);
            var clientMessage = Invoke(request, keyData);
            var response = MapPutCodec.DecodeResponse(clientMessage).response;
            return ToObject<V>(response);
        }

        public void PutTransient(K key, V value, long ttl, TimeUnit timeunit)
        {
            var keyData = ToData(key);
            var valueData = ToData(value);
            var request = MapPutTransientCodec.EncodeRequest(GetName(), keyData, valueData, ThreadUtil.GetThreadId(),
                timeunit.ToMillis(ttl));
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
            var request = MapPutIfAbsentCodec.EncodeRequest(GetName(), keyData, valueData, ThreadUtil.GetThreadId(),
                timeunit.ToMillis(ttl));
            var clientMessage = Invoke(request, keyData);
            var response = MapPutIfAbsentCodec.DecodeResponse(clientMessage).response;
            return ToObject<V>(response);
        }

        public bool Replace(K key, V oldValue, V newValue)
        {
            var keyData = ToData(key);
            var oldValueData = ToData(oldValue);
            var newValueData = ToData(newValue);
            var request = MapReplaceIfSameCodec.EncodeRequest(GetName(), keyData, oldValueData, newValueData,
                ThreadUtil.GetThreadId());
            InvalidateNearCacheEntry(keyData);
            var clientMessage = Invoke(request, keyData);
            return MapReplaceIfSameCodec.DecodeResponse(clientMessage).response;
        }

        public V Replace(K key, V value)
        {
            var keyData = ToData(key);
            var valueData = ToData(value);
            var request = MapReplaceCodec.EncodeRequest(GetName(), keyData, valueData, ThreadUtil.GetThreadId());
            InvalidateNearCacheEntry(keyData);
            var clientMessage = Invoke(request, keyData);
            var response = MapReplaceCodec.DecodeResponse(clientMessage).response;
            return ToObject<V>(response);
        }

        public void Set(K key, V value, long ttl, TimeUnit timeunit)
        {
            var keyData = ToData(key);
            var valueData = ToData(value);
            var request = MapSetCodec.EncodeRequest(GetName(), keyData, valueData, ThreadUtil.GetThreadId(),
                timeunit.ToMillis(ttl));
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
            var request = MapLockCodec.EncodeRequest(GetName(), keyData, ThreadUtil.GetThreadId(),
                timeUnit.ToMillis(leaseTime));
            Invoke(request, keyData);
        }

        public bool IsLocked(K key)
        {
            var keyData = ToData(key);
            var request = MapIsLockedCodec.EncodeRequest(GetName(), keyData);
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
            return TryLock(key, time, timeunit, Int64.MaxValue, TimeUnit.MILLISECONDS);
        }

        /// <exception cref="System.Exception"></exception>
        public bool TryLock(K key, long time, TimeUnit timeunit, long leaseTime, TimeUnit leaseUnit)
        {
            var keyData = ToData(key);
            var request = MapTryLockCodec.EncodeRequest(GetName(), keyData, ThreadUtil.GetThreadId(),
                leaseUnit.ToMillis(leaseTime), timeunit.ToMillis(time));
            return Invoke(request, keyData, m => MapTryLockCodec.DecodeResponse(m).response);
        }

        public void Unlock(K key)
        {
            var keyData = ToData(key);
            var request = MapUnlockCodec.EncodeRequest(GetName(), keyData, ThreadUtil.GetThreadId());
            Invoke(request, keyData);
        }

        public void ForceUnlock(K key)
        {
            var keyData = ToData(key);
            var request = MapForceUnlockCodec.EncodeRequest(GetName(), keyData);
            Invoke(request, keyData);
        }

        public string AddInterceptor(IMapInterceptor interceptor)
        {
            var data = ToData(interceptor);
            var request = MapAddInterceptorCodec.EncodeRequest(GetName(), data);
            return Invoke(request, m => MapAddInterceptorCodec.DecodeResponse(m).response);
        }

        public void RemoveInterceptor(string id)
        {
            var request = MapRemoveInterceptorCodec.EncodeRequest(GetName(), id);
            Invoke(request);
        }

        public string AddEntryListener(IEntryListener<K, V> listener, bool includeValue)
        {
            var request = MapAddEntryListenerCodec.EncodeRequest(GetName(), includeValue);
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
            var request = MapRemoveEntryListenerCodec.EncodeRequest(GetName(), id);
            return StopListening(request, message => MapRemoveEntryListenerCodec.DecodeResponse(message).response, id);
        }

        public string AddEntryListener(IEntryListener<K, V> listener, K keyK, bool includeValue)
        {
            var keyData = ToData(keyK);
            var request = MapAddEntryListenerToKeyCodec.EncodeRequest(GetName(), keyData, includeValue);
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
            var request = MapGetEntryViewCodec.EncodeRequest(GetName(), keyData, ThreadUtil.GetThreadId());
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
            var request = MapEvictCodec.EncodeRequest(GetName(), keyData, ThreadUtil.GetThreadId());
            var response = Invoke(request, keyData);
            var resultParameters = MapEvictCodec.DecodeResponse(response);
            return resultParameters.response;
        }

        public ISet<K> KeySet()
        {
            var request = MapKeySetCodec.EncodeRequest(GetName());
            var result = Invoke(request, m => MapKeySetCodec.DecodeResponse(m).set);

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

            if (_nearCache != null)
            {
                foreach (var keyData in keySet)
                {
                    var cached = _nearCache.Get(keyData);
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

            var request = MapGetAllCodec.EncodeRequest(GetName(), keySet);
            var entrySet = Invoke(request, m => MapGetAllCodec.DecodeResponse(m).entrySet);
            foreach (var entry in entrySet)
            {
                var value = ToObject<V>(entry.Value);
                var key_1 = ToObject<K>(entry.Key);
                result[key_1] = value;
                if (_nearCache != null)
                {
                    _nearCache.Put(entry.Key, value);
                }
            }
            return result;
        }

        public ICollection<V> Values()
        {
            var request = MapValuesCodec.EncodeRequest(GetName());
            var list = Invoke(request, m=> MapValuesCodec.DecodeResponse(m).list);
            ICollection<V> collection = new List<V>(list.Count);
            foreach (var data in list)
            {
                var value = ToObject<V>(data);
                collection.Add(value);
            }
            return collection;
        }

        public ISet<KeyValuePair<K, V>> EntrySet()
        {
            var request = MapEntrySetCodec.EncodeRequest(GetName());
            var entries = Invoke(request, m => MapEntrySetCodec.DecodeResponse(m).entrySet);
            ISet<KeyValuePair<K, V>> entrySet = new HashSet<KeyValuePair<K, V>>();
            foreach (var entry in entries)
            {
                var key = ToObject<K>(entry.Key);
                var value = ToObject<V>(entry.Value);
                entrySet.Add(new KeyValuePair<K, V>(key, value));
            }
            return entrySet;
        }

        public void AddIndex(string attribute, bool ordered)
        {
            var request = MapAddIndexCodec.EncodeRequest(GetName(), attribute, ordered);
            Invoke(request);
        }

        public void Set(K key, V value)
        {
            Set(key, value, -1, TimeUnit.SECONDS);
        }

        public int Size()
        {
            var request = MapSizeCodec.EncodeRequest(GetName());
            return Invoke(request, m=> MapSizeCodec.DecodeResponse(m).response);
        }

        public bool IsEmpty()
        {
            var request = MapIsEmptyCodec.EncodeRequest(GetName());
            return Invoke(request, m => MapIsEmptyCodec.DecodeResponse(m).response);
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
            var request = MapPutAllCodec.EncodeRequest(GetName(), map);
            Invoke(request);
        }

        public void Clear()
        {
            var request = MapClearCodec.EncodeRequest(GetName());
            Invoke(request);
            if (_nearCache != null)
            {
                _nearCache.InvalidateAll();
            }
        }

        public string AddEntryListener(IEntryListener<K, V> listener, IPredicate<K, V> predicate, K _key,
            bool includeValue)
        {
            var keyData = ToData(_key);
            var predicateData = ToData(predicate);
            var request = MapAddEntryListenerToKeyWithPredicateCodec.EncodeRequest(GetName(), keyData, predicateData,
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
            var request = MapAddEntryListenerWithPredicateCodec.EncodeRequest(GetName(), predicateData, includeValue);
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
            var request = MapKeySetWithPredicateCodec.EncodeRequest(GetName(), ToData(predicate));
            var keys = Invoke(request, m => MapKeySetWithPredicateCodec.DecodeResponse(m).set);
            var keySet = new HashSet<K>();
            foreach (var item in keys)
            {
                var key = ToObject<K>(item);
                keySet.Add(key);
            }
            return keySet;
        }

        public ISet<KeyValuePair<K, V>> EntrySet(IPredicate<K, V> predicate)
        {
            var request = MapEntriesWithPredicateCodec.EncodeRequest(GetName(), ToData(predicate));
            var entries = Invoke(request, m => MapEntriesWithPredicateCodec.DecodeResponse(m).entrySet);
            ISet<KeyValuePair<K, V>> entrySet = new HashSet<KeyValuePair<K, V>>();
            foreach (var dataEntry in entries)
            {
                var key = ToObject<K>(dataEntry.Key);
                var value = ToObject<V>(dataEntry.Value);
                entrySet.Add(new KeyValuePair<K, V>(key, value));
            }
            return entrySet;
        }

        public ICollection<V> Values(IPredicate<K, V> predicate)
        {
            var request = MapValuesWithPredicateCodec.EncodeRequest(GetName(), ToData(predicate));
            var result = Invoke(request, m => MapValuesWithPredicateCodec.DecodeResponse(m).list);
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
            if (_nearCache != null)
            {
                _nearCache.Destroy();
            }
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
                    listener.EntryAdded(new EntryEvent<K, V>(GetName(), member, eventType, key, oldValue, value));
                    break;
                }
                case EntryEventType.Removed:
                {
                    listener.EntryRemoved(new EntryEvent<K, V>(GetName(), member, eventType, key, oldValue, value));
                    break;
                }
                case EntryEventType.Updated:
                {
                    listener.EntryUpdated(new EntryEvent<K, V>(GetName(), member, eventType, key, oldValue, value));
                    break;
                }
                case EntryEventType.Evicted:
                {
                    listener.EntryEvicted(new EntryEvent<K, V>(GetName(), member, eventType, key, oldValue, value));
                    break;
                }
                case EntryEventType.EvictAll:
                {
                    listener.MapEvicted(new MapEvent(GetName(), member, eventType, numberOfAffectedEntries));
                    break;
                }
                case EntryEventType.ClearAll:
                {
                    listener.MapCleared(new MapEvent(GetName(), member, eventType, numberOfAffectedEntries));
                    break;
                }
            }
        }

        internal override void PostInit()
        {
            if (_nearCacheInitialized.CompareAndSet(false, true))
            {
                var nearCacheConfig = GetContext().GetClientConfig().GetNearCacheConfig(GetName());
                if (nearCacheConfig == null)
                {
                    return;
                }
                var _nearCache = new ClientNearCache(GetName(), ClientNearCacheType.Map, GetContext(), nearCacheConfig);
                this._nearCache = _nearCache;
            }
        }

        private void InvalidateNearCacheEntry(IData key)
        {
            if (_nearCache != null && _nearCache.invalidateOnChange)
            {
                _nearCache.Invalidate(key);
            }
        }
    }
}