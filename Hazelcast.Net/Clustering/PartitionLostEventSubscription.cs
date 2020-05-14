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
    internal class PartitionLostEventSubscription : EventSubscriptionBase
    {
        private readonly bool _isSmart;

        public PartitionLostEventSubscription(Cluster cluster, ILoggerFactory loggerFactory, bool isSmart)
            : base(cluster, loggerFactory)
        {
            _isSmart = isSmart;
        }

        protected override ClusterSubscription CreateSubscription()
        {
            return new ClusterSubscription(
                ClientAddPartitionLostListenerCodec.EncodeRequest(_isSmart),
                (message, state) => ClientAddPartitionLostListenerCodec.DecodeResponse(message).Response,
                (id, state) => ClientRemovePartitionLostListenerCodec.EncodeRequest(id),
                (message, state) => ClientAddPartitionLostListenerCodec.HandleEvent(message, HandleInternal, LoggerFactory));
        }

        internal Action<PartitionLostEventArgs> Handle { get; set; }

        private void HandleInternal(int partitionId, int lostBackupCount, Guid memberId)
        {
            const int maxLostBackupCount = 6; // TODO: document

            var member = Cluster.GetMember(memberId);

            Handle(new PartitionLostEventArgs(partitionId, lostBackupCount, lostBackupCount == maxLostBackupCount, member));
        }
    }
}