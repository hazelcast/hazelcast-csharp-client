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

using System.Collections.Generic;
using Hazelcast.Core;
using Hazelcast.IO;

#pragma warning disable CS1591
 namespace Hazelcast.Client.Spi
{
    public interface IClientClusterService
    {
        /// <param name="listener">The listener to be registered.</param>
        /// <returns>The registration id.</returns>
        string AddMembershipListener(IMembershipListener listener);

        /// <summary>Returns the cluster-time.</summary>
        /// <remarks>
        /// Returns the cluster-time.
        /// <p/>
        /// </remarks>
        /// <returns>The cluster-time.</returns>
        long GetClusterTime();

        /// <returns>The client interface representing the local client.</returns>
        IClient GetLocalClient();

        /// <summary>Returns the address of the master member.</summary>
        /// <returns>The address of the master member. Could be null if the master is not yet known.</returns>
        Address GetMasterAddress();

        /// <summary>Gets the member for the given address.</summary>
        /// <param name="address">The address of the member to look up.</param>
        /// <returns>The member that was found, or null if not found. If address is null, null is returned.</returns>
        IMember GetMember(Address address);

        /// <summary>Gets the member with the given uuid.</summary>
        /// <param name="uuid">The uuid of the member.</param>
        /// <returns>The member that was found, or null if not found. If uuid is null, null is returned.</returns>
        IMember GetMember(string uuid);

        /// <summary>Gets the collection of members.</summary>
        /// <returns>The collection of members. Null will never be returned.</returns>
        ICollection<IMember> GetMemberList();

        /// <summary>The owner connection is opened to owner member of the client in the cluster.</summary>
        /// <remarks>
        /// The owner connection is opened to owner member of the client in the cluster.
        /// If the owner member dies, other members of the cluster assumes this client is dead.
        /// </remarks>
        /// <returns>The address of the owner connection.</returns>
        Address GetOwnerConnectionAddress();

        /// <summary>Gets the current number of members.</summary>
        /// <returns>The current number of members.</returns>
        int GetSize();

        /// <param name="registrationId">The registrationId of the listener to be removed.</param>
        /// <returns>true if successfully removed, false otherwise.</returns>
        bool RemoveMembershipListener(string registrationId);
    }
}