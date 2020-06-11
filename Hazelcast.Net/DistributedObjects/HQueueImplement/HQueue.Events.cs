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
using Hazelcast.Messaging;
using Hazelcast.Protocol.Codecs;

namespace Hazelcast.DistributedObjects.HQueueImplement
{
    internal partial class HQueue<T> // Events
    {
        /// <inheritdoc />
        protected override ClientMessage CreateSubscribeRequest(bool includeValue, bool isSmartRouting)
            => QueueAddListenerCodec.EncodeRequest(Name, includeValue, isSmartRouting);

        /// <inheritdoc />
        protected override ClientMessage CreateUnsubscribeRequest(Guid subscriptionId, SubscriptionState<CollectionItemEventHandlers<T>> state)
            => QueueRemoveListenerCodec.EncodeRequest(Name, subscriptionId);

        /// <inheritdoc />
        protected override Guid ReadSubscribeResponse(ClientMessage responseMessage, SubscriptionState<CollectionItemEventHandlers<T>> state)
            => QueueAddListenerCodec.DecodeResponse(responseMessage).Response;

        /// <inheritdoc />
        protected override bool ReadUnsubscribeResponse(ClientMessage unsubscribeResponseMessage, SubscriptionState<CollectionItemEventHandlers<T>> state)
            => QueueRemoveListenerCodec.DecodeResponse(unsubscribeResponseMessage).Response;
    }
}