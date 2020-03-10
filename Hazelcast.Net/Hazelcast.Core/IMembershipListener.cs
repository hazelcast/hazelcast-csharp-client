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

namespace Hazelcast.Core
{
    /// <summary>ICluster membership listener.</summary>
    /// <remarks>
    ///     ICluster membership listener.
    ///     The IMembershipListener will never be called concurrently and all MembershipListeners will receive the events
    ///     in the same order.
    /// </remarks>
    /// <seealso cref="IInitialMembershipListener">IInitialMembershipListener</seealso>
    /// <seealso cref="ICluster.AddMembershipListener(IMembershipListener)">ICluster.AddMembershipListener(IMembershipListener)</seealso>
    public interface IMembershipListener : IEventListener
    {
        /// <summary>Invoked when a new member is added to the cluster.</summary>
        /// <remarks>Invoked when a new member is added to the cluster.</remarks>
        /// <param name="membershipEvent">membership event</param>
        void MemberAdded(MembershipEvent membershipEvent);

        /// <summary>Invoked when an existing member leaves the cluster.</summary>
        /// <remarks>Invoked when an existing member leaves the cluster.</remarks>
        /// <param name="membershipEvent">membership event</param>
        void MemberRemoved(MembershipEvent membershipEvent);
    }

    public interface IEventListener
    {
    }
}