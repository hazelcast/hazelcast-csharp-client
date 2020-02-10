// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.IO;

namespace Hazelcast.Core
{
    public interface IPartitionService
    {
        /// <summary>
        ///  Returns a set containing all the {@link Partition}s in the cluster.
        /// </summary>
        /// <returns>all partitions in the cluster</returns>
        ISet<IPartition> GetPartitions();

        /// <summary>
        /// Returns the partition that the given key belongs to.
        /// </summary>
        /// <param name="key">the given key</param>
        /// <returns>the partition that the given key belongs to</returns>
        IPartition GetPartition(object key);

        /// <summary>
        /// Adds a PartitionLostListener.
        /// </summary>
        /// <remarks>
        /// The addPartitionLostListener returns a registration ID. This ID is needed to remove the PartitionLostListener using the
        /// <see cref="RemovePartitionLostListener"/> method.
        /// </remarks>
        /// <param name="partitionLostListener">the added PartitionLostListener</param>
        /// <returns>the registration ID for the PartitionLostListener</returns>
        Guid AddPartitionLostListener(IPartitionLostListener partitionLostListener);

        /// <summary>
        /// Removes a PartitionLostListener.
        /// </summary>
        /// <remarks>
        /// If the same PartitionLostListener is registered multiple times, it needs to be removed multiple times.<p/>
        /// This method can safely be called multiple times for the same registration ID; every subsequent call is just ignored.
        /// </remarks>
        /// <param name="registrationId">the registration ID of the listener to remove</param>
        /// <returns><c>true</c> if the listener is removed, <c>false</c> otherwise</returns>
        bool RemovePartitionLostListener(Guid registrationId);
    }

    public interface IPartitionLostListener : IEventListener
    {
        void PartitionLost(IPartitionLostEvent partitionLostEvent);
    }

    public interface IPartitionLostEvent
    {
        /// <summary>
        /// Returns the number of lost backups for the partition. 0: the owner, 1: first backup, 2: second backup...
        /// </summary>
        int LostBackupCount { get; }
        
        /// <summary>
        /// Returns true if all replicas of a partition are lost
        /// </summary>
        bool IsAllReplicasInPartitionLost { get; }
        
        /// <summary>
        /// Returns the address of the node that dispatches the event
        /// </summary>
        Address EventSource { get; }
    }
}