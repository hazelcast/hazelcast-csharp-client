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

using System;
using System.Collections.Generic;

namespace Hazelcast.Core
{
    /// <summary>
    ///     A event that is send when a
    ///     <see cref="IInitialMembershipListener">IInitialMembershipListener</see>
    ///     registers itself on a
    ///     <see cref="ICluster">ICluster</see>
    ///     . For more
    ///     information see the
    ///     <see cref="IInitialMembershipListener">IInitialMembershipListener</see>
    ///     .
    /// </summary>
    /// <seealso cref="IInitialMembershipListener">IInitialMembershipListener</seealso>
    /// <seealso cref="IMembershipListener">IMembershipListener</seealso>
    /// <seealso cref="MembershipEvent">MembershipEvent</seealso>
    [Serializable]
    public class InitialMembershipEvent : EventObject
    {
        private readonly ICollection<IMember> members;

        public InitialMembershipEvent(ICluster cluster, ICollection<IMember> members) : base(cluster)
        {
            this.members = members;
        }

        /// <summary>
        ///     Returns an immutable set of ordered members at the moment this
        ///     <see cref="IInitialMembershipListener">IInitialMembershipListener</see>
        ///     is
        ///     registered. See
        ///     <see cref="ICluster.GetMembers()">ICluster.GetMembers()</see>
        ///     for more information.
        /// </summary>
        /// <returns>a set of members.</returns>
        public virtual ICollection<IMember> GetMembers()
        {
            return members;
        }

        /// <summary>Returns the cluster of the event.</summary>
        /// <remarks>Returns the cluster of the event.</remarks>
        /// <returns>the cluster of the event.</returns>
        public virtual ICluster GetCluster()
        {
            return (ICluster) GetSource();
        }

        public override string ToString()
        {
            return "MembershipInitializeEvent {" + members + "}";
        }
    }
}