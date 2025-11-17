// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Models;

namespace Hazelcast.Events
{
    /// <summary>
    /// Represents event data for the partition lost event.
    /// </summary>
    public class PartitionLostEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PartitionLostEventArgs"/> class.
        /// </summary>
        /// <param name="partitionId"></param>
        /// <param name="lostBackupCount"></param>
        /// <param name="isAllReplicasInPartitionLost"></param>
        /// <param name="member"></param>
        public PartitionLostEventArgs(int partitionId, int lostBackupCount, bool isAllReplicasInPartitionLost, MemberInfo member)
        {
            PartitionId = partitionId;
            LostBackupCount = lostBackupCount;
            IsAllReplicasInPartitionLost = isAllReplicasInPartitionLost;
            Member = member;
        }

        /// <summary>
        /// The partition id.
        /// </summary>
        public int PartitionId { get; }

        /// <summary>
        /// The lost backup count.
        /// </summary>
        public int LostBackupCount { get; }

        /// <summary>
        /// Indicates whether all replicas of the partition are lost.
        /// </summary>
        public bool IsAllReplicasInPartitionLost { get; }

        /// <summary>
        /// The member raised the event.
        /// </summary>
        public MemberInfo Member { get; }
    }
}
