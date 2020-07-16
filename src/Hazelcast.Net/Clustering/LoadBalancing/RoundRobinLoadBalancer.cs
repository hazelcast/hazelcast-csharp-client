﻿// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
        private int _index;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoundRobinLoadBalancer"/> class.
        /// </summary>
        public RoundRobinLoadBalancer()
        { }

        /// <inheritdoc />
        public override Guid GetMember()
        {
            // get an immutable list of members, and pick "the next one"
            var members = Members;
            if ((members?.Count ?? 0) == 0) throw new InvalidOperationException("The load balancer does not have members.");
            var index = Interlocked.Increment(ref _index);
            return members[index % members.Count];
        }
    }
}
