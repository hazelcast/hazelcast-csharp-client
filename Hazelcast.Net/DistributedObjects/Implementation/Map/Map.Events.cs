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
using Hazelcast.Data.Map;
using Hazelcast.Messaging;
using Hazelcast.Predicates;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;

namespace Hazelcast.DistributedObjects.Implementation.Map
{
    internal partial class Map<TKey, TValue> // Events
    {
        // TODO: could any of these events be ASYNC?!

        private async Task<Guid> SubscribeAsync(bool includeValues, IPredicate predicate, bool hasPredicate, TKey key, bool hasKey, Action<MapEventHandlers<TKey, TValue>> on, CancellationToken cancellationToken)
        {
            if (hasKey && key == null) throw new ArgumentNullException(nameof(key));
            if (hasPredicate && predicate == null) throw new ArgumentNullException(nameof(predicate));
            if (on == null) throw new ArgumentNullException(nameof(on));

            var handlers = new MapEventHandlers<TKey, TValue>();
            on(handlers);

            var flags = MapEventType.Nothing;
            foreach (var handler in handlers)
                flags |= handler.EventType;

            // 0: no entryKey, no predicate
            // 1: entryKey, no predicate
            // 2: no entryKey, predicate
            // 3: entryKey, predicate
            var mode = (hasKey ? 1 : 0) + (hasPredicate ? 2 : 0);

            ClientMessage subscribeRequest;
            switch (mode)
            {
                case 0:
                    subscribeRequest = MapAddEntryListenerCodec.EncodeRequest(Name, includeValues, (int)flags, Cluster.IsSmartRouting);
                    break;
                case 1:
                    subscribeRequest = MapAddEntryListenerToKeyCodec.EncodeRequest(Name, ToData(key), includeValues, (int)flags, Cluster.IsSmartRouting);
                    break;
                case 2:
                    subscribeRequest = MapAddEntryListenerWithPredicateCodec.EncodeRequest(Name, ToData(predicate), includeValues, (int)flags, Cluster.IsSmartRouting);
                    break;
                case 3:
                    subscribeRequest = MapAddEntryListenerToKeyWithPredicateCodec.EncodeRequest(Name, ToData(key), ToData(predicate), includeValues, (int)flags, Cluster.IsSmartRouting);
                    break;
                default:
                    throw new Exception();
            }

            var subscription = new ClusterSubscription(
                subscribeRequest,
                HandleSubscribeResponse,
                CreateUnsubscribeRequest,
                DecodeUnsubscribeResponse,
                HandleEvent,
                new SubscriptionState(mode, Name, handlers));

            await Cluster.InstallSubscriptionAsync(subscription, cancellationToken).CAF();

            return subscription.Id;
        }

        public Task<Guid> SubscribeAsync(Action<MapEventHandlers<TKey, TValue>> on, TimeSpan timeout = default)
        {
            var cancellation = timeout.AsCancellationTokenSource(Constants.DistributedObjects.DefaultOperationTimeoutMilliseconds);
            return SubscribeAsync(true, default, false, default, false, on, cancellation.Token).OrTimeout(cancellation);
        }

        public Task<Guid> SubscribeAsync(Action<MapEventHandlers<TKey, TValue>> on, CancellationToken cancellationToken)
            => SubscribeAsync(true, default, false, default, false, on, cancellationToken);

        public Task<Guid> SubscribeAsync(bool includeValues, Action<MapEventHandlers<TKey, TValue>> on, TimeSpan timeout = default)
        {
            var cancellation = timeout.AsCancellationTokenSource(Constants.DistributedObjects.DefaultOperationTimeoutMilliseconds);
            return SubscribeAsync(includeValues, default, false, default, false, on, cancellation.Token).OrTimeout(cancellation);
        }

        public Task<Guid> SubscribeAsync(bool includeValues, Action<MapEventHandlers<TKey, TValue>> on, CancellationToken cancellationToken)
            => SubscribeAsync(includeValues, default, false, default, false, on, cancellationToken);

        public Task<Guid> SubscribeAsync(TKey key, Action<MapEventHandlers<TKey, TValue>> on, TimeSpan timeout = default)
        {
            var cancellation = timeout.AsCancellationTokenSource(Constants.DistributedObjects.DefaultOperationTimeoutMilliseconds);
            return SubscribeAsync(true, default, false, key, true, on, cancellation.Token).OrTimeout(cancellation);
        }

        public Task<Guid> SubscribeAsync(TKey key, Action<MapEventHandlers<TKey, TValue>> on, CancellationToken cancellationToken)
            => SubscribeAsync(true, default, false, key, true, on, cancellationToken);

        public Task<Guid> SubscribeAsync(bool includeValues, TKey key, Action<MapEventHandlers<TKey, TValue>> on, TimeSpan timeout = default)
        {
            var cancellation = timeout.AsCancellationTokenSource(Constants.DistributedObjects.DefaultOperationTimeoutMilliseconds);
            return SubscribeAsync(includeValues, default, false, key, true, on, cancellation.Token).OrTimeout(cancellation);
        }

