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
using System.Threading.Tasks;
using Hazelcast.Models;

namespace Hazelcast.DistributedObjects
{
    /// <summary>
    /// Specifies a collection item event handler.
    /// </summary>
    /// <typeparam name="T">The collection items type.</typeparam>
    public interface ICollectionItemEventHandler<T>
    {
        /// <summary>
        /// Gets the handled event type.
        /// </summary>
        CollectionItemEventTypes EventType { get; }

        /// <summary>
        /// Handles an event.
        /// </summary>
        /// <param name="sender">The <see cref="IHCollection{T}"/> that triggered the event.</param>
        /// <param name="member">The member.</param>
        /// <param name="item">The item.</param>
        /// <param name="state">A state object.</param>
        ValueTask HandleAsync(IHCollection<T> sender, MemberInfo member, Lazy<T> item, object state);
    }
}
