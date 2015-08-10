using System;
using System.Collections.Generic;
using System.Linq;
using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Client.Request.Multimap;
using Hazelcast.Client.Spi;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;
using Hazelcast.Util;

namespace Hazelcast.Client.Proxy
{
    internal class ClientMultiMapProxy<K, V> : ClientProxy, IMultiMap<K, V>
    {
        public ClientMultiMapProxy(string serviceName, string name) : base(serviceName, name)
        {
        }

        public virtual bool Put(K key, V value)
        {
            var keyData = ToData(key);
            var valueData = ToData(value);
            var request = MultiMapPutCodec.EncodeRequest(GetName(), keyData, valueData, ThreadUtil.GetThreadId());
            
            return Invoke(request, keyData, m => MultiMapPutCodec.DecodeResponse(m).response);
        }

        public virtual ICollection<V> Get(K key)
        {
            var keyData = ToData(key);
            var request = MultiMapGetCodec.EncodeRequest(GetName(), keyData, ThreadUtil.GetThreadId());
            var list = Invoke(request, keyData, m => MultiMapGetCodec.DecodeResponse(m).list);

            return ToList<V>(list);
        }

        public virtual bool Remove(object key, object value)
        {
            var keyData = ToData(key);
            var valueData = ToData(value);

            var request = MultiMapRemoveEntryCodec.EncodeRequest(GetName(), keyData, valueData, ThreadUtil.GetThreadId());
            return Invoke(request, keyData, m => MultiMapRemoveEntryCodec.DecodeResponse(m).response);
        }

        public virtual ICollection<V> Remove(object key)
        {
            var keyData = ToData(key);

            var request = MultiMapRemoveCodec.EncodeRequest(GetName(), keyData, ThreadUtil.GetThreadId());
            var list = Invoke(request, keyData, m => MultiMapRemoveCodec.DecodeResponse(request).list);
            return ToList<V>(list);
        }

        public virtual ISet<K> KeySet()
        {
            var request = MultiMapKeySetCodec.EncodeRequest(GetName());
            var keySet = Invoke(request, m => MultiMapKeySetCodec.DecodeResponse(m).list);

            return ToSet<K>(keySet);
        }

        public virtual ICollection<V> Values()
        {
            var request = MultiMapValuesCodec.EncodeRequest(GetName());
            var result = Invoke(request, m => MultiMapValuesCodec.DecodeResponse(m).list);
            return ToList<V>(result);
        }

        public virtual ISet<KeyValuePair<K, V>> EntrySet()
        {
            var request = MultiMapEntrySetCodec.EncodeRequest(GetName());
            var dataEntrySet = Invoke(request, m => MultiMapEntrySetCodec.DecodeResponse(m).entrySet);
            return ToEntrySet<K, V>(dataEntrySet);
        }

        public virtual bool ContainsKey(K key)
        {
            var keyData = ToData(key);
            var request = MultiMapContainsKeyCodec.EncodeRequest(GetName(), keyData, ThreadUtil.GetThreadId());
            return Invoke(request, keyData, m => MultiMapContainsKeyCodec.DecodeResponse(m).response);
        }

        public virtual bool ContainsValue(object value)
        {
            var valueData = ToData(value);
            var request = MultiMapContainsValueCodec.EncodeRequest(GetName(), valueData);
            return Invoke(request, m => MultiMapContainsValueCodec.DecodeResponse(request).response);
        }

        public virtual bool ContainsEntry(K key, V value)
        {
            var keyData = ToData(key);
            var valueData = ToData(value);
            var request = MultiMapContainsEntryCodec.EncodeRequest(GetName(), keyData, valueData, ThreadUtil.GetThreadId());
            return Invoke(request, keyData, m => MultiMapContainsEntryCodec.DecodeResponse(m).response);
        }

        public virtual int Size()
        {
            var request = MultiMapSizeCodec.EncodeRequest(GetName());
            return Invoke(request, m => MultiMapSizeCodec.DecodeResponse(m).response);
        }

        public virtual void Clear()
        {
            var request = MultiMapClearCodec.EncodeRequest(GetName());
            Invoke(request);
        }

        public virtual int ValueCount(K key)
        {
            var keyData = ToData(key);
            var request = MultiMapValueCountCodec.EncodeRequest(GetName(), keyData, ThreadUtil.GetThreadId());
            return Invoke(request, keyData, m => MultiMapValueCountCodec.DecodeResponse(m).response);
        }

