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

namespace Hazelcast.Core
{
    /// <summary>
    ///     Membership event fired when a new member is added
    ///     to the cluster and/or when a member leaves the cluster.
    /// </summary>
    /// <remarks>
    ///     Membership event fired when a new member is added
    ///     to the cluster and/or when a member leaves the cluster.
    /// </remarks>
    /// <seealso cref="IMembershipListener">IMembershipListener</seealso>
    [Serializable]
    public class MembershipEvent : EventObject
    {
        public const int MemberAdded = 1;
        public const int MemberRemoved = 2;

        private readonly int _eventType;
        private readonly IMember _member;

        private readonly ICollection<IMember> _members;

        public MembershipEvent(ICluster cluster, IMember member, int eventType, ICollection<IMember> members)
            : base(cluster)
        {
            _member = member;
            _eventType = eventType;
            _members = members;
        }

        /// <summary>Returns the cluster of the event.</summary>
        /// <remarks>Returns the cluster of the event.</remarks>
        /// <returns></returns>
        public virtual ICluster GetCluster()
        {
            return (ICluster) GetSource();
        }

        /// <summary>Returns the membership event type; #MEMBER_ADDED or #MEMBER_REMOVED</summary>
        /// <returns>the membership event type</returns>
        public virtual int GetEventType()
        {
            return _eventType;
        }

        /// <summary>Returns the removed or added member.</summary>
        /// <remarks>Returns the removed or added member.</remarks>
        /// <returns>member which is removed/added</returns>
        public virtual IMember GetMember()
        {
            return _member;
        }

        /// <summary>Returns a consistent view of the the members exactly after this MembershipEvent has been processed.</summary>
        /// <remarks>
        ///     Returns a consistent view of the the members exactly after this MembershipEvent has been processed. So if a
        ///     member is removed, the returned set will not include this member. And if a member is added it will include
        ///     this member.
        ///     The problem with calling the
        ///     <see cref="ICluster.GetMembers()">ICluster.GetMembers()</see>
        ///     is that the content could already
        ///     have changed while processing this event so it becomes very difficult to write a deterministic algorithm since
        ///     you can't get a deterministic view of the members. This method solves that problem.
        ///     The set is immutable and ordered. For more information see
        ///     <see cref="ICluster.GetMembers()">ICluster.GetMembers()</see>
        ///     .
        /// </remarks>
        /// <returns>the members at the moment after this event.</returns>
        public virtual ICollection<IMember> GetMembers()
        {
            return _members;
        }

        public override string ToString()
        {
            return "MembershipEvent {" + _member + "} " + ((_eventType == MemberAdded) ? "added" : "removed");
        }
    }
}