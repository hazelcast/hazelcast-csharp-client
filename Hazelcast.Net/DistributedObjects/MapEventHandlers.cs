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
using Hazelcast.Core;

namespace Hazelcast.DistributedObjects
{
    /// <summary>
    /// Represents map event handlers.
    /// </summary>
    public sealed class MapEventHandlers<TKey, TValue> : EventHandlersBase<IMapEventHandlerBase>
    {
        /// <summary>
        /// Adds an handler which runs when the map is cleared.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public MapEventHandlers<TKey, TValue> Cleared(Action<IHMap<TKey, TValue>, MapClearedEventArgs> handler)
        {
            Add(new MapClearedEventHandler<TKey, TValue>(handler));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when the map is evicted.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public MapEventHandlers<TKey, TValue> Evicted(Action<IHMap<TKey, TValue>, MapEvictedEventArgs> handler)
        {
            Add(new MapEvictedEventHandler<TKey, TValue>(handler));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is updated.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public MapEventHandlers<TKey, TValue> EntryUpdated(Action<IHMap<TKey, TValue>, MapEntryUpdatedEventArgs<TKey, TValue>> handler)
        {
            Add(new MapEntryUpdatedEventHandler<TKey, TValue>(handler));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is removed.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public MapEventHandlers<TKey, TValue> EntryRemoved(Action<IHMap<TKey, TValue>, MapEntryRemovedEventArgs<TKey, TValue>> handler)
        {
            Add(new MapEntryRemovedEventHandler<TKey, TValue>(handler));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is added.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public MapEventHandlers<TKey, TValue> EntryAdded(Action<IHMap<TKey, TValue>, MapEntryAddedEventArgs<TKey, TValue>> handler)
        {
            Add(new MapEntryAddedEventHandler<TKey, TValue>(handler));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is evicted.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public MapEventHandlers<TKey, TValue> EntryEvicted(Action<IHMap<TKey, TValue>, MapEntryEvictedEventArgs<TKey, TValue>> handler)
        {
            Add(new MapEntryEvictedEventHandler<TKey, TValue>(handler));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is expired.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public MapEventHandlers<TKey, TValue> EntryExpired(Action<IHMap<TKey, TValue>, MapEntryExpiredEventArgs<TKey, TValue>> handler)
        {
            Add(new MapEntryExpiredEventHandler<TKey, TValue>(handler));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is invalidated.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public MapEventHandlers<TKey, TValue> EntryInvalidated(Action<IHMap<TKey, TValue>, MapEntryInvalidatedEventArgs<TKey, TValue>> handler)
        {
            Add(new MapEntryInvalidatedEventHandler<TKey, TValue>(handler));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is loaded.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public MapEventHandlers<TKey, TValue> EntryLoaded(Action<IHMap<TKey, TValue>, MapEntryLoadedEventArgs<TKey, TValue>> handler)
        {
            Add(new MapEntryLoadedEventHandler<TKey, TValue>(handler));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is merged.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public MapEventHandlers<TKey, TValue> EntryMerged(Action<IHMap<TKey, TValue>, MapEntryMergedEventArgs<TKey, TValue>> handler)
        {
            Add(new MapEntryMergedEventHandler<TKey, TValue>(handler));
            return this;
        }
    }
}
