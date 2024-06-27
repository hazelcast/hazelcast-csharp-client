// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
using System.Linq;
namespace Hazelcast.Models
{
    internal class MemberGroups
    {

        public MemberGroups(IList<IList<Guid>> groups, int version, Guid clusterId, Guid memberReceivedFrom)
        {
            Groups = groups ?? Enumerable.Empty<IList<Guid>>().ToList();
            Version = version;
            ClusterId = clusterId;
            MemberReceivedFrom = memberReceivedFrom;
            SelectedGroup = GetGroupOf(MemberReceivedFrom);
        }
        public IList<IList<Guid>> Groups { get; }
        public int Version { get; }
        public Guid ClusterId { get; }
        public Guid MemberReceivedFrom { get; }

        /// Group of the member that group information received from.
        public IReadOnlyList<Guid> SelectedGroup { get; }

        // internal for testing
        public IReadOnlyList<Guid> GetGroupOf(Guid memberId)
        {
            // Find given member's group.
            return ((Groups.FirstOrDefault(group => group.Contains(memberId))) ?? Enumerable.Empty<Guid>()).ToList();
        }

    }
}
