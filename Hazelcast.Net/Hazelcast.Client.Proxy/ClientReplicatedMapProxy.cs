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
    internal sealed class ClientReplicatedMapProxy<TKey, TValue> : ClientProxy, IReplicatedMap<TKey, TValue>
    {
        private static Random _random = new Random();
        private static object _randomLock = new object();
        private int _targetPartitionId = -1;
        
        public ClientReplicatedMapProxy(string serviceName, string name, HazelcastClient client) : base(serviceName, name, client)
        {
        }

        public Guid AddEntryListener(IEntryListener<TKey, TValue> listener)
        {
            var listenerAdapter =
                EntryListenerAdapter<TKey, TValue>.CreateAdapter(listener, Client.SerializationService);
            var request = ReplicatedMapAddEntryListenerCodec.EncodeRequest(Name, IsSmart());
            DistributedEventHandler handler =
                eventData => ReplicatedMapAddEntryListenerCodec.EventHandler.HandleEvent(eventData,
                    (key, value, oldValue, mergingValue, type, uuid, entries) =>
                    {
                        OnEntryEvent(key, value, oldValue, mergingValue, type, uuid, entries, listenerAdapter);
                    });
            return RegisterListener(request,
                message => ReplicatedMapAddEntryListenerCodec.DecodeResponse(message).Response,
                id => ReplicatedMapRemoveEntryListenerCodec.EncodeRequest(Name, id), handler);
        }

        public Guid AddEntryListener(IEntryListener<TKey, TValue> listener, TKey key)
        {
            var keyData = ToData(key);
            var listenerAdapter =
                EntryListenerAdapter<TKey, TValue>.CreateAdapter(listener, Client.SerializationService);
            var request = ReplicatedMapAddEntryListenerToKeyCodec.EncodeRequest(Name, keyData, IsSmart());
            DistributedEventHandler handler =
                eventData => ReplicatedMapAddEntryListenerToKeyCodec.EventHandler.HandleEvent(eventData,
                    (k, value, oldValue, mergingValue, type, uuid, entries) =>
                    {
                        OnEntryEvent(k, value, oldValue, mergingValue, type, uuid, entries, listenerAdapter);
                    });
            return RegisterListener(request,
                message => ReplicatedMapAddEntryListenerCodec.DecodeResponse(message).Response,
                id => ReplicatedMapRemoveEntryListenerCodec.EncodeRequest(Name, id), handler);
        }

        public Guid AddEntryListener(IEntryListener<TKey, TValue> listener, IPredicate predicate)
        {
            var predicateData = ToData(predicate);
            var listenerAdapter =
                EntryListenerAdapter<TKey, TValue>.CreateAdapter(listener, Client.SerializationService);
            var request =
                ReplicatedMapAddEntryListenerWithPredicateCodec.EncodeRequest(Name, predicateData, IsSmart());
            DistributedEventHandler handler =
                eventData => ReplicatedMapAddEntryListenerWithPredicateCodec.EventHandler.HandleEvent(eventData,
                    (k, value, oldValue, mergingValue, type, uuid, entries) =>
                    {
                        OnEntryEvent(k, value, oldValue, mergingValue, type, uuid, entries, listenerAdapter);
                    });
            return RegisterListener(request,
                message => ReplicatedMapAddEntryListenerWithPredicateCodec.DecodeResponse(message).Response,
                id => ReplicatedMapRemoveEntryListenerCodec.EncodeRequest(Name, id), handler);
        }

        public Guid AddEntryListener(IEntryListener<TKey, TValue> listener, IPredicate predicate, TKey key)
        {
            var keyData = ToData(key);
            var predicateData = ToData(predicate);
            var listenerAdapter =
                EntryListenerAdapter<TKey, TValue>.CreateAdapter(listener, Client.SerializationService);
            var request =
                ReplicatedMapAddEntryListenerToKeyWithPredicateCodec.EncodeRequest(Name, keyData, predicateData,
                    IsSmart());
            DistributedEventHandler handler =
                eventData => ReplicatedMapAddEntryListenerToKeyWithPredicateCodec.EventHandler.HandleEvent(eventData,
                    (k, value, oldValue, mergingValue, type, uuid, entries) =>
                    {
                        OnEntryEvent(k, value, oldValue, mergingValue, type, uuid, entries, listenerAdapter);
                    });
            return RegisterListener(request,
                message => ReplicatedMapAddEntryListenerToKeyWithPredicateCodec.DecodeResponse(message).Response,
                id => ReplicatedMapRemoveEntryListenerCodec.EncodeRequest(Name, id), handler);
        }

        public void Clear()
        {
            var request = ReplicatedMapClearCodec.EncodeRequest(Name);
            Invoke(request);
        }

        public bool ContainsKey(object key)
        {
            var keyData = ToData(key);
            var request = ReplicatedMapContainsKeyCodec.EncodeRequest(Name, keyData);
            return Invoke(request, keyData, m => ReplicatedMapContainsKeyCodec.DecodeResponse(m).Response);
        }

        public bool ContainsValue(object value)
        {
            var valueData = ToData(value);
            var request = ReplicatedMapContainsValueCodec.EncodeRequest(Name, valueData);
            return InvokeOnPartition(request, _targetPartitionId, m => ReplicatedMapContainsValueCodec.DecodeResponse(m).Response);
        }

        public TValue Get(object key)
        {
            var keyData = ToData(key);
            var request = ReplicatedMapGetCodec.EncodeRequest(Name, keyData);
            var result = Invoke(request, keyData);
            var value = ToObject<TValue>(ReplicatedMapGetCodec.DecodeResponse(result).Response);
            return value;
        }

        public bool IsEmpty()
        {
            var request = ReplicatedMapIsEmptyCodec.EncodeRequest(Name);
            return InvokeOnPartition(request, _targetPartitionId, m => ReplicatedMapIsEmptyCodec.DecodeResponse(m).Response);
        }

        public ISet<KeyValuePair<TKey, TValue>> EntrySet()
        {
            var request = ReplicatedMapEntrySetCodec.EncodeRequest(Name);
            var entries = InvokeOnPartition(request, _targetPartitionId, m => ReplicatedMapEntrySetCodec.DecodeResponse(m).Response);
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
            var request = ReplicatedMapKeySetCodec.EncodeRequest(Name);
            var result = InvokeOnPartition(request, _targetPartitionId, m => ReplicatedMapKeySetCodec.DecodeResponse(m).Response);
            return new ReadOnlyLazySet<TKey>(result, Client.SerializationService);
        }

        public TValue Put(TKey key, TValue value)
        {
            return Put(key, value, 0, TimeUnit.Milliseconds);
        }

        public TValue Put(TKey key, TValue value, long ttl, TimeUnit timeunit)
        {
            var keyData = ToData(key);
            var valueData = ToData(value);
            var request = ReplicatedMapPutCodec.EncodeRequest(Name, keyData, valueData, timeunit.ToMillis(ttl));
            var clientMessage = Invoke(request, keyData);
            var response = ReplicatedMapPutCodec.DecodeResponse(clientMessage).Response;
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
            var request = ReplicatedMapPutAllCodec.EncodeRequest(Name, dataEntries);
            Invoke(request);        
        }

        public TValue Remove(object key)
        {
            var keyData = ToData(key);
            var request = ReplicatedMapRemoveCodec.EncodeRequest(Name, keyData);
            var clientMessage = Invoke(request, keyData);
            return ToObject<TValue>(ReplicatedMapRemoveCodec.DecodeResponse(clientMessage).Response);
        }

        public bool RemoveEntryListener(Guid registrationId)
        {
            return DeregisterListener(registrationId);
        }

        public int Size()
        {
            var request = ReplicatedMapSizeCodec.EncodeRequest(Name);
            return InvokeOnPartition(request, _targetPartitionId, m => ReplicatedMapSizeCodec.DecodeResponse(m).Response);
        }

        public ICollection<TValue> Values()
        {
            var request = ReplicatedMapValuesCodec.EncodeRequest(Name);
            var list = InvokeOnPartition(request, _targetPartitionId, m => ReplicatedMapValuesCodec.DecodeResponse(m).Response);
            return new ReadOnlyLazyList<TValue, IData>(list, Client.SerializationService);
        }

        private void OnEntryEvent(IData keyData, IData valueData, IData oldValueData, IData mergingValue,
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
            var partitionCount = Client.PartitionService.GetPartitionCount();
            lock (_randomLock)
            {
                _targetPartitionId = _random.Next(partitionCount);
            }
        }
    }
}