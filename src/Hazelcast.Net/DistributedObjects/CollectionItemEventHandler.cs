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
using System.Threading.Tasks;
using Hazelcast.Data;

namespace Hazelcast.DistributedObjects
{
    /// <summary>
    /// Represents a handler for the <see cref="CollectionItemEventTypes.Message"/> event.
    /// </summary>
    /// <typeparam name="T">The topic object type.</typeparam>
    internal class CollectionItemEventHandler<T> : ICollectionItemEventHandler<T>
    {
        private readonly Func<IHCollection<T>, CollectionItemEventArgs<T>, ValueTask> _handler;

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionItemEventHandler{T}"/> class.
        /// </summary>
        /// <param name="eventType">The event type to handle.</param>
        /// <param name="handler">An action to execute</param>
        public CollectionItemEventHandler(CollectionItemEventTypes eventType, Func<IHCollection<T>, CollectionItemEventArgs<T>, ValueTask> handler)
        {
            EventType = eventType;
            _handler = handler;
        }

        /// <inheritdoc />
        public CollectionItemEventTypes EventType { get; }

        /// <inheritdoc />
        public ValueTask HandleAsync(IHCollection<T> sender, MemberInfo member, Lazy<T> item, object state)
            => _handler(sender, CreateEventArgs(member, item, state));

        /// <summary>
        /// Creates event arguments.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="item">The item.</param>
        /// <param name="state">A state object.</param>
        /// <returns>Event arguments.</returns>
        private static CollectionItemEventArgs<T> CreateEventArgs(MemberInfo member, Lazy<T> item, object state)
            => new CollectionItemEventArgs<T>(member, item, state);
    }
}
