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
using Hazelcast.Exceptions;

namespace Hazelcast.Clustering.LoadBalancing
{
    /// <summary>
    /// Represents a static load balancer.
    /// </summary>
    /// <remarks>
    /// <para>A static load balancer knows only one member and always return that member.</para>
    /// </remarks>
    public class StaticLoadBalancer : LoadBalancerBase
    {
        private readonly Guid _memberId;

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomLoadBalancer"/> class.
        /// </summary>
        /// <param name="options">Options.</param>
        public StaticLoadBalancer(LoadBalancingOptions options)
        {
            if (!options.LoadBalancerArgs.TryGetValue("memberId", out var memberIdArg) ||
                !Guid.TryParse(memberIdArg.ToString(), out var memberId))
                throw new ConfigurationException($"Value '{memberIdArg}' is not a valid Guid member identifier.");

            _memberId = memberId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticLoadBalancer"/> class.
        /// </summary>
        /// <param name="memberId"></param>
        public StaticLoadBalancer(Guid memberId)
        {
            _memberId = memberId;
        }

        /// <inheritdoc />
        public override Guid Select()
            => _memberId;
    }
}
