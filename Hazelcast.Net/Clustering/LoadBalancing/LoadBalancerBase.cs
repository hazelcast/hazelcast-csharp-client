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
using System.Collections.Generic;

namespace Hazelcast.Clustering.LoadBalancing
{
    /// <summary>
    /// Provides a base class for load balancers.
    /// </summary>
    public abstract class LoadBalancerBase : ILoadBalancer
    {
        // NOTE
        // we use a HashSet to keep a de-duplicated set of member identifiers,
        // and provide an immutable list of member identifiers to classes
        // inheriting from this base class. this allows for fast reads, but
        // slowish writes if the list grows. we may want to use different data
        // structures if performance became a problem here

        private readonly HashSet<Guid> _memberIds = new HashSet<Guid>();

        /// <summary>
        /// Gets the members.
        /// </summary>
        protected List<Guid> Members {get; private set; }

        /// <inheritdoc />
        public int Count => Members.Count;

        /// <inheritdoc />
        public abstract Guid Select();

        /// <inheritdoc />
        public void NotifyMembers(IEnumerable<Guid> memberIds)
        {
            lock (_memberIds)
            {
                _memberIds.Clear();
                foreach (var memberId in memberIds)
                    _memberIds.Add(memberId);
                Members = new List<Guid>(_memberIds);
            }
        }
    }
}
