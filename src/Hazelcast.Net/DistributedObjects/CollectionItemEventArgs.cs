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
using Hazelcast.Models;

namespace Hazelcast.DistributedObjects
{
    /// <summary>
    /// Represents event data for the <see cref="CollectionItemEventTypes"/> events.
    /// </summary>
    /// <typeparam name="T">The topic object type.</typeparam>
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix - here it is correct
    public sealed class CollectionItemEventArgs<T> : EventArgsBase
#pragma warning restore CA1711 
    {
        private readonly Lazy<T> _item;

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionItemEventArgs{T}"/> class.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="item">The item.</param>
        /// <param name="state">A state object.</param>
        public CollectionItemEventArgs(MemberInfo member, Lazy<T> item, object state)
            : base(state)
        {
            Member = member;
            _item = item;
        }

        /// <summary>
        /// Gets the member that fired the event.
        /// </summary>
        public MemberInfo Member { get; }

        /// <summary>
        /// Gets the item.
        /// </summary>
        public T Item => _item.Value;
    }
}
