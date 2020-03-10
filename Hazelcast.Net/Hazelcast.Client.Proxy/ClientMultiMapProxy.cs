// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections.Generic;
using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Client.Spi;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;
using Hazelcast.Map;
using Hazelcast.Util;

namespace Hazelcast.Client.Proxy
{
    internal class ClientMultiMapProxy<TKey, TValue> : ClientProxy, IMultiMap<TKey, TValue>
    {
        private ClientLockReferenceIdGenerator _lockReferenceIdGenerator;

        public ClientMultiMapProxy(string serviceName, string name, HazelcastClient client) : base(serviceName, name, client)
        {
        }

        public virtual bool Put(TKey key, TValue value)
        {
            var keyData = ToData(key);
            var valueData = ToData(value);
            var request = MultiMapPutCodec.EncodeRequest(Name, keyData, valueData, ThreadUtil.GetThreadId());

            return Invoke(request, keyData, m => MultiMapPutCodec.DecodeResponse(m).Response);
        }

        public virtual ICollection<TValue> Get(TKey key)
        {
            var keyData = ToData(key);
            var request = MultiMapGetCodec.EncodeRequest(Name, keyData, ThreadUtil.GetThreadId());
            var list = Invoke(request, keyData, m => MultiMapGetCodec.DecodeResponse(m).Response);
            return new ReadOnlyLazyList<TValue, IData>(list, Client.SerializationService);
        }

        public virtual bool Remove(object key, object value)
        {
            var keyData = ToData(key);
            var valueData = ToData(value);

            var request = MultiMapRemoveEntryCodec.EncodeRequest(Name, keyData, valueData, ThreadUtil.GetThreadId());
            return Invoke(request, keyData, m => MultiMapRemoveEntryCodec.DecodeResponse(m).Response);
        }

        public virtual ICollection<TValue> Remove(object key)
        {
            var keyData = ToData(key);

            var request = MultiMapRemoveCodec.EncodeRequest(Name, keyData, ThreadUtil.GetThreadId());
            var list = Invoke(request, keyData, m => MultiMapRemoveCodec.DecodeResponse(m).Response);
            return new ReadOnlyLazyList<TValue, IData>(list, Client.SerializationService);
        }

        public virtual ISet<TKey> KeySet()
        {
            var request = MultiMapKeySetCodec.EncodeRequest(Name);
            var result = Invoke(request, m => MultiMapKeySetCodec.DecodeResponse(m).Response);
            return new ReadOnlyLazySet<TKey>(result, Client.SerializationService);
        }

        public virtual ICollection<TValue> Values()
        {
            var request = MultiMapValuesCodec.EncodeRequest(Name);
            var list = Invoke(request, m => MultiMapValuesCodec.DecodeResponse(m).Response);
            return new ReadOnlyLazyList<TValue, IData>(list, Client.SerializationService);
        }

        public virtual ISet<KeyValuePair<TKey, TValue>> EntrySet()
        {
            var request = MultiMapEntrySetCodec.EncodeRequest(Name);
            var dataEntrySet = Invoke(request, m => MultiMapEntrySetCodec.DecodeResponse(m).Response);
            return ToEntrySet<TKey, TValue>(dataEntrySet);
        }

        public virtual bool ContainsKey(TKey key)
        {
            var keyData = ToData(key);
            var request = MultiMapContainsKeyCodec.EncodeRequest(Name, keyData, ThreadUtil.GetThreadId());
            return Invoke(request, keyData, m => MultiMapContainsKeyCodec.DecodeResponse(m).Response);
        }

        public virtual bool ContainsValue(object value)
        {
            var valueData = ToData(value);
            var request = MultiMapContainsValueCodec.EncodeRequest(Name, valueData);
            return Invoke(request, m => MultiMapContainsValueCodec.DecodeResponse(m).Response);
        }

        public virtual bool ContainsEntry(TKey key, TValue value)
        {
            var keyData = ToData(key);
            var valueData = ToData(value);
            var request = MultiMapContainsEntryCodec.EncodeRequest(Name, keyData, valueData,
                ThreadUtil.GetThreadId());
            return Invoke(request, keyData, m => MultiMapContainsEntryCodec.DecodeResponse(m).Response);
        }

        public virtual int Size()
        {
            var request = MultiMapSizeCodec.EncodeRequest(Name);
            return Invoke(request, m => MultiMapSizeCodec.DecodeResponse(m).Response);
        }

        public virtual void Clear()
        {
            var request = MultiMapClearCodec.EncodeRequest(Name);
            Invoke(request);
        }

        public virtual int ValueCount(TKey key)
        {
            var keyData = ToData(key);
            var request = MultiMapValueCountCodec.EncodeRequest(Name, keyData, ThreadUtil.GetThreadId());
            return Invoke(request, keyData, m => MultiMapValueCountCodec.DecodeResponse(m).Response);
        }

