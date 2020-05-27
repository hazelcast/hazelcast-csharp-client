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
using Hazelcast.Protocol.Codecs;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Clustering
{
    internal class ObjectLifecycleEventSubscription : EventSubscriptionBase
    {
        private readonly bool _isSmart;

        public ObjectLifecycleEventSubscription(Cluster cluster, ILoggerFactory loggerFactory, bool isSmart)
            : base(cluster, loggerFactory)
        {
            _isSmart = isSmart;
        }

        protected override ClusterSubscription CreateSubscription()
        {
            return new ClusterSubscription(
                ClientAddDistributedObjectListenerCodec.EncodeRequest(_isSmart),
                (message, state) => ClientAddDistributedObjectListenerCodec.DecodeResponse(message).Response,
                (id, state) => ClientRemoveDistributedObjectListenerCodec.EncodeRequest(id),
                (message, state) => ClientRemoveDistributedObjectListenerCodec.DecodeResponse(message).Response,
                (message, state) => ClientAddDistributedObjectListenerCodec.HandleEvent(message, HandleInternal, LoggerFactory));
        }

        internal Action<ClusterObjectLifecycleEventType, ClusterObjectLifecycleEventArgs> Handle { get; set; }

        private void HandleInternal(string name, string serviceName, string eventTypeName, Guid memberId)
        {
            switch (eventTypeName)
            {
                case "CREATED":
                    Handle(ClusterObjectLifecycleEventType.Created, new ClusterObjectLifecycleEventArgs(serviceName, name, memberId));
                    break;
                case "DESTROYED":
                    Handle(ClusterObjectLifecycleEventType.Destroyed, new ClusterObjectLifecycleEventArgs(serviceName, name, memberId));
                    break;
                default:
                    throw new NotSupportedException($"Event type \"{eventTypeName}\" is not supported.");
            }
        }
    }
}