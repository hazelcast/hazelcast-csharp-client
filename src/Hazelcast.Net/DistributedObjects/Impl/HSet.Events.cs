// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Messaging;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.DistributedObjects.Impl
{
    internal partial class HSet<T> // Events
    {
        protected override ClientMessage CreateSubscribeRequest(bool includeValue, bool isSmartRouting)
            => SetAddListenerCodec.EncodeRequest(Name, includeValue, isSmartRouting);

        protected override ClientMessage CreateUnsubscribeRequest(Guid subscriptionId, SubscriptionState<CollectionItemEventHandlers<T>> state)
            => SetRemoveListenerCodec.EncodeRequest(Name, subscriptionId);

        protected override Guid ReadSubscribeResponse(ClientMessage responseMessage, SubscriptionState<CollectionItemEventHandlers<T>> state)
            => SetAddListenerCodec.DecodeResponse(responseMessage).Response;

        protected override bool ReadUnsubscribeResponse(ClientMessage unsubscribeResponseMessage, SubscriptionState<CollectionItemEventHandlers<T>> state)
            => SetRemoveListenerCodec.DecodeResponse(unsubscribeResponseMessage).Response;

        protected override ValueTask CodecHandleEventAsync(ClientMessage eventMessage, Func<IData, Guid, int, object, ValueTask> handler, object state, ILoggerFactory loggerFactory)
            => SetAddListenerCodec.HandleEventAsync(eventMessage, handler, state, loggerFactory);
    }
}
