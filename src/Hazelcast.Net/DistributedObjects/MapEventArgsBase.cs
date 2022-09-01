﻿// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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

using Hazelcast.Models;

namespace Hazelcast.DistributedObjects
{
    /// <summary>
    /// Represents event data for map events.
    /// </summary>
    public abstract class MapEventArgsBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MapEventArgsBase"/> class.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="numberOfAffectedEntries">The number of affected entries.</param>
        /// <param name="state">A state object.</param>
        protected MapEventArgsBase(MemberInfo member, int numberOfAffectedEntries, object state)
        {
            Member = member;
            NumberOfAffectedEntries = numberOfAffectedEntries;
            State = state;
        }

        /// <summary>
        /// Gets the member that originated the event.
        /// </summary>
        public MemberInfo Member { get; }

        /// <summary>
        /// Gets the number of affected entries.
        /// </summary>
        public int NumberOfAffectedEntries { get; }

        /// <summary>
        /// Gets the state object.
        /// </summary>
        public object State { get; }
    }
}