        public virtual string AddEntryListener(IEntryListener<K, V> listener, bool includeValue)
        {
            var request = MultiMapAddEntryListenerCodec.EncodeRequest(GetName(), includeValue);

            DistributedEventHandler handler =
                eventData => MultiMapAddEntryListenerCodec.AbstractEventHandler.Handle(eventData, 
                    (key, value, oldValue, mergingValue, type, uuid, entries) =>
                        OnEntryEvent(key, value, oldValue, mergingValue, (EntryEventType)type, uuid, entries, includeValue, listener)
                   );

            return Listen(request, message => MultiMapAddEntryListenerCodec.DecodeResponse(message).response, handler);
        }

        public virtual bool RemoveEntryListener(string registrationId)
        {
            var request = MultiMapRemoveEntryListenerCodec.EncodeRequest(GetName(), registrationId);
            return StopListening(request, m => MultiMapRemoveEntryListenerCodec.DecodeResponse(m).response, registrationId);
        }

        public virtual string AddEntryListener(IEntryListener<K, V> listener, K key, bool includeValue)
        {
            var keyData = ToData(key);
            var request = MultiMapAddEntryListenerToKeyCodec.EncodeRequest(GetName(), keyData, includeValue);

            DistributedEventHandler handler =
                eventData => MultiMapAddEntryListenerToKeyCodec.AbstractEventHandler.Handle(eventData,
                    (thekey, value, oldValue, mergingValue, type, uuid, entries) =>
                        OnEntryEvent(thekey, value, oldValue, mergingValue, (EntryEventType)type, uuid, entries, includeValue, listener)
                   );

            return Listen(request, message => MultiMapAddEntryListenerToKeyCodec.DecodeResponse(message).response, handler);
        }

        public virtual void Lock(K key)
        {
            ThrowExceptionIfNull(key);

            Lock(key, long.MaxValue, TimeUnit.MILLISECONDS);
        }

        public virtual void Lock(K key, long leaseTime, TimeUnit timeUnit)
        {
            ThrowExceptionIfNull(key);

            var keyData = ToData(key);
            var request = MultiMapLockCodec.EncodeRequest(GetName(), keyData, ThreadUtil.GetThreadId(),
                timeUnit.ToMillis(leaseTime));
            Invoke(request, keyData);
        }

        public virtual bool IsLocked(K key)
        {
            ThrowExceptionIfNull(key);

            var keyData = ToData(key);
            var request = MultiMapIsLockedCodec.EncodeRequest(GetName(), keyData);
            return Invoke(request, keyData, m => MultiMapIsLockedCodec.DecodeResponse(m).response);
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
            return TryLock(key, time, timeunit, long.MaxValue, TimeUnit.MILLISECONDS);
        }

        /// <exception cref="System.Exception"></exception>
        public virtual bool TryLock(K key, long timeout, TimeUnit timeunit, long leaseTime, TimeUnit leaseUnit)
        {
            ThrowExceptionIfNull(key);

            var keyData = ToData(key);
            var request = MultiMapTryLockCodec.EncodeRequest(GetName(), keyData, ThreadUtil.GetThreadId(), leaseUnit.ToMillis(leaseTime),
                timeunit.ToMillis(timeout));
            return Invoke(request, keyData, m => MultiMapTryLockCodec.DecodeResponse(m).response);
        }

        public virtual void Unlock(K key)
        {
            ThrowExceptionIfNull(key);

            var keyData = ToData(key);
            var request = MultiMapUnlockCodec.EncodeRequest(GetName(), keyData, ThreadUtil.GetThreadId());
            Invoke(request, keyData);
        }

        public virtual void ForceUnlock(K key)
        {
            ThrowExceptionIfNull(key);

            var keyData = ToData(key);
            var request = MultiMapForceUnlockCodec.EncodeRequest(GetName(), keyData);
            Invoke(request, keyData);
        }

        public void OnEntryEvent(IData keyData, IData valueData, IData oldValueData, IData mergingValueData,
            EntryEventType eventType, string uuid,
            int numberOfAffectedEntries, bool includeValue, IEntryListener<K, V> listener)
        {
            V value = default(V);
            V oldValue = default(V);
            if (includeValue)
            {
                value = ToObject<V>(valueData);
                oldValue = ToObject<V>(oldValueData);
            }
            var key = ToObject<K>(keyData);
            var member = GetContext().GetClusterService().GetMember(uuid);
            switch (eventType)
            {
                case EntryEventType.Added:
                {
                    listener.EntryAdded(new EntryEvent<K, V>(GetName(), member, eventType, key, oldValue,
                        value));
                    break;
                }
                case EntryEventType.Removed:
                {
                    listener.EntryRemoved(new EntryEvent<K, V>(GetName(), member, eventType, key, oldValue,
                        value));
                    break;
                }
                case EntryEventType.ClearAll:
                {
                    listener.MapCleared(new MapEvent(GetName(), member, eventType,
                        numberOfAffectedEntries));
                    break;
                }
            }
        }
    }
}