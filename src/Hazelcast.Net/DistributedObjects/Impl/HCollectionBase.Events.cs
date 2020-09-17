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
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Data;
using Hazelcast.Messaging;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.DistributedObjects.Impl
{
    internal partial class HCollectionBase<T> // Events
    {
        /// <inheritdoc />
        public async Task<Guid> SubscribeAsync(bool includeValue, Action<CollectionItemEventHandlers<T>> handle)
        {
            if (handle == null) throw new ArgumentNullException(nameof(handle));

            var handlers = new CollectionItemEventHandlers<T>();
            handle(handlers);

            var subscription = new ClusterSubscription(
                CreateSubscribeRequest(includeValue, Cluster.IsSmartRouting),
                ReadSubscribeResponse, CreateUnsubscribeRequest, ReadUnsubscribeResponse, HandleEventAsync,
                new SubscriptionState<CollectionItemEventHandlers<T>>(Name, handlers));

            await Cluster.Events.InstallSubscriptionAsync(subscription).CAF();

            return subscription.Id;
        }

        private ValueTask HandleEventAsync(ClientMessage eventMessage, object state)
        {
            var sstate = ToSafeState<SubscriptionState<CollectionItemEventHandlers<T>>>(state);

            async ValueTask HandleItemEventAsync(IData itemData, Guid memberId, int eventTypeData)
            {
                var eventType = (CollectionItemEventTypes) eventTypeData;
                if (eventType == CollectionItemEventTypes.Nothing) return;

                var member = Cluster.Members.GetMember(memberId);
                var item = LazyArg<T>(itemData);

                // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                foreach (var handler in sstate.Handlers)
                {
                    if (handler.EventType.HasAll(eventType))
                    {
                        await handler.HandleAsync(this, member, item).CAF();
                    }
                }
            }

            return CodecHandleEventAsync(eventMessage, HandleItemEventAsync, LoggerFactory);
        }

        protected abstract ValueTask CodecHandleEventAsync(ClientMessage eventMessage, Func<IData, Guid, int, ValueTask> f, ILoggerFactory loggerFactory);

        protected abstract ClientMessage CreateSubscribeRequest(bool includeValue, bool isSmartRouting);

        private ClientMessage CreateUnsubscribeRequest(Guid subscriptionId, object state)
        {
            var sstate = ToSafeState<SubscriptionState<CollectionItemEventHandlers<T>>>(state);
            return CreateUnsubscribeRequest(subscriptionId, sstate);
        }

        protected abstract ClientMessage CreateUnsubscribeRequest(Guid subscriptionId, SubscriptionState<CollectionItemEventHandlers<T>> state);

        private Guid ReadSubscribeResponse(ClientMessage responseMessage, object state)
        {
            var sstate = ToSafeState<SubscriptionState<CollectionItemEventHandlers<T>>>(state);
            return ReadSubscribeResponse(responseMessage, sstate);
        }

        protected abstract Guid ReadSubscribeResponse(ClientMessage responseMessage, SubscriptionState<CollectionItemEventHandlers<T>> state);

        private bool ReadUnsubscribeResponse(ClientMessage unsubscribeResponseMessage, object state)
        {
            var sstate = ToSafeState<SubscriptionState<CollectionItemEventHandlers<T>>>(state);
            return ReadUnsubscribeResponse(unsubscribeResponseMessage, sstate);
        }

        protected abstract bool ReadUnsubscribeResponse(ClientMessage unsubscribeResponseMessage, SubscriptionState<CollectionItemEventHandlers<T>> state);

        public ValueTask<bool> UnsubscribeAsync(Guid subscriptionId)
            => UnsubscribeBaseAsync(subscriptionId);
    }
}