        public virtual Guid AddEntryListener(IEntryListener<TKey, TValue> listener, bool includeValue)
        {
            var listenerAdapter =
                EntryListenerAdapter<TKey, TValue>.CreateAdapter(listener, Client.SerializationService);
            var request = MultiMapAddEntryListenerCodec.EncodeRequest(Name, includeValue, IsSmart());

            DistributedEventHandler handler =
                eventData => MultiMapAddEntryListenerCodec.EventHandler.HandleEvent(eventData,
                    (key, value, oldValue, mergingValue, type, uuid, entries) =>
                    {
                        OnEntryEvent(key, value, oldValue, mergingValue, type, uuid, entries, listenerAdapter);
                    });

            return RegisterListener(request, message => MultiMapAddEntryListenerCodec.DecodeResponse(message).Response,
                id => MultiMapRemoveEntryListenerCodec.EncodeRequest(Name, id), handler);
        }

        public virtual bool RemoveEntryListener(Guid registrationId)
        {
            return DeregisterListener(registrationId);
        }

        public virtual Guid AddEntryListener(IEntryListener<TKey, TValue> listener, TKey key, bool includeValue)
        {
            var listenerAdapter =
                EntryListenerAdapter<TKey, TValue>.CreateAdapter(listener, Client.SerializationService);
            var keyData = ToData(key);
            var request = MultiMapAddEntryListenerToKeyCodec.EncodeRequest(Name, keyData, includeValue, IsSmart());

            DistributedEventHandler handler =
                eventData => MultiMapAddEntryListenerToKeyCodec.EventHandler.HandleEvent(eventData,
                    (k, value, oldValue, mergingValue, type, uuid, entries) =>
                    {
                        OnEntryEvent(k, value, oldValue, mergingValue, type, uuid, entries, listenerAdapter);
                    });

            return RegisterListener(request, message => MultiMapAddEntryListenerToKeyCodec.DecodeResponse(message).Response,
                id => MultiMapRemoveEntryListenerCodec.EncodeRequest(Name, id), handler);
        }

        public virtual void Lock(TKey key)
        {
            ValidationUtil.ThrowExceptionIfNull(key);

            Lock(key, long.MaxValue, TimeUnit.Milliseconds);
        }

        public virtual void Lock(TKey key, long leaseTime, TimeUnit timeUnit)
        {
            ValidationUtil.ThrowExceptionIfNull(key);

            var keyData = ToData(key);
            var request = MultiMapLockCodec.EncodeRequest(Name, keyData, ThreadUtil.GetThreadId(),
                timeUnit.ToMillis(leaseTime), _lockReferenceIdGenerator.GetNextReferenceId());
            Invoke(request, keyData);
        }

        public virtual bool IsLocked(TKey key)
        {
            ValidationUtil.ThrowExceptionIfNull(key);

            var keyData = ToData(key);
            var request = MultiMapIsLockedCodec.EncodeRequest(Name, keyData);
            return Invoke(request, keyData, m => MultiMapIsLockedCodec.DecodeResponse(m).Response);
        }

        public virtual bool TryLock(TKey key)
        {
            try
            {
                return TryLock(key, 0, TimeUnit.Seconds);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <exception cref="System.Exception"></exception>
        public virtual bool TryLock(TKey key, long time, TimeUnit timeunit)
        {
            return TryLock(key, time, timeunit, long.MaxValue, TimeUnit.Milliseconds);
        }

        /// <exception cref="System.Exception"></exception>
        public virtual bool TryLock(TKey key, long timeout, TimeUnit timeunit, long leaseTime, TimeUnit leaseUnit)
        {
            ValidationUtil.ThrowExceptionIfNull(key);

            var keyData = ToData(key);
            var request = MultiMapTryLockCodec.EncodeRequest(Name, keyData, ThreadUtil.GetThreadId(),
                leaseUnit.ToMillis(leaseTime),
                timeunit.ToMillis(timeout), _lockReferenceIdGenerator.GetNextReferenceId());
            return Invoke(request, keyData, m => MultiMapTryLockCodec.DecodeResponse(m).Response);
        }

        public virtual void Unlock(TKey key)
        {
            ValidationUtil.ThrowExceptionIfNull(key);

            var keyData = ToData(key);
            var request = MultiMapUnlockCodec.EncodeRequest(Name, keyData, ThreadUtil.GetThreadId(), _lockReferenceIdGenerator.GetNextReferenceId());
            Invoke(request, keyData);
        }

        public virtual void ForceUnlock(TKey key)
        {
            ValidationUtil.ThrowExceptionIfNull(key);

            var keyData = ToData(key);
            var request = MultiMapForceUnlockCodec.EncodeRequest(Name, keyData, _lockReferenceIdGenerator.GetNextReferenceId());
            Invoke(request, keyData);
        }

        public void OnEntryEvent(IData keyData, IData valueData, IData oldValueData, IData mergingValue,
            int eventTypeInt, Guid uuid, int numberOfAffectedEntries,
            EntryListenerAdapter<TKey, TValue> listenerAdapter)
        {
            var member = Client.ClusterService.GetMember(uuid);
            listenerAdapter.OnEntryEvent(Name, keyData, valueData, oldValueData, mergingValue,
                (EntryEventType) eventTypeInt, member,
                numberOfAffectedEntries);
        }

        protected internal override void OnInitialize()
        {
            base.OnInitialize();
            _lockReferenceIdGenerator = Client.LockReferenceIdGenerator;
        }

    }
}