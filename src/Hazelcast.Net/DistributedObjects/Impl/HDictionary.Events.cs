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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Data;
using Hazelcast.Messaging;
using Hazelcast.Predicates;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;

namespace Hazelcast.DistributedObjects.Impl
{
    internal partial class HDictionary<TKey, TValue> // Events
    {
        private async Task<Guid> SubscribeAsync(bool includeValues, IPredicate predicate, bool hasPredicate, TKey key, bool hasKey, Action<DictionaryEventHandlers<TKey, TValue>> handle, CancellationToken cancellationToken)
        {
            if (hasKey && key == null) throw new ArgumentNullException(nameof(key));
            if (hasPredicate && predicate == null) throw new ArgumentNullException(nameof(predicate));
            if (handle == null) throw new ArgumentNullException(nameof(handle));

            var handlers = new DictionaryEventHandlers<TKey, TValue>();
            handle(handlers);

            var flags = HDictionaryEventTypes.Nothing;
            foreach (var handler in handlers)
                flags |= handler.EventType;

            // 0: no entryKey, no predicate
            // 1: entryKey, no predicate
            // 2: no entryKey, predicate
            // 3: entryKey, predicate
            var mode = (hasKey ? 1 : 0) + (hasPredicate ? 2 : 0);

            var subscribeRequest = mode switch
            {
                0 => MapAddEntryListenerCodec.EncodeRequest(Name, includeValues, (int) flags, Cluster.IsSmartRouting),
                1 => MapAddEntryListenerToKeyCodec.EncodeRequest(Name, ToData(key), includeValues, (int) flags, Cluster.IsSmartRouting),
                2 => MapAddEntryListenerWithPredicateCodec.EncodeRequest(Name, ToData(predicate), includeValues, (int) flags, Cluster.IsSmartRouting),
                3 => MapAddEntryListenerToKeyWithPredicateCodec.EncodeRequest(Name, ToData(key), ToData(predicate), includeValues, (int) flags, Cluster.IsSmartRouting),
                _ => throw new NotSupportedException()
            };

            var subscription = new ClusterSubscription(
                subscribeRequest,
                ReadSubscribeResponse,
                CreateUnsubscribeRequest,
                ReadUnsubscribeResponse,
                HandleEventAsync,
                new MapSubscriptionState(mode, Name, handlers));

            await Cluster.Events.InstallSubscriptionAsync(subscription, cancellationToken).CAF();

            return subscription.Id;
        }

        public Task<Guid> SubscribeAsync(Action<DictionaryEventHandlers<TKey, TValue>> handle)
            => SubscribeAsync(true, default, false, default, false, handle, CancellationToken.None);

        public Task<Guid> SubscribeAsync(bool includeValues, Action<DictionaryEventHandlers<TKey, TValue>> handle)
            => SubscribeAsync(includeValues, default, false, default, false, handle, CancellationToken.None);

        public Task<Guid> SubscribeAsync(TKey key, Action<DictionaryEventHandlers<TKey, TValue>> handle)
            => SubscribeAsync(true, default, false, key, true, handle, CancellationToken.None);

        public Task<Guid> SubscribeAsync(bool includeValues, TKey key, Action<DictionaryEventHandlers<TKey, TValue>> handle)
            => SubscribeAsync(includeValues, default, false, key, true, handle, CancellationToken.None);

        public Task<Guid> SubscribeAsync(IPredicate predicate, Action<DictionaryEventHandlers<TKey, TValue>> handle)
            => SubscribeAsync(true, predicate, true, default, false, handle, CancellationToken.None);

        public Task<Guid> SubscribeAsync(bool includeValues, IPredicate predicate, Action<DictionaryEventHandlers<TKey, TValue>> handle)
            => SubscribeAsync(includeValues, predicate, true, default, false, handle, CancellationToken.None);

        public Task<Guid> SubscribeAsync(TKey key, IPredicate predicate, Action<DictionaryEventHandlers<TKey, TValue>> handle)
            => SubscribeAsync(true, predicate, true, key, true, handle, CancellationToken.None);

        public Task<Guid> SubscribeAsync(bool includeValues, TKey key, IPredicate predicate, Action<DictionaryEventHandlers<TKey, TValue>> handle)
            => SubscribeAsync(includeValues, predicate, true, key, true, handle, CancellationToken.None);

        private class MapSubscriptionState : SubscriptionState<DictionaryEventHandlers<TKey, TValue>>
        {
            public MapSubscriptionState(int mode, string name, DictionaryEventHandlers<TKey, TValue> handlers)
                : base(name, handlers)
            {
                Mode = mode;
            }

            public int Mode { get; }
        }

        private ValueTask HandleEventAsync(ClientMessage eventMessage, object state)
        {
            var sstate = ToSafeState<MapSubscriptionState>(state);

            async ValueTask HandleEntryEvent(IData keyData, IData valueData, IData oldValueData, IData mergingValueData, int eventTypeData, Guid memberId, int numberOfAffectedEntries)
            {
                var eventType = (HDictionaryEventTypes) eventTypeData;
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
                            IDictionaryEntryEventHandler<TKey, TValue, IHDictionary<TKey, TValue>> entryHandler => entryHandler.HandleAsync(this, member, key, value, oldValue, mergingValue, eventType, numberOfAffectedEntries),
                            IDictionaryEventHandler<TKey, TValue, IHDictionary<TKey, TValue>> mapHandler => mapHandler.HandleAsync(this, member, numberOfAffectedEntries),
                            _ => throw new NotSupportedException()
                        };
                        await task.CAF();
                    }
                }
            }

            return sstate.Mode switch
            {
                0 => MapAddEntryListenerCodec.HandleEventAsync(eventMessage, HandleEntryEvent, LoggerFactory),
                1 => MapAddEntryListenerToKeyCodec.HandleEventAsync(eventMessage, HandleEntryEvent, LoggerFactory),
                2 => MapAddEntryListenerWithPredicateCodec.HandleEventAsync(eventMessage, HandleEntryEvent, LoggerFactory),
                3 => MapAddEntryListenerToKeyWithPredicateCodec.HandleEventAsync(eventMessage, HandleEntryEvent, LoggerFactory),
                _ => throw new NotSupportedException()
            };
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
