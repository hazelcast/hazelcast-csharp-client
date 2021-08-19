﻿// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading;

namespace Hazelcast.Clustering.LoadBalancing
{
    /// <summary>
    /// Represents a round-robin load balancer.
    /// </summary>
    /// <remarks>
    /// <para>A round-robin load balancer returns members one after another.</para>
    /// </remarks>
    public class RoundRobinLoadBalancer : LoadBalancerBase
    {
        // TODO: consider separating dataMember vs allMember indexes?
        private int _index;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoundRobinLoadBalancer"/> class.
        /// </summary>
        public RoundRobinLoadBalancer()
        { }

        /// <inheritdoc />
        public override Guid GetMember(bool onlyDataMember = false)
        {
            var members = Members;
            if (members == null) return default;

            var count = onlyDataMember ? members.DataMembers.Count : members.Count;
            if (count == 0) return default;

            var index = Interlocked.Increment(ref _index) % count;
            if (index < 0) index += count; // in case of integer overflow

            return index < members.DataMembers.Count
                ? members.DataMembers[index].Id
                : members.LiteMembers[index - members.DataMembers.Count].Id;
        }
    }
}
