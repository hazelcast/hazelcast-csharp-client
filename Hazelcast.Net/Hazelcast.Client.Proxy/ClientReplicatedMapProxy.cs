// Copyright (c) 2008-2017, Hazelcast, Inc. All Rights Reserved.
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
    internal sealed class ClientReplicatedMapProxy<TKey, TValue> : ClientProxy, IReplicatedMap<TKey, TValue>
    {
        private static Random _random = new Random();
        private static object _randomLock = new object();
        private int _targetPartitionId = -1;
        
        public ClientReplicatedMapProxy(string serviceName, string name) : base(serviceName, name)
        {
        }

        public string AddEntryListener(IEntryListener<TKey, TValue> listener)
        {
            var listenerAdapter =
                EntryListenerAdapter<TKey, TValue>.CreateAdapter(listener, GetContext().GetSerializationService());
            var request = ReplicatedMapAddEntryListenerCodec.EncodeRequest(GetName(), IsSmart());
            DistributedEventHandler handler =
                eventData => ReplicatedMapAddEntryListenerCodec.AbstractEventHandler.Handle(eventData,
                    (key, value, oldValue, mergingValue, type, uuid, entries) =>
                    {
                        OnEntryEvent(key, value, oldValue, mergingValue, type, uuid, entries, listenerAdapter);
                    });
            return RegisterListener(request,
                message => ReplicatedMapAddEntryListenerCodec.DecodeResponse(message).response,
                id => ReplicatedMapRemoveEntryListenerCodec.EncodeRequest(GetName(), id), handler);
        }

        public string AddEntryListener(IEntryListener<TKey, TValue> listener, TKey key)
        {
            var keyData = ToData(key);
            var listenerAdapter =
                EntryListenerAdapter<TKey, TValue>.CreateAdapter(listener, GetContext().GetSerializationService());
            var request = ReplicatedMapAddEntryListenerToKeyCodec.EncodeRequest(GetName(), keyData, IsSmart());
            DistributedEventHandler handler =
                eventData => ReplicatedMapAddEntryListenerToKeyCodec.AbstractEventHandler.Handle(eventData,
                    (k, value, oldValue, mergingValue, type, uuid, entries) =>
                    {
                        OnEntryEvent(k, value, oldValue, mergingValue, type, uuid, entries, listenerAdapter);
                    });
            return RegisterListener(request,
                message => ReplicatedMapAddEntryListenerCodec.DecodeResponse(message).response,
                id => ReplicatedMapRemoveEntryListenerCodec.EncodeRequest(GetName(), id), handler);
        }

        public string AddEntryListener(IEntryListener<TKey, TValue> listener, IPredicate predicate)
        {
            var predicateData = ToData(predicate);
            var listenerAdapter =
                EntryListenerAdapter<TKey, TValue>.CreateAdapter(listener, GetContext().GetSerializationService());
            var request =
                ReplicatedMapAddEntryListenerWithPredicateCodec.EncodeRequest(GetName(), predicateData, IsSmart());
            DistributedEventHandler handler =
                eventData => ReplicatedMapAddEntryListenerWithPredicateCodec.AbstractEventHandler.Handle(eventData,
                    (k, value, oldValue, mergingValue, type, uuid, entries) =>
                    {
                        OnEntryEvent(k, value, oldValue, mergingValue, type, uuid, entries, listenerAdapter);
                    });
            return RegisterListener(request,
                message => ReplicatedMapAddEntryListenerWithPredicateCodec.DecodeResponse(message).response,
                id => ReplicatedMapRemoveEntryListenerCodec.EncodeRequest(GetName(), id), handler);
        }

        public string AddEntryListener(IEntryListener<TKey, TValue> listener, IPredicate predicate, TKey key)
        {
            var keyData = ToData(key);
            var predicateData = ToData(predicate);
            var listenerAdapter =
                EntryListenerAdapter<TKey, TValue>.CreateAdapter(listener, GetContext().GetSerializationService());
            var request =
                ReplicatedMapAddEntryListenerToKeyWithPredicateCodec.EncodeRequest(GetName(), keyData, predicateData,
                    IsSmart());
            DistributedEventHandler handler =
                eventData => ReplicatedMapAddEntryListenerToKeyWithPredicateCodec.AbstractEventHandler.Handle(eventData,
                    (k, value, oldValue, mergingValue, type, uuid, entries) =>
                    {
                        OnEntryEvent(k, value, oldValue, mergingValue, type, uuid, entries, listenerAdapter);
                    });
            return RegisterListener(request,
                message => ReplicatedMapAddEntryListenerToKeyWithPredicateCodec.DecodeResponse(message).response,
                id => ReplicatedMapRemoveEntryListenerCodec.EncodeRequest(GetName(), id), handler);
        }

        public void Clear()
        {
            var request = ReplicatedMapClearCodec.EncodeRequest(GetName());
            Invoke(request);
        }

        public bool ContainsKey(object key)
        {
            var keyData = ToData(key);
            var request = ReplicatedMapContainsKeyCodec.EncodeRequest(GetName(), keyData);
            return Invoke(request, keyData, m => ReplicatedMapContainsKeyCodec.DecodeResponse(m).response);
        }

        public bool ContainsValue(object value)
        {
            var valueData = ToData(value);
            var request = ReplicatedMapContainsValueCodec.EncodeRequest(GetName(), valueData);
            return InvokeOnPartition(request, _targetPartitionId, m => ReplicatedMapContainsValueCodec.DecodeResponse(m).response);
        }

        public TValue Get(object key)
        {
            var keyData = ToData(key);
            var request = ReplicatedMapGetCodec.EncodeRequest(GetName(), keyData);
            var result = Invoke(request, keyData);
            var value = ToObject<TValue>(ReplicatedMapGetCodec.DecodeResponse(result).response);
            return value;
        }

        public bool IsEmpty()
        {
            var request = ReplicatedMapIsEmptyCodec.EncodeRequest(GetName());
            return InvokeOnPartition(request, _targetPartitionId, m => ReplicatedMapIsEmptyCodec.DecodeResponse(m).response);
        }

        public ISet<KeyValuePair<TKey, TValue>> EntrySet()
        {
            var request = ReplicatedMapEntrySetCodec.EncodeRequest(GetName());
            var entries = InvokeOnPartition(request, _targetPartitionId, m => ReplicatedMapEntrySetCodec.DecodeResponse(m).response);
            ISet<KeyValuePair<TKey, TValue>> entrySet = new HashSet<KeyValuePair<TKey, TValue>>();
            foreach (var entry in entries)
            {
                var key = ToObject<TKey>(entry.Key);
                var value = ToObject<TValue>(entry.Value);
                entrySet.Add(new KeyValuePair<TKey, TValue>(key, value));
            }
            return entrySet;
        }

        public ISet<TKey> KeySet()
        {
            var request = ReplicatedMapKeySetCodec.EncodeRequest(GetName());
            var result = InvokeOnPartition(request, _targetPartitionId, m => ReplicatedMapKeySetCodec.DecodeResponse(m).response);
            return new ReadOnlyLazySet<TKey>(result, GetContext().GetSerializationService());
        }

        public TValue Put(TKey key, TValue value)
        {
            return Put(key, value, 0, TimeUnit.Milliseconds);
        }

        public TValue Put(TKey key, TValue value, long ttl, TimeUnit timeunit)
        {
            var keyData = ToData(key);
            var valueData = ToData(value);
            var request = ReplicatedMapPutCodec.EncodeRequest(GetName(), keyData, valueData, timeunit.ToMillis(ttl));
            var clientMessage = Invoke(request, keyData);
            var response = ReplicatedMapPutCodec.DecodeResponse(clientMessage).response;
            return ToObject<TValue>(response);
        }

        public void PutAll(IDictionary<TKey, TValue> map)
        {
            var dataEntries = new List<KeyValuePair<IData, IData>>(map.Count);
            foreach (var kvPair in map)
            {
                var keyData = ToData(kvPair.Key);
                var valueData = ToData(kvPair.Value);
                dataEntries.Add(new KeyValuePair<IData, IData>(keyData, valueData));
            }
            var request = ReplicatedMapPutAllCodec.EncodeRequest(GetName(), dataEntries);
            Invoke(request);        
        }

        public TValue Remove(object key)
        {
            var keyData = ToData(key);
            var request = ReplicatedMapRemoveCodec.EncodeRequest(GetName(), keyData);
            var clientMessage = Invoke(request, keyData);
            return ToObject<TValue>(ReplicatedMapRemoveCodec.DecodeResponse(clientMessage).response);
        }

        public bool RemoveEntryListener(string registrationId)
        {
            return DeregisterListener(registrationId, id => ReplicatedMapRemoveEntryListenerCodec.EncodeRequest(GetName(), id));
        }

        public int Size()
        {
            var request = ReplicatedMapSizeCodec.EncodeRequest(GetName());
            return InvokeOnPartition(request, _targetPartitionId, m => ReplicatedMapSizeCodec.DecodeResponse(m).response);
        }

        public ICollection<TValue> Values()
        {
            var request = ReplicatedMapValuesCodec.EncodeRequest(GetName());
            var list = InvokeOnPartition(request, _targetPartitionId, m => ReplicatedMapValuesCodec.DecodeResponse(m).response);
            return new ReadOnlyLazyList<TValue>(list, GetContext().GetSerializationService());
        }

        private void OnEntryEvent(IData keyData, IData valueData, IData oldValueData, IData mergingValue,
            int eventTypeInt, string uuid, int numberOfAffectedEntries,
            EntryListenerAdapter<TKey, TValue> listenerAdapter)
        {
            var member = GetContext().GetClusterService().GetMember(uuid);
            listenerAdapter.OnEntryEvent(GetName(), keyData, valueData, oldValueData, mergingValue,
                (EntryEventType) eventTypeInt, member,
                numberOfAffectedEntries);
        }

        internal override void PostInit()
        {
            var partitionCount = GetContext().GetPartitionService().GetPartitionCount();
            lock (_randomLock)
            {
                _targetPartitionId = _random.Next(partitionCount);
            }
        }
    }
}