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

using System;

namespace Hazelcast.Clustering.LoadBalancing
{
    /// <summary>
    /// Represents a random load balancer.
    /// </summary>
    /// <remarks>
    /// <para>A random load balancer returns random members.</para>
    /// </remarks>
    public class RandomLoadBalancer : LoadBalancerBase
    {
        private readonly Random _random = new Random(DateTime.Now.Millisecond);

        /// <inheritdoc />
        public override Guid Select()
        {
            // get an immutable list of members, and pick one at random
            var members = Members;
            return members[_random.Next(members.Count)];
        }
    }
}