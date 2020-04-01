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

namespace Hazelcast.Core
{
    /// <summary>Hazelcast cluster interface.</summary>
    /// <remarks>Hazelcast cluster interface.</remarks>
    public interface ICluster
    {
        /// <summary>Adds IMembershipListener to listen for membership updates.</summary>
        /// <remarks>
        ///     Adds IMembershipListener to listen for membership updates.
        ///     If the IMembershipListener implements the
        ///     <see cref="IInitialMembershipListener">IInitialMembershipListener</see>
        ///     interface, it will also receive
        ///     the
        ///     <see cref="InitialMembershipEvent">InitialMembershipEvent</see>
        ///     .
        /// </remarks>
        /// <param name="listener">membership listener</param>
        /// <returns>returns registration id.</returns>
        string AddMembershipListener(IMembershipListener listener);

        /// <summary>Returns the cluster-wide time in milliseconds.</summary>
        /// <remarks>
        ///     Returns the cluster-wide time in milliseconds.
        ///     <p />
        ///     ICluster tries to keep a cluster-wide time which is
        ///     might be different than the member's own system time.
        ///     ICluster-wide time is -almost- the same on all members
        ///     of the cluster.
        /// </remarks>
        /// <returns>cluster-wide time</returns>
        long GetClusterTime();

        /// <summary>Returns this Hazelcast instance member</summary>
        /// <returns>this Hazelcast instance member</returns>
        IMember GetLocalMember();

        /// <summary>List of current members of the cluster.</summary>
        /// <remarks>
        ///     List of current members of the cluster.
        ///     Returning set instance is not modifiable.
        ///     Every member in the cluster has the same member list in the same
        ///     order. First member is the oldest member.
        /// </remarks>
        /// <returns>current members of the cluster</returns>
        IList<IMember> GetMembers();

        /// <summary>Removes the specified membership listener.</summary>
        /// <remarks>Removes the specified membership listener.</remarks>
        /// <param name="registrationId">Id of listener registration.</param>
        /// <returns>true if registration is removed, false otherwise</returns>
        bool RemoveMembershipListener(string registrationId);
    }
}