        public Task<Guid> SubscribeAsync(bool includeValues, TKey key, Action<MapEventHandlers<TKey, TValue>> on, CancellationToken cancellationToken)
            => SubscribeAsync(includeValues, default, false, key, true, on, cancellationToken);

        public Task<Guid> SubscribeAsync(IPredicate predicate, Action<MapEventHandlers<TKey, TValue>> on, TimeSpan timeout = default)
        {
            var cancellation = timeout.AsCancellationTokenSource(Constants.DistributedObjects.DefaultOperationTimeoutMilliseconds);
            return SubscribeAsync(true, predicate, true, default, false, on, cancellation.Token).OrTimeout(cancellation);
        }

        public Task<Guid> SubscribeAsync(IPredicate predicate, Action<MapEventHandlers<TKey, TValue>> on, CancellationToken cancellationToken)
            => SubscribeAsync(true, predicate, true, default, false, on, cancellationToken);

        public Task<Guid> SubscribeAsync(bool includeValues, IPredicate predicate, Action<MapEventHandlers<TKey, TValue>> on, TimeSpan timeout = default)
        {
            var cancellation = timeout.AsCancellationTokenSource(Constants.DistributedObjects.DefaultOperationTimeoutMilliseconds);
            return SubscribeAsync(includeValues, predicate, true, default, false, on, cancellation.Token).OrTimeout(cancellation);
        }

        public Task<Guid> SubscribeAsync(bool includeValues, IPredicate predicate, Action<MapEventHandlers<TKey, TValue>> on, CancellationToken cancellationToken)
            => SubscribeAsync(includeValues, predicate, true, default, false, on, cancellationToken);

        public Task<Guid> SubscribeAsync(TKey key, IPredicate predicate, Action<MapEventHandlers<TKey, TValue>> on, TimeSpan timeout = default)
        {
            var cancellation = timeout.AsCancellationTokenSource(Constants.DistributedObjects.DefaultOperationTimeoutMilliseconds);
            return SubscribeAsync(true, predicate, true, key, true, on, cancellation.Token).OrTimeout(cancellation);
        }

        public Task<Guid> SubscribeAsync(TKey key, IPredicate predicate, Action<MapEventHandlers<TKey, TValue>> on, CancellationToken cancellationToken)
            => SubscribeAsync(true, predicate, true, key, true, on, cancellationToken);

        public Task<Guid> SubscribeAsync(bool includeValues, TKey key, IPredicate predicate, Action<MapEventHandlers<TKey, TValue>> on, TimeSpan timeout = default)
        {
            var cancellation = timeout.AsCancellationTokenSource(Constants.DistributedObjects.DefaultOperationTimeoutMilliseconds);
            return SubscribeAsync(includeValues, predicate, true, key, true, on, cancellation.Token).OrTimeout(cancellation);
        }

        public Task<Guid> SubscribeAsync(bool includeValues, TKey key, IPredicate predicate, Action<MapEventHandlers<TKey, TValue>> on, CancellationToken cancellationToken)
            => SubscribeAsync(includeValues, predicate, true, key, true, on, cancellationToken);

        private class SubscriptionState
        {
            public SubscriptionState(int mode, string name, MapEventHandlers<TKey, TValue> handlers)
            {
                Mode = mode;
                Name = name;
                Handlers = handlers;
            }

            public int Mode { get; }

            public string Name { get;}

            public MapEventHandlers<TKey, TValue> Handlers { get; }
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
                    MapAddEntryListenerCodec.HandleEvent(eventMessage, HandleEntryEvent, LoggerFactory);
                    break;
                case 1:
                    MapAddEntryListenerToKeyCodec.HandleEvent(eventMessage, HandleEntryEvent, LoggerFactory);
                    break;
                case 2:
                    MapAddEntryListenerWithPredicateCodec.HandleEvent(eventMessage, HandleEntryEvent, LoggerFactory);
                    break;
                case 3:
                    MapAddEntryListenerToKeyWithPredicateCodec.HandleEvent(eventMessage, HandleEntryEvent, LoggerFactory);
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
                    throw new NotSupportedException();
            }
        }

        private static bool DecodeUnsubscribeResponse(ClientMessage unsubscribeResponseMessage, object state)
        {
            return MapRemoveEntryListenerCodec.DecodeResponse(unsubscribeResponseMessage).Response;
        }

        /// <inheritdoc />
        public
#if !OPTIMIZE_ASYNC
            async
#endif
        Task UnsubscribeAsync(Guid subscriptionId, TimeSpan timeout = default)
        {
            var cancellation = timeout.AsCancellationTokenSource(Constants.DistributedObjects.DefaultOperationTimeoutMilliseconds);
            var task = UnsubscribeAsync(subscriptionId, cancellation.Token).OrTimeout(cancellation);

#if OPTIMIZE_ASYNC
            return task;
#else
            await task.CAF();
#endif
        }

        /// <inheritdoc />
        public
#if !OPTIMIZE_ASYNC
            async
#endif
        Task UnsubscribeAsync(Guid subscriptionId, CancellationToken cancellationToken)
        {
            var task = Cluster.RemoveSubscriptionAsync(subscriptionId, cancellationToken);

#if OPTIMIZE_ASYNC
            return task;
#else
            await task.CAF();
#endif
        }
    }
}