using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hazelcast.Client;
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


    public sealed class ClientMapProxy<K, V> : ClientProxy, IHazelcastMap<K, V> //, IMap<K, V>
	{
		private readonly string name;

		private volatile ClientNearCache nearCache;

		private string nearCacheListenerId;

		private readonly AtomicBoolean nearCacheInitialized = new AtomicBoolean();

		public ClientMapProxy(string serviceName, string name) : base(serviceName, name)
		{
			this.name = name;
		}

		public bool ContainsKey(object key)
		{
			Data keyData = ToData(key);
			MapContainsKeyRequest request = new MapContainsKeyRequest(name, keyData);
			bool result = Invoke<bool>(request, keyData);
			return result;
		}

		public bool ContainsValue(object value)
		{
			Data valueData = ToData(value);
			MapContainsValueRequest request = new MapContainsValueRequest(name, valueData);
            bool result = Invoke<bool>(request);
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
					return (V)cached;
				}
			}
			MapGetRequest request = new MapGetRequest(name, keyData);
			V result = Invoke<V>(request, keyData);
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
			MapRemoveRequest request = new MapRemoveRequest(name, keyData, ThreadUtil.GetThreadId());
			return Invoke<V>(request, keyData);
		}

		public bool Remove(object key, object value)
		{
			Data keyData = ToData(key);
			Data valueData = ToData(value);
			MapRemoveIfSameRequest request = new MapRemoveIfSameRequest(name, keyData, valueData, ThreadUtil.GetThreadId());
			bool result = Invoke<bool>(request, keyData);
			return result;
		}

		public void Delete(object key)
		{
			Data keyData = ToData(key);
			MapDeleteRequest request = new MapDeleteRequest(name, keyData, ThreadUtil.GetThreadId());
			Invoke<object>(request, keyData);
		}

		public void Flush()
		{
			MapFlushRequest request = new MapFlushRequest(name);
			Invoke<object>(request);
		}

		public Task<V> GetAsync(K key)
		{
            return GetContext().GetExecutionService().Submit(v => Get(key));
		}

		public Task<V> PutAsync(K key, V value)
		{
			return PutAsync(key, value, -1, TimeUnit.SECONDS);
		}

		public Task<V> PutAsync(K key, V value, long ttl, TimeUnit timeunit)
		{
			Task<V> f = GetContext().GetExecutionService().Submit(v=>Put(key, value, ttl, timeunit));
			return f;
		}

		public Task<V> RemoveAsync(K key)
		{
            Task<V> f = GetContext().GetExecutionService().Submit(v => Remove(key));
			return f;
		}

		public bool TryRemove(K key, long timeout, TimeUnit timeunit)
		{
			Data keyData = ToData(key);
			MapTryRemoveRequest request = new MapTryRemoveRequest(name, keyData, ThreadUtil.GetThreadId(), timeunit.ToMillis(timeout));
			bool result = Invoke<bool>(request, keyData);
			return result;
		}

		public bool TryPut(K key, V value, long timeout, TimeUnit timeunit)
		{
			Data keyData = ToData(key);
			Data valueData = ToData(value);
			MapTryPutRequest request = new MapTryPutRequest(name, keyData, valueData, ThreadUtil.GetThreadId(), timeunit.ToMillis(timeout));
			bool result = Invoke<bool>(request, keyData);
			return result;
		}

		public V Put(K key, V value, long ttl, TimeUnit timeunit)
		{
			Data keyData = ToData(key);
			Data valueData = ToData(value);
			MapPutRequest request = new MapPutRequest(name, keyData, valueData, ThreadUtil.GetThreadId(), GetTimeInMillis(ttl, timeunit));
			return Invoke<V>(request, keyData);
		}

		public void PutTransient(K key, V value, long ttl, TimeUnit timeunit)
		{
			Data keyData = ToData(key);
			Data valueData = ToData(value);
			MapPutTransientRequest request = new MapPutTransientRequest(name, keyData, valueData, ThreadUtil.GetThreadId(), GetTimeInMillis(ttl, timeunit));
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
			MapPutIfAbsentRequest request = new MapPutIfAbsentRequest(name, keyData, valueData, ThreadUtil.GetThreadId(), GetTimeInMillis(ttl, timeunit));
			return Invoke<V>(request, keyData);
		}

		public bool Replace(K key, V oldValue, V newValue)
		{
			Data keyData = ToData(key);
			Data oldValueData = ToData(oldValue);
			Data newValueData = ToData(newValue);
			MapReplaceIfSameRequest request = new MapReplaceIfSameRequest(name, keyData, oldValueData, newValueData, ThreadUtil.GetThreadId());
			bool result = Invoke<bool>(request, keyData);
			return result;
		}

		public V Replace(K key, V value)
		{
			Data keyData = ToData(key);
			Data valueData = ToData(value);
			MapReplaceRequest request = new MapReplaceRequest(name, keyData, valueData, ThreadUtil.GetThreadId());
            return Invoke<V>(request, keyData);
		}

		public void Set(K key, V value, long ttl, TimeUnit timeunit)
		{
			Data keyData = ToData(key);
			Data valueData = ToData(value);
			MapSetRequest request = new MapSetRequest(name, keyData, valueData, ThreadUtil.GetThreadId(), GetTimeInMillis(ttl, timeunit));
            Invoke<object>(request, keyData);
		}

		public void Lock(K key)
		{
			Data keyData = ToData(key);
			MapLockRequest request = new MapLockRequest(name, keyData, ThreadUtil.GetThreadId());
            Invoke<object>(request, keyData);
		}

		public void Lock(K key, long leaseTime, TimeUnit timeUnit)
		{
			Data keyData = ToData(key);
			MapLockRequest request = new MapLockRequest(name, keyData, ThreadUtil.GetThreadId(), GetTimeInMillis(leaseTime, timeUnit), -1);
            Invoke<object>(request, keyData);
		}

		public bool IsLocked(K key)
		{
			Data keyData = ToData(key);
			MapIsLockedRequest request = new MapIsLockedRequest(name, keyData);
            bool result = Invoke<bool>(request, keyData);
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
			MapLockRequest request = new MapLockRequest(name, keyData, ThreadUtil.GetThreadId(), long.MaxValue, GetTimeInMillis(time, timeunit));
			bool result = Invoke<bool>(request, keyData);
			return result;
		}

		public void Unlock(K key)
		{
			Data keyData = ToData(key);
			MapUnlockRequest request = new MapUnlockRequest(name, keyData, ThreadUtil.GetThreadId(), false);
            Invoke<object>(request, keyData);
		}

		public void ForceUnlock(K key)
		{
			Data keyData = ToData(key);
			MapUnlockRequest request = new MapUnlockRequest(name, keyData, ThreadUtil.GetThreadId(), true);
            Invoke<object>(request, keyData);
		}

		public string AddLocalEntryListener(IEntryListener<K, V> listener)
		{
			throw new NotSupportedException("Locality is ambiguous for client!!!");
		}

		public string AddInterceptor(MapInterceptor interceptor)
		{
			MapAddInterceptorRequest request = new MapAddInterceptorRequest(name, interceptor);
            return Invoke<string>(request);
		}

		public void RemoveInterceptor(string id)
		{
			MapRemoveInterceptorRequest request = new MapRemoveInterceptorRequest(name, id);
            Invoke<object>(request);
		}

		public string AddEntryListener(IEntryListener<K, V> listener, bool includeValue)
		{
            MapAddEntryListenerRequest request = new MapAddEntryListenerRequest(name, includeValue);
            EventHandler<PortableEntryEvent> handler = (sender, _event) => OnEntryEvent(_event, includeValue, listener);
            return Listen(request, handler);
		}

		public bool RemoveEntryListener(string id)
		{
			return StopListening(id);
		}

		public string AddEntryListener(IEntryListener<K, V> listener, K key, bool includeValue)
		{
            Data keyData = ToData(key);
            MapAddEntryListenerRequest request = new MapAddEntryListenerRequest(name, keyData, includeValue);
            EventHandler<PortableEntryEvent> handler = (sender, _event) => OnEntryEvent(_event, includeValue, listener);
		    return Listen(request, keyData, handler);
		}

		public string AddEntryListener(IEntryListener<K, V> listener, IPredicate<K, V> predicate, K key, bool includeValue)
		{
            throw new NotSupportedException("EntryView not implemented");
            //Data keyData = ToData(key);
            //MapAddEntryListenerRequest request = new MapAddEntryListenerRequest(name, keyData, includeValue, predicate);
            //EventHandler<PortableEntryEvent> handler = CreateHandler(listener, includeValue);
            //return Listen(request, keyData, handler);
		}

		public string AddEntryListener(IEntryListener<K, V> listener, IPredicate<K, V> predicate, bool includeValue)
		{
            throw new NotSupportedException("EntryView not implemented");

            //MapAddEntryListenerRequest request = new MapAddEntryListenerRequest(name, null, includeValue, predicate);
            //EventHandler<PortableEntryEvent> handler = CreateHandler(listener, includeValue);
            //return Listen(request, null, handler);
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
			MapEvictRequest request = new MapEvictRequest(name, keyData, ThreadUtil.GetThreadId());
            bool result = Invoke<bool>(request);
			return result;
		}

		public ICollection<K> Keys()
		{
			MapKeySetRequest request = new MapKeySetRequest(name);
            MapKeySet mapKeySet = Invoke<MapKeySet>(request);
			ICollection<Data> keySetData = mapKeySet.GetKeySet();
			ICollection<K> keySet = new HashSet<K>();
			foreach (Data data in keySetData)
			{
				K key = ToObject<K>(data);
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
			MapGetAllRequest request = new MapGetAllRequest(name, keySet);
            MapEntrySet mapEntrySet = Invoke<MapEntrySet>(request);
			IDictionary<K, V> result = new Dictionary<K, V>();
			ICollection<KeyValuePair<Data, Data>> entrySet = mapEntrySet.GetEntrySet();
			foreach (KeyValuePair<Data, Data> dataEntry in entrySet)
			{
                result.Add(ToObject<K>(dataEntry.Key), ToObject<V>(dataEntry.Value));
			}
			return result;
		}

		public ICollection<V> Values()
		{
			MapValuesRequest request = new MapValuesRequest(name);
            MapValueCollection mapValueCollection = Invoke<MapValueCollection>(request);
			ICollection<Data> collectionData = mapValueCollection.GetValues();
			ICollection<V> collection = new List<V>(collectionData.Count);
			foreach (Data data in collectionData)
			{
				V value = ToObject<V>(data);
				collection.Add(value);
			}
			return collection;
		}

		public ICollection<KeyValuePair<K, V>> EntrySet()
		{
		    throw new NotSupportedException("");
            //MapEntrySetRequest request = new MapEntrySetRequest(name);
            //MapEntrySet result = Invoke<MapEntrySet>(request);
            //ICollection<KeyValuePair<K, V>> entrySet = new HashSet<KeyValuePair<K, V>>();
            //ICollection<KeyValuePair<Data, Data>> entries = result.GetEntrySet();
            //foreach (KeyValuePair<Data, Data> dataEntry in entries)
            //{
            //    Data keyData = dataEntry.Key;
            //    Data valueData = dataEntry.Value;
            //    K key = ToObject<K>(keyData);
            //    V value = ToObject<V>(valueData);
            //    entrySet.Add(new KeyValuePair<K, V>(key, value));
            //}
            //return entrySet;
		}

		public ICollection<K> KeySet(IPredicate<K,V> predicate)
		{
            throw new NotSupportedException("");
            //var request = new MapQueryRequest<K,V>(name, predicate, IterationType.Key);
            //var result = Invoke<QueryResultSet>(request);
            //ICollection<K> keySet = new HashSet<K>();
            //foreach (object data in result)
            //{
            //    K key = ToObject<K>((Data)data);
            //    keySet.Add(key);
            //}
            //return keySet;
		}

        public ICollection<KeyValuePair<K, V>> EntrySet(IPredicate<K, V> predicate)
		{
            throw new NotSupportedException("");
            //var request = new MapQueryRequest<K, V>(name, predicate, IterationType.Entry);
            //var result = Invoke<QueryResultSet>(request);
            //ICollection<KeyValuePair<K, V>> entrySet = new HashSet<KeyValuePair<K, V>>();
            //foreach (object data in result)
            //{
            //    KeyValuePair<Data, Data> dataEntry = (KeyValuePair<Data, Data>)data;
            //    K key = ToObject<K>(dataEntry.Key);
            //    V value = ToObject<V>(dataEntry.Value);
            //    entrySet.Add(new KeyValuePair<K, V>(key, value));
            //}
            //return entrySet;
		}

		public ICollection<V> Values(IPredicate<K,V> predicate)
		{
            throw new NotSupportedException("");
            //var request = new MapQueryRequest<K, V>(name, predicate, IterationType.Value);
            //var result = Invoke<QueryResultSet>(request);
            //ICollection<V> values = new List<V>(result.Count);
            //foreach (object data in result)
            //{
            //    V value = ToObject<V>((Data)data);
            //    values.Add(value);
            //}
            //return values;
		}

		public ICollection<K> LocalKeySet()
		{
			throw new NotSupportedException("Locality is ambiguous for client!!!");
		}

        public ICollection<K> LocalKeySet(IPredicate<K, V> predicate)
		{
			throw new NotSupportedException("Locality is ambiguous for client!!!");
		}

		public void AddIndex(string attribute, bool ordered)
		{
			var request = new MapAddIndexRequest(name, attribute, ordered);
			Invoke<object>(request);
		}

		//    public LocalMapStats getLocalMapStats() {
		//        throw new UnsupportedOperationException("Locality is ambiguous for client!!!");
		//    }
		public object ExecuteOnKey(K key, EntryProcessor<K,V> entryProcessor)
		{
			throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT");
		}

		//        final Data keyData = toData(key);
		//        MapExecuteOnKeyRequest request = new MapExecuteOnKeyRequest(name, entryProcessor, keyData);
		//        return invoke(request, keyData);
        public IDictionary<K, object> ExecuteOnEntries(EntryProcessor<K, V> entryProcessor)
		{
			throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT");
		}

		//        MapExecuteOnAllKeysRequest request = new MapExecuteOnAllKeysRequest(name, entryProcessor);
		//        MapEntrySet entrySet = invoke(request);
		//        Map<K, Object> result = new HashMap<K, Object>();
		//        for (Entry<Data, Data> dataEntry : entrySet.getEntrySet()) {
		//            final Data keyData = dataEntry.getKey();
		//            final Data valueData = dataEntry.getValue();
		//            K key = toObject(keyData);
		//            result.put(key, toObject(valueData));
		//        }
		//        return result;
        public IDictionary<K, object> ExecuteOnEntries(EntryProcessor<K, V> entryProcessor, IPredicate<K, V> predicate)
		{
			throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT");
		}

		//        MapExecuteWithPredicateRequest request = new MapExecuteWithPredicateRequest(name, entryProcessor, predicate);
		//        MapEntrySet entrySet = invoke(request);
		//        Map<K, Object> result = new HashMap<K, Object>();
		//        for (Entry<Data, Data> dataEntry : entrySet.getEntrySet()) {
		//            final Data keyData = dataEntry.getKey();
		//            final Data valueData = dataEntry.getValue();
		//            K key = toObject(keyData);
		//            result.put(key, toObject(valueData));
		//        }
		//        return result;
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

		public void PutAll<K>(IDictionary<K,V> m) 
		{
			MapEntrySet entrySet = new MapEntrySet();
			foreach (KeyValuePair<K, V> entry in m)
			{
                entrySet.Add(new KeyValuePair<Data, Data>(ToData(entry.Key), ToData(entry.Value)));
			}
			MapPutAllRequest request = new MapPutAllRequest(name, entrySet);
			Invoke<object>(request);
		}

		public void Clear()
		{
			MapClearRequest request = new MapClearRequest(name);
			Invoke<object>(request);
		}

		protected internal override void OnDestroy()
		{
			if (nearCacheListenerId != null)
			{
				RemoveEntryListener(nearCacheListenerId);
			}
			if (nearCache != null)
			{
				nearCache.Clear();
			}
		}

		private Data ToData(object o)
		{
			return GetContext().GetSerializationService().ToData(o);
		}

		private T ToObject<T>(Data data)
		{
			return (T)GetContext().GetSerializationService().ToObject(data);
		}

		private T Invoke<T>(object req, Data keyData)
		{
			try
			{
				return GetContext().GetInvocationService().InvokeOnKeyOwner<T>(req, keyData);
			}
			catch (Exception e)
			{
				throw ExceptionUtil.Rethrow(e);
			}
		}

		private T Invoke<T>(object req)
		{
			try
			{
				return GetContext().GetInvocationService().InvokeOnRandomTarget<T>(req);
			}
			catch (Exception e)
			{
				throw ExceptionUtil.Rethrow(e);
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
            K key = ToObject<K>(_event.GetKey());
            IMember member = GetContext().GetClusterService().GetMember(_event.GetUuid());
            EntryEvent<K, V> entryEvent = new EntryEvent<K, V>(name, member, _event.GetEventType(), key, oldValue, value);
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
                ClientNearCache _nearCache = new ClientNearCache(name, GetContext(), nearCacheConfig);
                if (nearCacheConfig.IsInvalidateOnChange())
                {
                    try
                    {
                        nearCacheListenerId = AddEntryListener(new _NearCacheEntryListener(this), false);
                    }
                    catch (Exception e)
                    {
                        _nearCache = null;
                        //                    nearCacheInitialized.set(false);
                        Logger.GetLogger(typeof(ClientMapProxy<,>)).Severe("-----------------\n Near Cache is not initialized!!! \n-----------------", e);
                    }
                }
                nearCache = _nearCache;
            }
		}

        private sealed class _NearCacheEntryListener : IEntryListener<K, V>
        {
            public _NearCacheEntryListener(ClientMapProxy<K, V> _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public void EntryAdded(EntryEvent<K, V> _event)
            {
                this.Invalidate(_event);
            }

            public void EntryRemoved(EntryEvent<K, V> _event)
            {
                this.Invalidate(_event);
            }

            public void EntryUpdated(EntryEvent<K, V> _event)
            {
                this.Invalidate(_event);
            }

            public void EntryEvicted(EntryEvent<K, V> _event)
            {
                this.Invalidate(_event);
            }

            internal void Invalidate(EntryEvent<K, V> _event)
            {
                Data key = this._enclosing.ToData(_event.GetKey());
                this._enclosing.nearCache.Invalidate(key);
            }

            private readonly ClientMapProxy<K, V> _enclosing;
        }

	}
 
}
