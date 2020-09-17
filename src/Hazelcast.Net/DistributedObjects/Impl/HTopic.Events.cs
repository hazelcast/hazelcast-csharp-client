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
using Hazelcast.Messaging;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;

namespace Hazelcast.DistributedObjects.Impl
{
    internal partial class HTopic<T> // Events
    {
        /// <inheritdoc />
        public async Task<Guid> SubscribeAsync(Action<TopicEventHandlers<T>> handle)
        {
            if (handle == null) throw new ArgumentNullException(nameof(handle));

            var handlers = new TopicEventHandlers<T>();
            handle(handlers);

            var subscription = new ClusterSubscription(
                TopicAddMessageListenerCodec.EncodeRequest(Name, Cluster.IsSmartRouting),
                ReadSubscribeResponse,
                CreateUnsubscribeRequest,
                ReadUnsubscribeResponse,
                HandleEventAsync,
                new SubscriptionState<TopicEventHandlers<T>>(Name, handlers));

            await Cluster.Events.InstallSubscriptionAsync(subscription).CAF();

            return subscription.Id;
        }

        private ValueTask HandleEventAsync(ClientMessage eventMessage, object state)
        {
            var sstate = ToSafeState<SubscriptionState<TopicEventHandlers<T>>>(state);

            async ValueTask HandleEventAsync(IData itemData, long publishTime, Guid memberId)
            {
                var member = Cluster.Members.GetMember(memberId);

                // that one is not lazy...
                var item = ToObject<T>(itemData);

                // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                foreach (var handler in sstate.Handlers)
                {
                    // there is only one event type...
                    await handler.HandleAsync(this, member, publishTime, item).CAF();
                }
            }

            return TopicAddMessageListenerCodec.HandleEventAsync(eventMessage, HandleEventAsync, LoggerFactory);
        }

        private ClientMessage CreateUnsubscribeRequest(Guid subscriptionId, object state)
            => TopicRemoveMessageListenerCodec.EncodeRequest(Name, subscriptionId);

        private static Guid ReadSubscribeResponse(ClientMessage responseMessage, object state)
            => TopicAddMessageListenerCodec.DecodeResponse(responseMessage).Response;

        private static bool ReadUnsubscribeResponse(ClientMessage message, object state)
            => TopicRemoveMessageListenerCodec.DecodeResponse(message).Response;

        public ValueTask<bool> UnsubscribeAsync(Guid subscriptionId)
            => UnsubscribeBaseAsync(subscriptionId);
    }
}
