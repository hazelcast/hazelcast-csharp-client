﻿// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Data;
using Hazelcast.Messaging;
using Hazelcast.Predicates;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;
using Hazelcast.Serialization.Collections;
using Microsoft.Extensions.Logging;

namespace Hazelcast.DistributedObjects.Impl
{
    internal class HReplicatedDictionary<TKey, TValue> : DistributedObjectBase, IHReplicatedDictionary<TKey, TValue>
    {
        private readonly int _partitionId;

        public HReplicatedDictionary(string name, DistributedObjectFactory factory, Cluster cluster, ISerializationService serializationService, int partitionId, ILoggerFactory loggerFactory)
            : base(HReplicatedDictionary.ServiceName, name, factory, cluster, serializationService, loggerFactory)
        {
            _partitionId = partitionId;
        }

        public Task<TValue> SetAsync(TKey key, TValue value)
            => SetAsync(key, value, TimeToLive.InfiniteTimeSpan);

        public async Task<TValue> SetAsync(TKey key, TValue value, TimeSpan timeToLive)
        {
            var (keyData, valueData) = ToSafeData(key, value);
            var ttl = timeToLive.CodecMilliseconds(0); // codec wants 0 for infinite
            var requestMessage = ReplicatedMapPutCodec.EncodeRequest(Name, keyData, valueData, ttl);
            var responseMessage = await Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, keyData).CAF();
            var response = ReplicatedMapPutCodec.DecodeResponse(responseMessage).Response;
            return ToObject<TValue>(response);
        }

        public async Task SetAsync(IDictionary<TKey, TValue> entries)
        {
            var entriesData = new List<KeyValuePair<IData, IData>>(entries.Count);
            foreach (var (key, value) in entries)
            {
                var (keyData, valueData) = ToSafeData(key, value);
                entriesData.Add(new KeyValuePair<IData, IData>(keyData, valueData));
            }

            var requestMessage = ReplicatedMapPutAllCodec.EncodeRequest(Name, entriesData);
            var responseMessage = await Cluster.Messaging.SendAsync(requestMessage).CAF();
            _ = ReplicatedMapPutAllCodec.DecodeResponse(responseMessage);
        }

