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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Messaging;
using Hazelcast.Models;
using Hazelcast.Query;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;
using Hazelcast.Serialization.Collections;
using Microsoft.Extensions.Logging;

namespace Hazelcast.DistributedObjects.Impl
{
    internal class HReplicatedMap<TKey, TValue> : DistributedObjectBase, IHReplicatedMap<TKey, TValue>
    {
        private readonly int _partitionId;

        public HReplicatedMap(string name, DistributedObjectFactory factory, Cluster cluster, SerializationService serializationService, int partitionId, ILoggerFactory loggerFactory)
            : base(ServiceNames.ReplicatedMap, name, factory, cluster, serializationService, loggerFactory)
        {
            _partitionId = partitionId;
        }

        public Task<TValue> PutAsync(TKey key, TValue value)
            => PutAsync(key, value, TimeSpan.Zero);

        public async Task<TValue> PutAsync(TKey key, TValue value, TimeSpan timeToLive)
        {
            var (keyData, valueData) = ToSafeData(key, value);

            // server wants 0 for infinite, no negative value
            var timeToLiveMs = timeToLive.RoundedMilliseconds(false); // codec: 0 = infinite, -1 = not supported?

            var requestMessage = ReplicatedMapPutCodec.EncodeRequest(Name, keyData, valueData, timeToLiveMs);
            var responseMessage = await Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, keyData).CAF();
            var response = ReplicatedMapPutCodec.DecodeResponse(responseMessage).Response;
            return ToObject<TValue>(response);
        }

        public async Task SetAllAsync(IDictionary<TKey, TValue> entries)
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

        public async Task<IReadOnlyCollection<TKey>> GetKeysAsync()
        {
            var requestMessage = ReplicatedMapKeySetCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.Messaging.SendToPartitionOwnerAsync(requestMessage, _partitionId).CAF();
            var response = ReplicatedMapKeySetCodec.DecodeResponse(responseMessage).Response;
            return new ReadOnlyLazyList<TKey>(response, SerializationService);
        }

        public async Task<IReadOnlyCollection<TValue>> GetValuesAsync()
        {
            var requestMessage = ReplicatedMapValuesCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.Messaging.SendToPartitionOwnerAsync(requestMessage, _partitionId).CAF();
            var response = ReplicatedMapKeySetCodec.DecodeResponse(responseMessage).Response;
            return new ReadOnlyLazyList<TValue>(response, SerializationService);
        }

        public Task<IReadOnlyDictionary<TKey, TValue>> GetEntriesAsync() => GetEntriesAsync(CancellationToken.None);

        private async Task<IReadOnlyDictionary<TKey, TValue>> GetEntriesAsync(CancellationToken cancellationToken)
        {
            var requestMessage = ReplicatedMapEntrySetCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.Messaging.SendToPartitionOwnerAsync(requestMessage, _partitionId, cancellationToken).CAF();
            var response = ReplicatedMapEntrySetCodec.DecodeResponse(responseMessage).Response;
            return new ReadOnlyLazyDictionary<TKey, TValue>(SerializationService) { response };
        }

        public async Task<int> GetSizeAsync()
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

        public async Task<bool> ContainsValueAsync(TValue value)
        {
            var valueData = ToSafeData(value);
            var requestMessage = ReplicatedMapContainsValueCodec.EncodeRequest(Name, valueData);
            var responseMessage = await Cluster.Messaging.SendToPartitionOwnerAsync(requestMessage, _partitionId).CAF();
            return ReplicatedMapContainsValueCodec.DecodeResponse(responseMessage).Response;
        }

        private async Task<Guid> SubscribeAsync(Action<ReplicatedMapEventHandlers<TKey, TValue>> events, Maybe<TKey> key, IPredicate predicate, object state)
        {
            if (events == null) throw new ArgumentNullException(nameof(events));

            var handlers = new ReplicatedMapEventHandlers<TKey, TValue>();
            events(handlers);

            // 0: no entryKey, no predicate
            // 1: entryKey, no predicate
            // 2: no entryKey, predicate
            // 3: entryKey, predicate
            var mode = key.Match(1, 0) + (predicate != null ? 2 : 0);
            var keyv = key.ValueOrDefault();

            var subscribeRequest = mode switch
            {
                0 => ReplicatedMapAddEntryListenerCodec.EncodeRequest(Name, Cluster.IsSmartRouting),
                1 => ReplicatedMapAddEntryListenerToKeyCodec.EncodeRequest(Name, ToData(keyv), Cluster.IsSmartRouting),
                2 => ReplicatedMapAddEntryListenerWithPredicateCodec.EncodeRequest(Name, ToData(predicate), Cluster.IsSmartRouting),
                3 => ReplicatedMapAddEntryListenerToKeyWithPredicateCodec.EncodeRequest(Name, ToData(keyv), ToData(predicate), Cluster.IsSmartRouting),
                _ => throw new NotSupportedException()
            };

            var subscription = new ClusterSubscription(
                subscribeRequest,
                ReadSubscribeResponse,
                CreateUnsubscribeRequest,
                ReadUnsubscribeResponse,
                HandleEventAsync,
                new SubscriptionState(mode, Name, handlers, state));

            await Cluster.Events.InstallSubscriptionAsync(subscription).CAF();

            return subscription.Id;
        }

