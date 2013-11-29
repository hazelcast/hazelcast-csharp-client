using System;
using System.Collections.Generic;
using Hazelcast.Client.Request.Multimap;
using Hazelcast.Client.Spi;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;
using Hazelcast.Util;

namespace Hazelcast.Client.Proxy
{
    public class ClientMultiMapProxy<K, V> : ClientProxy, IMultiMap<K, V>
    {
        private readonly string name;

        public ClientMultiMapProxy(string serviceName, string name) : base(serviceName, name)
        {
            this.name = name;
        }

        public virtual bool Put(K key, V value)
        {
            Data keyData = GetSerializationService().ToData(key);
            Data valueData = GetSerializationService().ToData(value);
            var request = new PutRequest(name, keyData, valueData, -1, ThreadUtil.GetThreadId());
            var result = Invoke<bool>(request, keyData);
            return result;
        }

        public virtual ICollection<V> Get(K key)
        {
            Data keyData = GetSerializationService().ToData(key);
            var request = new GetAllRequest(name, keyData);
            var result = Invoke<PortableCollection>(request, keyData);
            return ToObjectCollection<V>(result, true);
        }

        public virtual bool Remove(object key, object value)
        {
            Data keyData = GetSerializationService().ToData(key);
            Data valueData = GetSerializationService().ToData(value);
            var request = new RemoveRequest(name, keyData, valueData, ThreadUtil.GetThreadId());
            var result = Invoke<bool>(request, keyData);
            return result;
        }

        public virtual ICollection<V> Remove(object key)
        {
            Data keyData = GetSerializationService().ToData(key);
            var request = new RemoveAllRequest(name, keyData, ThreadUtil.GetThreadId());
            var result = Invoke<PortableCollection>(request, keyData);
            return ToObjectCollection<V>(result, true);
        }

        public virtual ICollection<K> LocalKeySet()
        {
            throw new NotSupportedException("Locality for client is ambiguous");
        }

        public virtual ICollection<K> KeySet()
        {
            var request = new KeySetRequest(name);
            var result = Invoke<PortableCollection>(request);
            return ToObjectCollection<K>(result, false);
        }

        public virtual ICollection<V> Values()
        {
            var request = new ValuesRequest(name);
            var result = Invoke<PortableCollection>(request);
            return ToObjectCollection<V>(result, true);
        }

        public virtual ICollection<KeyValuePair<K, V>> EntrySet()
        {
            var request = new EntrySetRequest(name);
            var result = Invoke<PortableEntrySetResponse>(request);
            ICollection<KeyValuePair<Data, Data>> dataEntrySet = result.GetEntrySet();
            ICollection<KeyValuePair<K, V>> entrySet = new HashSet<KeyValuePair<K, V>>();
            foreach (var entry in dataEntrySet)
            {
                var key = (K) GetSerializationService().ToObject(entry.Key);
                var val = (V) GetSerializationService().ToObject(entry.Value);
                entrySet.Add(new KeyValuePair<K, V>(key, val));
            }
            return entrySet;
        }

        public virtual bool ContainsKey(K key)
        {
            Data keyData = GetSerializationService().ToData(key);
            var request = new ContainsEntryRequest(name, keyData, null);
            var result = Invoke<bool>(request, keyData);
            return result;
        }

        public virtual bool ContainsValue(object value)
        {
            Data valueData = GetSerializationService().ToData(value);
            var request = new ContainsEntryRequest(name, null, valueData);
            var result = Invoke<bool>(request);
            return result;
        }

        public virtual bool ContainsEntry(K key, V value)
        {
            Data keyData = GetSerializationService().ToData(key);
            Data valueData = GetSerializationService().ToData(value);
            var request = new ContainsEntryRequest(name, keyData, valueData);
            var result = Invoke<bool>(request, keyData);
            return result;
        }

        public virtual int Size()
        {
            var request = new SizeRequest(name);
            var result = Invoke<int>(request);
            return result;
        }

        public virtual void Clear()
        {
            var request = new ClearRequest(name);
            Invoke<object>(request);
        }

        public virtual int ValueCount(K key)
        {
            Data keyData = GetSerializationService().ToData(key);
            var request = new CountRequest(name, keyData);
            var result = Invoke<int>(request, keyData);
            return result;
        }

        public virtual string AddLocalEntryListener(IEntryListener<K, V> listener)
        {
            throw new NotSupportedException("Locality for client is ambiguous");
        }

