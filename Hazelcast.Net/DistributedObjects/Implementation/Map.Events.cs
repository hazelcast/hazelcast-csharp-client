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
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Messaging;
using Hazelcast.Predicates;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;

namespace Hazelcast.DistributedObjects.Implementation
{
    // partial: events
    internal partial class Map<TKey, TValue>
    {
        // TODO: could any of these events be ASYNC?!

        private async Task<Guid> SubscribeAsync(bool includeValues, IPredicate predicate, bool hasPredicate, TKey key, bool hasKey, Action<MapEvents<TKey, TValue>> on)
        {
            if (hasKey && key == null) throw new ArgumentNullException(nameof(key));
            if (hasPredicate && predicate == null) throw new ArgumentNullException(nameof(predicate));
            if (on == null) throw new ArgumentNullException(nameof(on));

            var subscriber = new MapEvents<TKey, TValue>();
            on(subscriber);

            var flags = MapEventType.Nothing;
            foreach (var handler in subscriber.Handlers)
                flags |= handler.EventType;

            // FIXME wtf
            var localOnly = false;

            // 0: no entryKey, no predicate
            // 1: entryKey, no predicate
            // 2: no entryKey, predicate
            // 3: entryKey, predicate
            var mode = (hasKey ? 1 : 0) + (hasPredicate ? 2 : 0);

            ClientMessage subscribeRequest;
            switch (mode)
            {
                case 0:
                    subscribeRequest = MapAddEntryListenerCodec.EncodeRequest(Name, includeValues, (int)flags, localOnly);
                    break;
                case 1:
                    subscribeRequest = MapAddEntryListenerToKeyCodec.EncodeRequest(Name, ToData(key), includeValues, (int)flags, localOnly);
                    break;
                case 2:
                    subscribeRequest = MapAddEntryListenerWithPredicateCodec.EncodeRequest(Name, ToData(predicate), includeValues, (int)flags, localOnly);
                    break;
                case 3:
                    subscribeRequest = MapAddEntryListenerToKeyWithPredicateCodec.EncodeRequest(Name, ToData(key), ToData(predicate), includeValues, (int)flags, localOnly);
                    break;
                default:
                    throw new Exception();
            }

            var subscription = new ClusterEventSubscription(
                subscribeRequest,
                HandleSubscribeResponse,
                CreateUnsubscribeRequest,
                HandleEvent,
                new SubscriptionState(mode, Name, subscriber.Handlers));

            await Cluster.SubscribeAsync(subscription);

            return subscription.Id;
        }

        public Task<Guid> SubscribeAsync(Action<MapEvents<TKey, TValue>> on)
            => SubscribeAsync(true, default, false, default, false, on);

        public Task<Guid> SubscribeAsync(bool includeValues, Action<MapEvents<TKey, TValue>> on)
            => SubscribeAsync(includeValues, default, false, default, false, on);

        public Task<Guid> SubscribeAsync(TKey key, Action<MapEvents<TKey, TValue>> on)
            => SubscribeAsync(true, default, false, key, true, on);

        public Task<Guid> SubscribeAsync(bool includeValues, TKey key, Action<MapEvents<TKey, TValue>> on)
            => SubscribeAsync(includeValues, default, false, key, true, on);

        public Task<Guid> SubscribeAsync(IPredicate predicate, Action<MapEvents<TKey, TValue>> on)
            => SubscribeAsync(true, predicate, true, default, false, on);

        public Task<Guid> SubscribeAsync(bool includeValues, IPredicate predicate, Action<MapEvents<TKey, TValue>> on)
            => SubscribeAsync(includeValues, predicate, true, default, false, on);

        public Task<Guid> SubscribeAsync(TKey key, IPredicate predicate, Action<MapEvents<TKey, TValue>> on)
            => SubscribeAsync(true, predicate, true, key, true, on);

        public Task<Guid> SubscribeAsync(bool includeValues, TKey key, IPredicate predicate, Action<MapEvents<TKey, TValue>> on)
            => SubscribeAsync(includeValues, predicate, true, key, true, on);


