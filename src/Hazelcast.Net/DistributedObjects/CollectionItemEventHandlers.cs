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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Data;

namespace Hazelcast.DistributedObjects
{
    /// <summary>
    /// Represent collection item event handlers.
    /// </summary>
    /// <typeparam name="T">The collection item type.</typeparam>
    public sealed class CollectionItemEventHandlers<T> : EventHandlersBase<ICollectionItemEventHandler<T>>
    {
        /// <summary>
        /// Adds an handler which runs when an item is added.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public CollectionItemEventHandlers<T> ItemAdded(Func<IHCollection<T>, CollectionItemEventArgs<T>, CancellationToken, ValueTask> handler)
        {
            Add(new CollectionItemEventHandler<T>(CollectionItemEventTypes.Added, handler));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when an item is removed.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public CollectionItemEventHandlers<T> ItemRemoved(Func<IHCollection<T>, CollectionItemEventArgs<T>, CancellationToken, ValueTask> handler)
        {
            Add(new CollectionItemEventHandler<T>(CollectionItemEventTypes.Removed, handler));
            return this;
        }
    }
}