        public async Task<TValue> RemoveAsync(TKey key)
        {
            var keyData = ToSafeData(key);
            var requestMessage = ReplicatedMapRemoveCodec.EncodeRequest(Name, keyData);
            var responseMessage = await Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, keyData).CAF();
            var response = ReplicatedMapRemoveCodec.DecodeResponse(responseMessage).Response;
            return ToObject<TValue>(response);
        }

        public async Task ClearAsync()
        {
            var requestMessage = ReplicatedMapClearCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.Messaging.SendAsync(requestMessage).CAF();
            _ = ReplicatedMapClearCodec.DecodeResponse(responseMessage);
        }

        public async Task<TValue> GetAsync(TKey key)
        {
            var keyData = ToSafeData(key);
            var requestMessage = ReplicatedMapGetCodec.EncodeRequest(Name, keyData);
            var responseMessage = await Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, keyData).CAF();
            var response = ReplicatedMapGetCodec.DecodeResponse(responseMessage).Response;
            return ToObject<TValue>(response);
        }

        public async Task<IReadOnlyList<TKey>> GetKeysAsync()
        {
            var requestMessage = ReplicatedMapKeySetCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.Messaging.SendToPartitionOwnerAsync(requestMessage, _partitionId).CAF();
            var response = ReplicatedMapKeySetCodec.DecodeResponse(responseMessage).Response;
            return new ReadOnlyLazyList<TKey>(response, SerializationService);
        }

        public async Task<IReadOnlyList<TValue>> GetValuesAsync()
        {
            var requestMessage = ReplicatedMapValuesCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.Messaging.SendToPartitionOwnerAsync(requestMessage, _partitionId).CAF();
            var response = ReplicatedMapKeySetCodec.DecodeResponse(responseMessage).Response;
            return new ReadOnlyLazyList<TValue>(response, SerializationService);
        }

        public async Task<IReadOnlyDictionary<TKey, TValue>> GetAsync()
        {
            var requestMessage = ReplicatedMapEntrySetCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.Messaging.SendToPartitionOwnerAsync(requestMessage, _partitionId).CAF();
            var response = ReplicatedMapEntrySetCodec.DecodeResponse(responseMessage).Response;
            return new ReadOnlyLazyDictionary<TKey, TValue>(SerializationService) { response };
        }

        public async Task<int> CountAsync()
        {
            var requestMessage = ReplicatedMapSizeCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.Messaging.SendToPartitionOwnerAsync(requestMessage, _partitionId).CAF();
            return ReplicatedMapSizeCodec.DecodeResponse(responseMessage).Response;
        }

        public async Task<bool> IsEmptyAsync()
        {
            var requestMessage = ReplicatedMapIsEmptyCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.Messaging.SendToPartitionOwnerAsync(requestMessage, _partitionId).CAF();
            return ReplicatedMapIsEmptyCodec.DecodeResponse(responseMessage).Response;
        }

        public async Task<bool> ContainsKeyAsync(TKey key)
        {
            var keyData = ToSafeData(key);
            var requestMessage = ReplicatedMapContainsKeyCodec.EncodeRequest(Name, keyData);
            var responseMessage = await Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, keyData).CAF();
            return ReplicatedMapContainsKeyCodec.DecodeResponse(responseMessage).Response;
        }

        public async Task<bool> ContainsAsync(TValue value)
        {
            var valueData = ToSafeData(value);
            var requestMessage = ReplicatedMapContainsValueCodec.EncodeRequest(Name, valueData);
            var responseMessage = await Cluster.Messaging.SendToPartitionOwnerAsync(requestMessage, _partitionId).CAF();
            return ReplicatedMapContainsValueCodec.DecodeResponse(responseMessage).Response;
        }

        private async Task<Guid> SubscribeAsync(IPredicate predicate, bool hasPredicate, TKey key, bool hasKey, Action<ReplicatedDictionaryEventHandlers<TKey, TValue>> handle)
        {
            if (hasKey && key == null) throw new ArgumentNullException(nameof(key));
            if (hasPredicate && predicate == null) throw new ArgumentNullException(nameof(predicate));
            if (handle == null) throw new ArgumentNullException(nameof(handle));

            var handlers = new ReplicatedDictionaryEventHandlers<TKey, TValue>();
            handle(handlers);

            // 0: no entryKey, no predicate
            // 1: entryKey, no predicate
            // 2: no entryKey, predicate
            // 3: entryKey, predicate
            var mode = (hasKey ? 1 : 0) + (hasPredicate ? 2 : 0);

            var subscribeRequest = mode switch
            {
                0 => ReplicatedMapAddEntryListenerCodec.EncodeRequest(Name, Cluster.IsSmartRouting),
                1 => ReplicatedMapAddEntryListenerToKeyCodec.EncodeRequest(Name, ToData(key), Cluster.IsSmartRouting),
                2 => ReplicatedMapAddEntryListenerWithPredicateCodec.EncodeRequest(Name, ToData(predicate), Cluster.IsSmartRouting),
                3 => ReplicatedMapAddEntryListenerToKeyWithPredicateCodec.EncodeRequest(Name, ToData(key), ToData(predicate), Cluster.IsSmartRouting),
                _ => throw new NotSupportedException()
            };

            var subscription = new ClusterSubscription(
                subscribeRequest,
                ReadSubscribeResponse,
                CreateUnsubscribeRequest,
                ReadUnsubscribeResponse,
                HandleEventAsync,
                new MapSubscriptionState(mode, Name, handlers));

            await Cluster.Events.InstallSubscriptionAsync(subscription).CAF();

            return subscription.Id;
        }

        public Task<Guid> SubscribeAsync(Action<ReplicatedDictionaryEventHandlers<TKey, TValue>> handle)
            => SubscribeAsync(default, false, default, false, handle);

        public Task<Guid> SubscribeAsync(TKey key, Action<ReplicatedDictionaryEventHandlers<TKey, TValue>> handle)
            => SubscribeAsync(default, false, key, true, handle);

        public Task<Guid> SubscribeAsync(IPredicate predicate, Action<ReplicatedDictionaryEventHandlers<TKey, TValue>> handle)
            => SubscribeAsync(predicate, true, default, false, handle);

        public Task<Guid> SubscribeAsync(TKey key, IPredicate predicate, Action<ReplicatedDictionaryEventHandlers<TKey, TValue>> handle)
            => SubscribeAsync(predicate, true, key, true, handle);

        private class MapSubscriptionState : SubscriptionState<ReplicatedDictionaryEventHandlers<TKey, TValue>>
        {
            public MapSubscriptionState(int mode, string name, ReplicatedDictionaryEventHandlers<TKey, TValue> handlers)
                : base(name, handlers)
            {
                Mode = mode;
            }

            public int Mode { get; }
        }

        private ValueTask HandleEventAsync(ClientMessage eventMessage, object state)
        {
            var sstate = ToSafeState<MapSubscriptionState>(state);

            async ValueTask HandleEntryEventAsync(IData keyData, IData valueData, IData oldValueData, IData mergingValueData, int eventTypeData, Guid memberId, int numberOfAffectedEntries)
            {
                var eventType = (HDictionaryEventTypes)eventTypeData;
                if (eventType == HDictionaryEventTypes.Nothing) return;

                var member = Cluster.Members.GetMember(memberId);
                var key = LazyArg<TKey>(keyData);
                var value = LazyArg<TValue>(valueData);
                var oldValue = LazyArg<TValue>(oldValueData);
                var mergingValue = LazyArg<TValue>(mergingValueData);

                // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                foreach (var handler in sstate.Handlers)
                {
                    if (handler.EventType.HasAll(eventType))
                    {
                        var task = handler switch
                        {
                            IDictionaryEntryEventHandler<TKey, TValue, HReplicatedDictionary<TKey, TValue>> entryHandler => entryHandler.HandleAsync(this, member, key, value, oldValue, mergingValue, eventType, numberOfAffectedEntries),
                            IDictionaryEventHandler<TKey, TValue, HReplicatedDictionary<TKey, TValue>> mapHandler => mapHandler.HandleAsync(this, member, numberOfAffectedEntries),
                            _ => throw new NotSupportedException()
                        };
                        await task.CAF();
                    }
                }
            }

            return sstate.Mode switch
            {
                0 => ReplicatedMapAddEntryListenerCodec.HandleEventAsync(eventMessage, HandleEntryEventAsync, LoggerFactory),
                1 => ReplicatedMapAddEntryListenerToKeyCodec.HandleEventAsync(eventMessage, HandleEntryEventAsync, LoggerFactory),
                2 => ReplicatedMapAddEntryListenerWithPredicateCodec.HandleEventAsync(eventMessage, HandleEntryEventAsync, LoggerFactory),
                3 => ReplicatedMapAddEntryListenerToKeyWithPredicateCodec.HandleEventAsync(eventMessage, HandleEntryEventAsync, LoggerFactory),
                _ => throw new NotSupportedException()
            };
        }

        private static ClientMessage CreateUnsubscribeRequest(Guid subscriptionId, object state)
        {
            var sstate = ToSafeState<MapSubscriptionState>(state);
            return ReplicatedMapRemoveEntryListenerCodec.EncodeRequest(sstate.Name, subscriptionId);
        }

        private static Guid ReadSubscribeResponse(ClientMessage responseMessage, object state)
        {
            var sstate = ToSafeState<MapSubscriptionState>(state);

            return sstate.Mode switch
            {
                0 => ReplicatedMapAddEntryListenerCodec.DecodeResponse(responseMessage).Response,
                1 => ReplicatedMapAddEntryListenerToKeyCodec.DecodeResponse(responseMessage).Response,
                2 => ReplicatedMapAddEntryListenerWithPredicateCodec.DecodeResponse(responseMessage).Response,
                3 => ReplicatedMapAddEntryListenerToKeyWithPredicateCodec.DecodeResponse(responseMessage).Response,
                _ => throw new NotSupportedException()
            };
        }

        private static bool ReadUnsubscribeResponse(ClientMessage unsubscribeResponseMessage, object state)
        {
            return ReplicatedMapRemoveEntryListenerCodec.DecodeResponse(unsubscribeResponseMessage).Response;
        }

        public ValueTask<bool> UnsubscribeAsync(Guid subscriptionId)
            => UnsubscribeBaseAsync(subscriptionId);
    }
}
