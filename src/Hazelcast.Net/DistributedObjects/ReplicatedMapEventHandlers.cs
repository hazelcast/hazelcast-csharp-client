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
using Hazelcast.Core;

namespace Hazelcast.DistributedObjects
{
    /// <summary>
    /// Represents replicated distributed dictionary event handlers.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    public class ReplicatedMapEventHandlers<TKey, TValue> : EventHandlersBase<IMapEventHandlerBase>
    {
        /// <summary>
        /// Adds an handler which runs when the map is cleared.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public ReplicatedMapEventHandlers<TKey, TValue> Cleared(Action<IHReplicatedMap<TKey, TValue>, MapClearedEventArgs> handler)
        {
            Add(new MapClearedEventHandler<TKey, TValue, IHReplicatedMap<TKey, TValue>>(handler.AsAsync()));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when the map is cleared.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public ReplicatedMapEventHandlers<TKey, TValue> Cleared(Func<IHReplicatedMap<TKey, TValue>, MapClearedEventArgs, ValueTask> handler)
        {
            Add(new MapClearedEventHandler<TKey, TValue, IHReplicatedMap<TKey, TValue>>(handler));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is updated.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public ReplicatedMapEventHandlers<TKey, TValue> EntryUpdated(Action<IHReplicatedMap<TKey, TValue>, MapEntryUpdatedEventArgs<TKey, TValue>> handler)
        {
            Add(new MapEntryUpdatedEventHandler<TKey, TValue, IHReplicatedMap<TKey, TValue>>(handler.AsAsync()));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is updated.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public ReplicatedMapEventHandlers<TKey, TValue> EntryUpdated(Func<IHReplicatedMap<TKey, TValue>, MapEntryUpdatedEventArgs<TKey, TValue>, ValueTask> handler)
        {
            Add(new MapEntryUpdatedEventHandler<TKey, TValue, IHReplicatedMap<TKey, TValue>>(handler));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is removed.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public ReplicatedMapEventHandlers<TKey, TValue> EntryRemoved(Action<IHReplicatedMap<TKey, TValue>, MapEntryRemovedEventArgs<TKey, TValue>> handler)
        {
            Add(new MapEntryRemovedEventHandler<TKey, TValue, IHReplicatedMap<TKey, TValue>>(handler.AsAsync()));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is removed.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public ReplicatedMapEventHandlers<TKey, TValue> EntryRemoved(Func<IHReplicatedMap<TKey, TValue>, MapEntryRemovedEventArgs<TKey, TValue>, ValueTask> handler)
        {
            Add(new MapEntryRemovedEventHandler<TKey, TValue, IHReplicatedMap<TKey, TValue>>(handler));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is added.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public ReplicatedMapEventHandlers<TKey, TValue> EntryAdded(Action<IHReplicatedMap<TKey, TValue>, MapEntryAddedEventArgs<TKey, TValue>> handler)
        {
            Add(new MapEntryAddedEventHandler<TKey, TValue, IHReplicatedMap<TKey, TValue>>(handler.AsAsync()));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is added.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public ReplicatedMapEventHandlers<TKey, TValue> EntryAdded(Func<IHReplicatedMap<TKey, TValue>, MapEntryAddedEventArgs<TKey, TValue>, ValueTask> handler)
        {
            Add(new MapEntryAddedEventHandler<TKey, TValue, IHReplicatedMap<TKey, TValue>>(handler));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is merged.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public ReplicatedMapEventHandlers<TKey, TValue> EntryMerged(Action<IHReplicatedMap<TKey, TValue>, MapEntryMergedEventArgs<TKey, TValue>> handler)
        {
            Add(new MapEntryMergedEventHandler<TKey, TValue, IHReplicatedMap<TKey, TValue>>(handler.AsAsync()));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is merged.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public ReplicatedMapEventHandlers<TKey, TValue> EntryMerged(Func<IHReplicatedMap<TKey, TValue>, MapEntryMergedEventArgs<TKey, TValue>, ValueTask> handler)
        {
            Add(new MapEntryMergedEventHandler<TKey, TValue, IHReplicatedMap<TKey, TValue>>(handler));
            return this;
        }
    }
}