        public virtual string AddEntryListener(IEntryListener<K, V> listener, bool includeValue)
        {
            var request = new AddEntryListenerRequest(name, null, includeValue);
            EventHandler<PortableEntryEvent> handler = (sender, _event) => OnEntryEvent(_event, includeValue, listener);
            return Listen(request, handler);
        }

        public virtual bool RemoveEntryListener(string registrationId)
        {
            return StopListening(registrationId);
        }

        public virtual string AddEntryListener(IEntryListener<K, V> listener, K key, bool includeValue)
        {
            Data keyData = GetSerializationService().ToData(key);
            var request = new AddEntryListenerRequest(name, keyData, includeValue);
            EventHandler<PortableEntryEvent> handler = (sender, _event) => OnEntryEvent(_event, includeValue, listener);
            return Listen(request, keyData, handler);
        }

        public virtual void Lock(K key)
        {
            Data keyData = GetSerializationService().ToData(key);
            var request = new MultiMapLockRequest(keyData, ThreadUtil.GetThreadId(), name);
            Invoke<object>(request, keyData);
        }

        public virtual void Lock(K key, long leaseTime, TimeUnit timeUnit)
        {
            Data keyData = GetSerializationService().ToData(key);
            var request = new MultiMapLockRequest(keyData, ThreadUtil.GetThreadId(),
                GetTimeInMillis(leaseTime, timeUnit), -1, name);
            Invoke<object>(request, keyData);
        }

        public virtual bool IsLocked(K key)
        {
            Data keyData = GetSerializationService().ToData(key);
            var request = new MultiMapIsLockedRequest(keyData, name);
            var result = Invoke<bool>(request, keyData);
            return result;
        }

        public virtual bool TryLock(K key)
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
        public virtual bool TryLock(K key, long time, TimeUnit timeunit)
        {
            Data keyData = GetSerializationService().ToData(key);
            var request = new MultiMapLockRequest(keyData, ThreadUtil.GetThreadId(), long.MaxValue,
                GetTimeInMillis(time, timeunit), name);
            var result = Invoke<bool>(request, keyData);
            return result;
        }

        public virtual void Unlock(K key)
        {
            Data keyData = GetSerializationService().ToData(key);
            var request = new MultiMapUnlockRequest(keyData, ThreadUtil.GetThreadId(), name);
            Invoke<bool>(request, keyData);
        }

        public virtual void ForceUnlock(K key)
        {
            Data keyData = GetSerializationService().ToData(key);
            var request = new MultiMapUnlockRequest(keyData, ThreadUtil.GetThreadId(), true, name);
            Invoke<bool>(request, keyData);
        }

        //    public LocalMultiMapStats getLocalMultiMapStats() {
        //        throw new UnsupportedOperationException("Locality is ambiguous for client!!!");
        //    }
        protected internal override void OnDestroy()
        {
        }

        private T Invoke<T>(object req, Data key)
        {
            try
            {
                return GetContext().GetInvocationService().InvokeOnKeyOwner<T>(req, key);
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

        private ICollection<T> ToObjectCollection<T>(PortableCollection result, bool list)
        {
            ICollection<Data> coll = result.GetCollection();
            ICollection<T> collection;
            if (list)
            {
                collection = new List<T>(coll == null ? 0 : coll.Count);
            }
            else
            {
                collection = new HashSet<T>();
            }
            if (coll == null)
            {
                return collection;
            }
            foreach (Data data in coll)
            {
                var item = (T) GetSerializationService().ToObject(data);
                collection.Add(item);
            }
            return collection;
        }

        private ISerializationService GetSerializationService()
        {
            return GetContext().GetSerializationService();
        }

        protected internal virtual long GetTimeInMillis(long time, TimeUnit timeunit)
        {
            return timeunit != null ? timeunit.ToMillis(time) : time;
        }

        public void OnEntryEvent(PortableEntryEvent _event, bool includeValue, IEntryListener<K, V> listener)
        {
            V value = default(V);
            V oldValue = default(V);
            if (includeValue)
            {
                value = (V) GetSerializationService().ToObject(_event.GetValue());
                oldValue = (V) GetSerializationService().ToObject(_event.GetOldValue());
            }
            var key = (K) GetSerializationService().ToObject(_event.GetKey());
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
    }
}