        private class SubscriptionState
        {
            public SubscriptionState(int mode, string name, List<IMapEventHandlerBase<TKey, TValue>> handlers)
            {
                Mode = mode;
                Name = name;
                Handlers = handlers;
            }

            public int Mode { get; }

            public string Name { get;}

            public List<IMapEventHandlerBase<TKey, TValue>> Handlers { get; }
        }

        private static SubscriptionState ToSafeState(object state)
        {
            if (state is SubscriptionState sstate) return sstate;
            throw new Exception();
        }

        private void HandleEvent(ClientMessage eventMessage, object state)
        {
            var sstate = ToSafeState(state);

            void HandleEntryEvent(IData keyData, IData valueData, IData oldValueData, IData mergingValueData, int eventTypeData, Guid memberId, int numberOfAffectedEntries)
            {
                var eventType = (MapEventType)eventTypeData;
                if (eventType == MapEventType.Nothing) return;

                Lazy<T> LazyArg<T>(IData source) => source == null ? null : new Lazy<T>(() => ToObject<T>(source));

                var member = Cluster.GetMember(memberId);

                // TODO: could this be optimized?
                var key = LazyArg<TKey>(keyData);
                var value = LazyArg<TValue>(valueData);
                var oldValue = LazyArg<TValue>(oldValueData);
                var mergingValue = LazyArg<TValue>(mergingValueData);

                // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                foreach (var handler in sstate.Handlers)
                {
                    if (handler.EventType.HasFlag(eventType)) // FIXME has any or...
                    {
                        switch (handler)
                        {
                            case IMapEntryEventHandler<TKey, TValue> entryHandler:
                                entryHandler.Handle(this, member, key, value, oldValue, mergingValue, eventType, numberOfAffectedEntries);
                                break;
                            case IMapEventHandler<TKey, TValue> mapHandler:
                                mapHandler.Handle(this, member, numberOfAffectedEntries);
                                break;
                            default:
                                throw new NotSupportedException();
                        }
                    }
                }
            }

            switch (sstate.Mode)
            {
                case 0:
                    MapAddEntryListenerCodec.EventHandler.HandleEvent(eventMessage, HandleEntryEvent);
                    break;
                case 1:
                    MapAddEntryListenerToKeyCodec.EventHandler.HandleEvent(eventMessage, HandleEntryEvent);
                    break;
                case 2:
                    MapAddEntryListenerWithPredicateCodec.EventHandler.HandleEvent(eventMessage, HandleEntryEvent);
                    break;
                case 3:
                    MapAddEntryListenerToKeyWithPredicateCodec.EventHandler.HandleEvent(eventMessage, HandleEntryEvent);
                    break;
                default:
                    throw new Exception();
            }
        }

        private static ClientMessage CreateUnsubscribeRequest(Guid subscriptionId, object state)
        {
            var sstate = ToSafeState(state);
            return MapRemoveEntryListenerCodec.EncodeRequest(sstate.Name, subscriptionId);
        }

        private static Guid HandleSubscribeResponse(ClientMessage responseMessage, object state)
        {
            var sstate = ToSafeState(state);

            switch (sstate.Mode)
            {
                case 0:
                    return MapAddEntryListenerCodec.DecodeResponse(responseMessage).Response;
                case 1:
                    return MapAddEntryListenerToKeyCodec.DecodeResponse(responseMessage).Response;
                case 2:
                    return MapAddEntryListenerWithPredicateCodec.DecodeResponse(responseMessage).Response;
                case 3:
                    return MapAddEntryListenerToKeyWithPredicateCodec.DecodeResponse(responseMessage).Response;
                default:
                    throw new Exception();
            }
        }

        /// <inheritdoc />
        public async Task<bool> UnsubscribeAsync(Guid subscriptionId)
        {
            // FIXME why would it return a bool?
            await Cluster.UnsubscribeAsync(subscriptionId);
            return true;
        }
    }
}