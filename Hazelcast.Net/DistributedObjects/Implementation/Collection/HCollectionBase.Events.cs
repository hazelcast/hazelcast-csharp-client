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
using Hazelcast.Data.Collection;
using Hazelcast.Messaging;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;

namespace Hazelcast.DistributedObjects.Implementation.Collection
{
    internal partial class HCollectionBase<T> // Events
    {
        /// <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
            Task<Guid> SubscribeAsync(bool includeValue, Action<CollectionItemEventHandlers<T>> on, TimeSpan timeout = default)
        {
            var cancellation = timeout.AsCancellationTokenSource(DefaultOperationTimeoutMilliseconds);
            var task = SubscribeAsync(includeValue, on, cancellation.Token).OrTimeout(cancellation);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <inheritdoc />
        public async Task<Guid> SubscribeAsync(bool includeValue, Action<CollectionItemEventHandlers<T>> on, CancellationToken cancellationToken)
        {
            if (on == null) throw new ArgumentNullException(nameof(on));

            var handlers = new CollectionItemEventHandlers<T>();
            on(handlers);

            var subscribeRequest = CreateSubscribeRequest(Name, includeValue, Cluster.IsSmartRouting);

            var subscription = new ClusterSubscription(
                subscribeRequest,
                HandleSubscribeResponse,
                CreateUnsubscribeRequest,
                DecodeUnsubscribeResponse,
                HandleEvent,
                new SubscriptionState<CollectionItemEventHandlers<T>>(Name, handlers));

            await Cluster.InstallSubscriptionAsync(subscription, cancellationToken).CAF();

            return subscription.Id;
        }

        private void HandleEvent(ClientMessage eventMessage, object state)
        {
            var sstate = ToSafeState<SubscriptionState<CollectionItemEventHandlers<T>>>(state);

            void HandleItemEvent(IData itemData, Guid memberId, int eventTypeData)
            {
                var eventType = (CollectionItemEventType)eventTypeData;
                if (eventType == CollectionItemEventType.Nothing) return;

                var member = Cluster.GetMember(memberId);
                var item = LazyArg<T>(itemData);

                // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                foreach (var handler in sstate.Handlers)
                {
                    if (handler.EventType.HasFlag(eventType)) // FIXME has any or...
                    {
                        handler.Handle(this, member, item);
                    }
                }
            }

            ListAddListenerCodec.HandleEvent(eventMessage, HandleItemEvent, LoggerFactory);
        }

        protected abstract ClientMessage CreateSubscribeRequest(string name, bool includeValue, bool isSmartRouting);

        private ClientMessage CreateUnsubscribeRequest(Guid subscriptionId, object state)
        {
            var sstate = ToSafeState<SubscriptionState<CollectionItemEventHandlers<T>>>(state);
            return CreateUnsubscribeRequest(subscriptionId, sstate);
        }

        protected abstract ClientMessage CreateUnsubscribeRequest(Guid subscriptionId, SubscriptionState<CollectionItemEventHandlers<T>> state);

        private Guid HandleSubscribeResponse(ClientMessage responseMessage, object state)
        {
            var sstate = ToSafeState<SubscriptionState<CollectionItemEventHandlers<T>>>(state);
            return HandleSubscribeResponse(responseMessage, sstate);
        }

        protected abstract Guid HandleSubscribeResponse(ClientMessage responseMessage, SubscriptionState<CollectionItemEventHandlers<T>> state);

        private bool DecodeUnsubscribeResponse(ClientMessage unsubscribeResponseMessage, object state)
        {
            var sstate = ToSafeState<SubscriptionState<CollectionItemEventHandlers<T>>>(state);
            return DecodeUnsubscribeResponse(unsubscribeResponseMessage, sstate);
        }

        protected abstract bool DecodeUnsubscribeResponse(ClientMessage unsubscribeResponseMessage, SubscriptionState<CollectionItemEventHandlers<T>> state);
    }
}