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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Data;
using Hazelcast.Messaging;
using Hazelcast.Query;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;

namespace Hazelcast.DistributedObjects.Impl
{
    internal partial class HMap<TKey, TValue> // Events
    {
        private async Task<Guid> SubscribeAsync(Action<MapEventHandlers<TKey, TValue>> events, Maybe<TKey> key, IPredicate predicate, bool includeValues, object state, CancellationToken cancellationToken)
        {
            if (events == null) throw new ArgumentNullException(nameof(events));

            var handlers = new MapEventHandlers<TKey, TValue>();
            events(handlers);

            var flags = default (MapEventTypes);
            foreach (var handler in handlers)
                flags |= handler.EventType;

            // 0: no entryKey, no predicate
            // 1: entryKey, no predicate
            // 2: no entryKey, predicate
            // 3: entryKey, predicate
            var mode = key.Match(1, 0) + (predicate != null ? 2 : 0);
            var keyv = key.ValueOrDefault();

            var subscribeRequest = mode switch
            {
                0 => MapAddEntryListenerCodec.EncodeRequest(Name, includeValues, (int) flags, Cluster.IsSmartRouting),
                1 => MapAddEntryListenerToKeyCodec.EncodeRequest(Name, ToData(keyv), includeValues, (int) flags, Cluster.IsSmartRouting),
                2 => MapAddEntryListenerWithPredicateCodec.EncodeRequest(Name, ToData(predicate), includeValues, (int) flags, Cluster.IsSmartRouting),
                3 => MapAddEntryListenerToKeyWithPredicateCodec.EncodeRequest(Name, ToData(keyv), ToData(predicate), includeValues, (int) flags, Cluster.IsSmartRouting),
                _ => throw new NotSupportedException()
            };

            var subscription = new ClusterSubscription(
                subscribeRequest,
                ReadSubscribeResponse,
                CreateUnsubscribeRequest,
                ReadUnsubscribeResponse,
                HandleEventAsync,
                new MapSubscriptionState(mode, Name, handlers, state));

            await Cluster.Events.InstallSubscriptionAsync(subscription, cancellationToken).CAF();

            return subscription.Id;
        }

        public Task<Guid> SubscribeAsync(Action<MapEventHandlers<TKey, TValue>> events, bool includeValues = true, object state = null)
            => SubscribeAsync(events, Maybe.None, null, includeValues, state, CancellationToken.None);

        public Task<Guid> SubscribeAsync(Action<MapEventHandlers<TKey, TValue>> events, TKey key, bool includeValues = true, object state = null)
            => SubscribeAsync(events, Maybe.Some(key), null, includeValues, state, CancellationToken.None);

        public Task<Guid> SubscribeAsync(Action<MapEventHandlers<TKey, TValue>> events, IPredicate predicate, bool includeValues = true, object state = null)
            => SubscribeAsync(events, Maybe.None, predicate, includeValues, state, CancellationToken.None);

        public Task<Guid> SubscribeAsync(Action<MapEventHandlers<TKey, TValue>> events, TKey key, IPredicate predicate, bool includeValues = true, object state = null)
            => SubscribeAsync(events, Maybe.Some(key), predicate, includeValues, state, CancellationToken.None);

        private class MapSubscriptionState : SubscriptionState<MapEventHandlers<TKey, TValue>>
        {
            public MapSubscriptionState(int mode, string name, MapEventHandlers<TKey, TValue> handlers, object state)
                : base(name, handlers, state)
            {
                Mode = mode;
            }

            public int Mode { get; }
        }

        private ValueTask HandleEventAsync(ClientMessage eventMessage, object state)
        {
            var sstate = ToSafeState<MapSubscriptionState>(state);

            return sstate.Mode switch
            {
                0 => MapAddEntryListenerCodec.HandleEventAsync(eventMessage, HandleEntryEvent, state, LoggerFactory),
                1 => MapAddEntryListenerToKeyCodec.HandleEventAsync(eventMessage, HandleEntryEvent, state, LoggerFactory),
                2 => MapAddEntryListenerWithPredicateCodec.HandleEventAsync(eventMessage, HandleEntryEvent, state, LoggerFactory),
                3 => MapAddEntryListenerToKeyWithPredicateCodec.HandleEventAsync(eventMessage, HandleEntryEvent, state, LoggerFactory),
                _ => throw new NotSupportedException()
            };
        }

        private async ValueTask HandleEntryEvent(IData keyData, IData valueData, IData oldValueData, IData mergingValueData, int eventTypeData, Guid memberId, int numberOfAffectedEntries, object state)
        {
            if (eventTypeData == 0) return;
            var eventType = (MapEventTypes) eventTypeData;

            var member = Cluster.Members.GetMember(memberId);
            var key = LazyArg<TKey>(keyData);
            var value = LazyArg<TValue>(valueData);
            var oldValue = LazyArg<TValue>(oldValueData);
            var mergingValue = LazyArg<TValue>(mergingValueData);

            var sstate = ToSafeState<MapSubscriptionState>(state);

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var handler in sstate.Handlers)
            {
                if (handler.EventType.HasAll(eventType))
                {
                    var task = handler switch
                    {
                        IMapEntryEventHandler<TKey, TValue, IHMap<TKey, TValue>> entryHandler => entryHandler.HandleAsync(this, member, key, value, oldValue, mergingValue, eventType, numberOfAffectedEntries, state),
                        IMapEventHandler<TKey, TValue, IHMap<TKey, TValue>> mapHandler => mapHandler.HandleAsync(this, member, numberOfAffectedEntries, state),
                        _ => throw new NotSupportedException()
                    };
                    await task.CAF();
                }
            }
        }

        private static ClientMessage CreateUnsubscribeRequest(Guid subscriptionId, object state)
        {
            var sstate = ToSafeState<MapSubscriptionState>(state);
            return MapRemoveEntryListenerCodec.EncodeRequest(sstate.Name, subscriptionId);
        }

        private static Guid ReadSubscribeResponse(ClientMessage responseMessage, object state)
        {
            var sstate = ToSafeState<MapSubscriptionState>(state);

            return sstate.Mode switch
            {
                0 => MapAddEntryListenerCodec.DecodeResponse(responseMessage).Response,
                1 => MapAddEntryListenerToKeyCodec.DecodeResponse(responseMessage).Response,
                2 => MapAddEntryListenerWithPredicateCodec.DecodeResponse(responseMessage).Response,
                3 => MapAddEntryListenerToKeyWithPredicateCodec.DecodeResponse(responseMessage).Response,
                _ => throw new NotSupportedException()
            };
        }

        private static bool ReadUnsubscribeResponse(ClientMessage unsubscribeResponseMessage, object state)
        {
            return MapRemoveEntryListenerCodec.DecodeResponse(unsubscribeResponseMessage).Response;
        }

        public ValueTask<bool> UnsubscribeAsync(Guid subscriptionId)
            => UnsubscribeBaseAsync(subscriptionId);
    }
}
