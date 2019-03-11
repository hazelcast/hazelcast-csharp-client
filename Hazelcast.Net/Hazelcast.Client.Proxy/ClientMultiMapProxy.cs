// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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

        public ClientMultiMapProxy(string serviceName, string name) : base(serviceName, name)
        {
        }

        public virtual bool Put(TKey key, TValue value)
        {
            var keyData = ToData(key);
            var valueData = ToData(value);
            var request = MultiMapPutCodec.EncodeRequest(GetName(), keyData, valueData, ThreadUtil.GetThreadId());

            return Invoke(request, keyData, m => MultiMapPutCodec.DecodeResponse(m).response);
        }

        public virtual ICollection<TValue> Get(TKey key)
        {
            var keyData = ToData(key);
            var request = MultiMapGetCodec.EncodeRequest(GetName(), keyData, ThreadUtil.GetThreadId());
            var list = Invoke(request, keyData, m => MultiMapGetCodec.DecodeResponse(m).response);
            return new ReadOnlyLazyList<TValue>(list, GetContext().GetSerializationService());
        }

        public virtual bool Remove(object key, object value)
        {
            var keyData = ToData(key);
            var valueData = ToData(value);

            var request = MultiMapRemoveEntryCodec.EncodeRequest(GetName(), keyData, valueData, ThreadUtil.GetThreadId());
            return Invoke(request, keyData, m => MultiMapRemoveEntryCodec.DecodeResponse(m).response);
        }

        public virtual ICollection<TValue> Remove(object key)
        {
            var keyData = ToData(key);

            var request = MultiMapRemoveCodec.EncodeRequest(GetName(), keyData, ThreadUtil.GetThreadId());
            var list = Invoke(request, keyData, m => MultiMapRemoveCodec.DecodeResponse(m).response);
            return new ReadOnlyLazyList<TValue>(list, GetContext().GetSerializationService());
        }

        public virtual ISet<TKey> KeySet()
        {
            var request = MultiMapKeySetCodec.EncodeRequest(GetName());
            var result = Invoke(request, m => MultiMapKeySetCodec.DecodeResponse(m).response);
            return new ReadOnlyLazySet<TKey>(result, GetContext().GetSerializationService());
        }

        public virtual ICollection<TValue> Values()
        {
            var request = MultiMapValuesCodec.EncodeRequest(GetName());
            var list = Invoke(request, m => MultiMapValuesCodec.DecodeResponse(m).response);
            return new ReadOnlyLazyList<TValue>(list, GetContext().GetSerializationService());
        }

        public virtual ISet<KeyValuePair<TKey, TValue>> EntrySet()
        {
            var request = MultiMapEntrySetCodec.EncodeRequest(GetName());
            var dataEntrySet = Invoke(request, m => MultiMapEntrySetCodec.DecodeResponse(m).response);
            return ToEntrySet<TKey, TValue>(dataEntrySet);
        }

        public virtual bool ContainsKey(TKey key)
        {
            var keyData = ToData(key);
            var request = MultiMapContainsKeyCodec.EncodeRequest(GetName(), keyData, ThreadUtil.GetThreadId());
            return Invoke(request, keyData, m => MultiMapContainsKeyCodec.DecodeResponse(m).response);
        }

        public virtual bool ContainsValue(object value)
        {
            var valueData = ToData(value);
            var request = MultiMapContainsValueCodec.EncodeRequest(GetName(), valueData);
            return Invoke(request, m => MultiMapContainsValueCodec.DecodeResponse(m).response);
        }

        public virtual bool ContainsEntry(TKey key, TValue value)
        {
            var keyData = ToData(key);
            var valueData = ToData(value);
            var request = MultiMapContainsEntryCodec.EncodeRequest(GetName(), keyData, valueData,
                ThreadUtil.GetThreadId());
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

        public virtual int ValueCount(TKey key)
        {
            var keyData = ToData(key);
            var request = MultiMapValueCountCodec.EncodeRequest(GetName(), keyData, ThreadUtil.GetThreadId());
            return Invoke(request, keyData, m => MultiMapValueCountCodec.DecodeResponse(m).response);
        }

        public virtual string AddEntryListener(IEntryListener<TKey, TValue> listener, bool includeValue)
        {
            var listenerAdapter =
                EntryListenerAdapter<TKey, TValue>.CreateAdapter(listener, GetContext().GetSerializationService());
            var request = MultiMapAddEntryListenerCodec.EncodeRequest(GetName(), includeValue, IsSmart());

            DistributedEventHandler handler =
                eventData => MultiMapAddEntryListenerCodec.EventHandler.HandleEvent(eventData,
                    (key, value, oldValue, mergingValue, type, uuid, entries) =>
                    {
                        OnEntryEvent(key, value, oldValue, mergingValue, type, uuid, entries, listenerAdapter);
                    });

            return RegisterListener(request, message => MultiMapAddEntryListenerCodec.DecodeResponse(message).response,
                id => MultiMapRemoveEntryListenerCodec.EncodeRequest(GetName(), id), handler);
        }

        public virtual bool RemoveEntryListener(string registrationId)
        {
            return DeregisterListener(registrationId);
        }

        public virtual string AddEntryListener(IEntryListener<TKey, TValue> listener, TKey key, bool includeValue)
        {
            var listenerAdapter =
                EntryListenerAdapter<TKey, TValue>.CreateAdapter(listener, GetContext().GetSerializationService());
            var keyData = ToData(key);
            var request = MultiMapAddEntryListenerToKeyCodec.EncodeRequest(GetName(), keyData, includeValue, IsSmart());

            DistributedEventHandler handler =
                eventData => MultiMapAddEntryListenerToKeyCodec.EventHandler.HandleEvent(eventData,
                    (k, value, oldValue, mergingValue, type, uuid, entries) =>
                    {
                        OnEntryEvent(k, value, oldValue, mergingValue, type, uuid, entries, listenerAdapter);
                    });

            return RegisterListener(request, message => MultiMapAddEntryListenerToKeyCodec.DecodeResponse(message).response,
                id => MultiMapRemoveEntryListenerCodec.EncodeRequest(GetName(), id), handler);
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
            var request = MultiMapLockCodec.EncodeRequest(GetName(), keyData, ThreadUtil.GetThreadId(),
                timeUnit.ToMillis(leaseTime), _lockReferenceIdGenerator.GetNextReferenceId());
            Invoke(request, keyData);
        }

        public virtual bool IsLocked(TKey key)
        {
            ValidationUtil.ThrowExceptionIfNull(key);

            var keyData = ToData(key);
            var request = MultiMapIsLockedCodec.EncodeRequest(GetName(), keyData);
            return Invoke(request, keyData, m => MultiMapIsLockedCodec.DecodeResponse(m).response);
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
            var request = MultiMapTryLockCodec.EncodeRequest(GetName(), keyData, ThreadUtil.GetThreadId(),
                leaseUnit.ToMillis(leaseTime),
                timeunit.ToMillis(timeout), _lockReferenceIdGenerator.GetNextReferenceId());
            return Invoke(request, keyData, m => MultiMapTryLockCodec.DecodeResponse(m).response);
        }

        public virtual void Unlock(TKey key)
        {
            ValidationUtil.ThrowExceptionIfNull(key);

            var keyData = ToData(key);
            var request = MultiMapUnlockCodec.EncodeRequest(GetName(), keyData, ThreadUtil.GetThreadId(), _lockReferenceIdGenerator.GetNextReferenceId());
            Invoke(request, keyData);
        }

        public virtual void ForceUnlock(TKey key)
        {
            ValidationUtil.ThrowExceptionIfNull(key);

            var keyData = ToData(key);
            var request = MultiMapForceUnlockCodec.EncodeRequest(GetName(), keyData, _lockReferenceIdGenerator.GetNextReferenceId());
            Invoke(request, keyData);
        }

        public void OnEntryEvent(IData keyData, IData valueData, IData oldValueData, IData mergingValue,
            int eventTypeInt, string uuid, int numberOfAffectedEntries,
            EntryListenerAdapter<TKey, TValue> listenerAdapter)
        {
            var member = GetContext().GetClusterService().GetMember(uuid);
            listenerAdapter.OnEntryEvent(GetName(), keyData, valueData, oldValueData, mergingValue,
                (EntryEventType) eventTypeInt, member,
                numberOfAffectedEntries);
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            _lockReferenceIdGenerator = GetContext().GetClient().GetLockReferenceIdGenerator();
        }

    }
}