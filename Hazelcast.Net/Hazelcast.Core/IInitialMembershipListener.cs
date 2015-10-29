/*
* Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

namespace Hazelcast.Core
{
    /// <summary>
    ///     The InitializingMembershipListener is a
    ///     <see cref="IMembershipListener">IMembershipListener</see>
    ///     that will first receives a
    ///     <see cref="InitialMembershipEvent">InitialMembershipEvent</see>
    ///     when it is registered so it immediately knows which members are available. After
    ///     that event has been received, it will receive the normal MembershipEvents.
    ///     When the InitializingMembershipListener already is registered on a
    ///     <see cref="ICluster">ICluster</see>
    ///     and is registered again on the same
    ///     ICluster instance, it will not receive an additional MembershipInitializeEvent. So this is a once only event.
    /// </summary>
    /// <seealso cref="ICluster.AddMembershipListener(IMembershipListener)">ICluster.AddMembershipListener(IMembershipListener)</seealso>
    /// <seealso cref="MembershipEvent.GetMembers()">MembershipEvent.GetMembers()</seealso>
    public interface IInitialMembershipListener : IMembershipListener
    {
        /// <summary>Is called when this listener is registered.</summary>
        /// <remarks>Is called when this listener is registered.</remarks>
        /// <param name="membershipEvent">the MembershipInitializeEvent</param>
        void Init(InitialMembershipEvent membershipEvent);
    }
}