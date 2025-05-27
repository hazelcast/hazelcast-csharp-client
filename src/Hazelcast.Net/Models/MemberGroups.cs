﻿// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Core;
namespace Hazelcast.Models
{
    internal class MemberGroups
    {
        public MemberGroups(IList<IList<Guid>> groups, int version, Guid clusterId, Guid memberReceivedFrom) :
            this(groups.Select(group => new HashSet<Guid>(group)).ToList(), version, clusterId, memberReceivedFrom)
        { }

        public MemberGroups(IList<HashSet<Guid>> groups, int version, Guid clusterId, Guid memberReceivedFrom)
        {
            Groups = groups ?? Enumerable.Empty<HashSet<Guid>>().ToList();
            Version = version;
            ClusterId = clusterId;
            MemberReceivedFrom = memberReceivedFrom;
            SelectedGroup = GetGroupOf(MemberReceivedFrom);
        }
        public IList<HashSet<Guid>> Groups { get; }
        public int Version { get; }
        public Guid ClusterId { get; }
        public Guid MemberReceivedFrom { get; }

        /// Group of the member that group information received from.
        public HashSet<Guid> SelectedGroup { get; }

        // internal for testing
        public HashSet<Guid> GetGroupOf(Guid memberId)
        {
            // Find given member's group.
            return Groups.FirstOrDefault(group => group.Contains(memberId)) ?? new HashSet<Guid>();
        }

        public override string ToString()
        {
            return $"MemberGroups[Version: {Version}," +
                   $" ClusterId: {ClusterId.ToShortString()}," +
                   $" MemberReceivedFrom: {MemberReceivedFrom.ToShortString()}," +
                   $" Groups: {string.Join(",", Groups.Select(group => $"[{string.Join(", ", group.Select(id => id.ToShortString()))}]"))}" +
                   "]";
        }

    }
}
