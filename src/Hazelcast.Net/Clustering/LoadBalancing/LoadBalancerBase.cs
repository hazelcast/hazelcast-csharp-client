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

using System;
using System.Collections.Generic;
using Hazelcast.Models;

namespace Hazelcast.Clustering.LoadBalancing
{
    /// <summary>
    /// Provides a base class for load balancers.
    /// </summary>
    public abstract class LoadBalancerBase : ILoadBalancer
    {
        /// <summary>
        /// Gets the members.
        /// </summary>
#pragma warning disable CA1002 // Do not expose generic lists
        protected MembersInfoCollection Members { get; private set; }
#pragma warning restore CA1002

        /// <inheritdoc />
        public virtual int Count => Members?.Count ?? 0;

        /// <inheritdoc />
        public abstract Guid GetMember(bool onlyDataMember = false);

        /// <inheritdoc />
        public virtual void SetMembers(IEnumerable<MemberInfo> members)
        {
            if (members == null)
                throw new ArgumentNullException(nameof(members));

            Members = new MembersInfoCollection(members); // atomic reference
        }
    }
}
