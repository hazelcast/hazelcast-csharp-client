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
using Hazelcast.Data.Topic;
using Hazelcast.Messaging;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;

namespace Hazelcast.DistributedObjects.Implementation.Topic
{
    // partial: events
    internal partial class Topic<T>
    {
        /// <inheritdoc />
        public async Task<Guid> SubscribeAsync(Action<TopicEventHandlers<T>> on)
        {
            if (on == null) throw new ArgumentNullException(nameof(on));

            var handlers = new TopicEventHandlers<T>();
            on(handlers);

            var flags = TopicEventType.Nothing;
            foreach (var handler in handlers)
                flags |= handler.EventType;

            var subscribeRequest = TopicAddMessageListenerCodec.EncodeRequest(Name, Cluster.IsSmartRouting);

            var subscription = new ClusterSubscription(
                subscribeRequest,
                HandleSubscribeResponse,
                CreateUnsubscribeRequest,
                HandleEvent,
                new SubscriptionState(Name, handlers));

            await Cluster.InstallSubscriptionAsync(subscription);

            return subscription.Id;
        }

        private sealed class SubscriptionState
        {
            public SubscriptionState(string name, TopicEventHandlers<T> handlers)
            {
                Name = name;
                Handlers = handlers;
            }

            public string Name { get; }

            public TopicEventHandlers<T> Handlers { get; }
        }

        private static SubscriptionState ToSafeState(object state)
        {
            if (state is SubscriptionState sstate) return sstate;
            throw new Exception();
        }

        private void HandleEvent(ClientMessage eventMessage, object state)
        {
            var sstate = ToSafeState(state);

            void HandleEvent(IData itemData, long publishTime, Guid memberId)
            {
                //Lazy<T> LazyArg<T>(IData source) => source == null ? null : new Lazy<T>(() => ToObject<T>(source));

                var member = Cluster.GetMember(memberId);

                // that one is not lazy...
                var item = ToObject<T>(itemData);

                // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                foreach (var handler in sstate.Handlers)
                {
                    // there is only one event type...
                    handler.Handle(this, member, publishTime, item);
                }
            }

            TopicAddMessageListenerCodec.HandleEvent(eventMessage, HandleEvent, LoggerFactory);
        }

        private static ClientMessage CreateUnsubscribeRequest(Guid subscriptionId, object state)
        {
            var sstate = ToSafeState(state);
            return TopicRemoveMessageListenerCodec.EncodeRequest(sstate.Name, subscriptionId);
        }

        private static Guid HandleSubscribeResponse(ClientMessage responseMessage, object state)
        {
            return TopicAddMessageListenerCodec.DecodeResponse(responseMessage).Response;
        }

        /// <inheritdoc />
        public async Task<bool> UnsubscribeAsync(Guid subscriptionId)
        {
            // FIXME why would it return a bool?
            await Cluster.RemoveSubscriptionAsync(subscriptionId);
            return true;
        }
    }
}