// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Models;

namespace Hazelcast.Events
{
    /// <summary>
    /// Represents event data for the members updated event.
    /// </summary>
    public class MembersUpdatedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MembersUpdatedEventArgs"/> class.
        /// </summary>
        /// <param name="addedMembers">Added members.</param>
        /// <param name="removedMembers">Removed members.</param>
        /// <param name="members">All members.</param>
        internal MembersUpdatedEventArgs(IReadOnlyCollection<MemberInfo> addedMembers, IReadOnlyCollection<MemberInfo> removedMembers, IReadOnlyCollection<MemberInfo> members)
        {
            AddedMembers = addedMembers;
            RemovedMembers = removedMembers;
            Members = members;
        }

        /// <summary>
        /// Gets the added members.
        /// </summary>
        public IReadOnlyCollection<MemberInfo> AddedMembers { get; }

        /// <summary>
        /// Gets the removed members.
        /// </summary>
        public IReadOnlyCollection<MemberInfo> RemovedMembers { get; }

        /// <summary>
        /// Gets the complete, updated list of members.
        /// </summary>
        public IReadOnlyCollection<MemberInfo> Members { get; }
    }
}
