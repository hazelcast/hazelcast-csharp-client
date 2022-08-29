// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Core;
using Hazelcast.Events;
using Hazelcast.Protocol.Codecs;

namespace Hazelcast.Clustering
{
    internal class ObjectLifecycleEventSubscription : EventSubscriptionBase
    {
        private readonly bool _isSmart;

        public ObjectLifecycleEventSubscription(ClusterState clusterState, ClusterEvents clusterEvents)
            : base(clusterState, clusterEvents)
        {
            _isSmart = clusterState.Options.Networking.SmartRouting;
        }

        protected override ClusterSubscription CreateSubscription()
        {
            return new ClusterSubscription(
                ClientAddDistributedObjectListenerCodec.EncodeRequest(_isSmart),
                (message, state) => ClientAddDistributedObjectListenerCodec.DecodeResponse(message).Response,
                (id, state) => ClientRemoveDistributedObjectListenerCodec.EncodeRequest(id),
                (message, state) => ClientRemoveDistributedObjectListenerCodec.DecodeResponse(message).Response,
                (message, state) => ClientAddDistributedObjectListenerCodec.HandleEventAsync(message, HandleCodecEvent, null, LoggerFactory));
        }

        internal Func<DistributedObjectCreatedEventArgs, ValueTask> ObjectCreated { get; set; }

        internal Func<DistributedObjectDestroyedEventArgs, ValueTask> ObjectDestroyed { get; set; }

        private ValueTask HandleCodecEvent(string name, string serviceName, string eventTypeName, Guid memberId, object state)
        {
            return eventTypeName switch
            {
                "CREATED" => ObjectCreated == null
                    ? default
                    : ObjectCreated.AwaitEach(new DistributedObjectCreatedEventArgs(serviceName, name, memberId)),

                "DESTROYED" => ObjectDestroyed == null
                    ? default
                    : ObjectDestroyed.AwaitEach(new DistributedObjectDestroyedEventArgs(serviceName, name, memberId)),

                _ => throw new NotSupportedException($"Event type \"{eventTypeName}\" is not supported.")
            };
        }
    }
}
