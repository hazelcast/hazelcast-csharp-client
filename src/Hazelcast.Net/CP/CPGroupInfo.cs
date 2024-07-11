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
namespace Hazelcast.CP
{
    internal class CPGroupInfo
    {
        public CPGroupInfo(CPGroupId groupId, CPMember leader, ICollection<CPMember> followers)
        {
            GroupId = groupId;
            Leader = leader;
            Followers = followers;
        }

        public CPGroupId GroupId { get; }

        public CPMember Leader { get; }

        public ICollection<CPMember> Followers { get; }

        public override string ToString()
        {
            return $"CPGroupInfo{{groupId={GroupId}, leader={Leader}, followers={string.Join(", ", Followers)}}}";
        }

        public override bool Equals(object obj)
        {
            if (obj is not CPGroupInfo other) return false;

            return GroupId == other.GroupId && Leader.Equals(other.Leader) && Followers.Equals(other.Followers);
        }

        public override int GetHashCode()
        {
            var arrayOf = new object[] { GroupId, Leader, Followers };
            return arrayOf.GetHashCode();
        }

    }
}