        public Task<Guid> SubscribeAsync(Action<ReplicatedMapEventHandlers<TKey, TValue>> events, object state = null)
            => SubscribeAsync(events, Maybe.None, null, state);

        public Task<Guid> SubscribeAsync(Action<ReplicatedMapEventHandlers<TKey, TValue>> events, TKey key, object state = null)
            => SubscribeAsync(events, Maybe.Some(key), null, state);

        public Task<Guid> SubscribeAsync(Action<ReplicatedMapEventHandlers<TKey, TValue>> events, IPredicate predicate, object state = null)
            => SubscribeAsync(events, Maybe.None, predicate, state);

        public Task<Guid> SubscribeAsync(Action<ReplicatedMapEventHandlers<TKey, TValue>> events, TKey key, IPredicate predicate, object state = null)
            => SubscribeAsync(events, Maybe.Some(key), predicate, state);

        private class SubscriptionState : SubscriptionState<ReplicatedMapEventHandlers<TKey, TValue>>
        {
            public SubscriptionState(int mode, string name, ReplicatedMapEventHandlers<TKey, TValue> handlers, object state)
                : base(name, handlers, state)
            {
                Mode = mode;
            }

            public int Mode { get; }
        }

        private ValueTask HandleEventAsync(ClientMessage eventMessage, object state)
        {
            var sstate = ToSafeState<SubscriptionState>(state);

            return sstate.Mode switch
            {
                0 => ReplicatedMapAddEntryListenerCodec.HandleEventAsync(eventMessage, HandleEntryEventAsync, state, LoggerFactory),
                1 => ReplicatedMapAddEntryListenerToKeyCodec.HandleEventAsync(eventMessage, HandleEntryEventAsync, state, LoggerFactory),
                2 => ReplicatedMapAddEntryListenerWithPredicateCodec.HandleEventAsync(eventMessage, HandleEntryEventAsync, state, LoggerFactory),
                3 => ReplicatedMapAddEntryListenerToKeyWithPredicateCodec.HandleEventAsync(eventMessage, HandleEntryEventAsync, state, LoggerFactory),
                _ => throw new NotSupportedException()
            };
        }

        private async ValueTask HandleEntryEventAsync(IData keyData, IData valueData, IData oldValueData, IData mergingValueData, int eventTypeData, Guid memberId, int numberOfAffectedEntries, object state)
        {
            if (eventTypeData == 0) return;
            var eventType = (MapEventTypes) eventTypeData;

            var member = Cluster.Members.GetMember(memberId);
            var key = LazyArg<TKey>(keyData);
            var value = LazyArg<TValue>(valueData);
            var oldValue = LazyArg<TValue>(oldValueData);
            var mergingValue = LazyArg<TValue>(mergingValueData);

            var sstate = ToSafeState<SubscriptionState>(state);

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var handler in sstate.Handlers)
            {
                if (handler.EventType.HasAll(eventType))
                {
                    var task = handler switch
                    {
                        IMapEntryEventHandler<TKey, TValue, HReplicatedMap<TKey, TValue>> entryHandler => entryHandler.HandleAsync(this, member, key, value, oldValue, mergingValue, eventType, numberOfAffectedEntries, sstate.HandlerState),
                        IMapEventHandler<TKey, TValue, HReplicatedMap<TKey, TValue>> mapHandler => mapHandler.HandleAsync(this, member, numberOfAffectedEntries, sstate.HandlerState),
                        _ => throw new NotSupportedException()
                    };
                    await task.CAF();
                }
            }
        }

        private static ClientMessage CreateUnsubscribeRequest(Guid subscriptionId, object state)
        {
            var sstate = ToSafeState<SubscriptionState>(state);
            return ReplicatedMapRemoveEntryListenerCodec.EncodeRequest(sstate.Name, subscriptionId);
        }

        private static Guid ReadSubscribeResponse(ClientMessage responseMessage, object state)
        {
            var sstate = ToSafeState<SubscriptionState>(state);

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


        public async IAsyncEnumerator<KeyValuePair<TKey, TValue>> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            // all collections are async enumerable,
            // but by default we load the whole items set at once,
            // then iterate in memory
            var items = await GetEntriesAsync(cancellationToken).CAF();
            foreach (var item in items)
                yield return item;
        }
    }
}
