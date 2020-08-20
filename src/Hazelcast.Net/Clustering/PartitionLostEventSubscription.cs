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
using Hazelcast.Events;
using Hazelcast.Protocol.Codecs;

namespace Hazelcast.Clustering
{
    internal class PartitionLostEventSubscription : EventSubscriptionBase
    {
        private readonly ClusterMembers _clusterMembers;
        private readonly bool _isSmart;

        public PartitionLostEventSubscription(ClusterState clusterState, ClusterEvents clusterEvents, ClusterMembers clusterMembers)
            : base(clusterState, clusterEvents)
        {
            _clusterMembers = clusterMembers;
            _isSmart = clusterState.Options.Networking.SmartRouting;
        }

        protected override ClusterSubscription CreateSubscription()
        {
            return new ClusterSubscription(
                ClientAddPartitionLostListenerCodec.EncodeRequest(_isSmart),
                (message, state) => ClientAddPartitionLostListenerCodec.DecodeResponse(message).Response,
                (id, state) => ClientRemovePartitionLostListenerCodec.EncodeRequest(id),
                (message, state) => ClientRemovePartitionLostListenerCodec.DecodeResponse(message).Response,
                (message, state) => ClientAddPartitionLostListenerCodec.HandleEventAsync(message, HandleInternal, LoggerFactory));
        }

        internal Func<PartitionLostEventArgs, ValueTask> Handle { get; set; }

        private ValueTask HandleInternal(int partitionId, int lostBackupCount, Guid memberId)
        {
            if (Handle == null) return default;

            // TODO: document + avoid hard-coded constants
            const int maxLostBackupCount = 6;

            var member = _clusterMembers.GetMember(memberId);

            return Handle(new PartitionLostEventArgs(partitionId, lostBackupCount, lostBackupCount == maxLostBackupCount, member));
        }
    }
}
