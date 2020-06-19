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

namespace Hazelcast.DistributedObjects
{
    /// <summary>
    /// Represents multi map event handlers.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    public class MultiMapEventHandlers<TKey, TValue> : EventHandlersBase<IMapEventHandlerBase>
    {
        /// <summary>
        /// Adds an handler which runs when the map is cleared.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public MultiMapEventHandlers<TKey, TValue> Cleared(Action<IHMultiMap<TKey, TValue>, MapClearedEventArgs> handler)
        {
            Add(new MapClearedEventHandler<TKey, TValue, IHMultiMap<TKey, TValue>>(handler.AsAsync()));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when the map is cleared.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public MultiMapEventHandlers<TKey, TValue> Cleared(Func<IHMultiMap<TKey, TValue>, MapClearedEventArgs, CancellationToken, ValueTask> handler)
        {
            Add(new MapClearedEventHandler<TKey, TValue, IHMultiMap<TKey, TValue>>(handler));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is updated.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public MultiMapEventHandlers<TKey, TValue> EntryUpdated(Action<IHMultiMap<TKey, TValue>, MapEntryUpdatedEventArgs<TKey, TValue>> handler)
        {
            Add(new MapEntryUpdatedEventHandler<TKey, TValue, IHMultiMap<TKey, TValue>>(handler.AsAsync()));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is updated.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public MultiMapEventHandlers<TKey, TValue> EntryUpdated(Func<IHMultiMap<TKey, TValue>, MapEntryUpdatedEventArgs<TKey, TValue>, CancellationToken, ValueTask> handler)
        {
            Add(new MapEntryUpdatedEventHandler<TKey, TValue, IHMultiMap<TKey, TValue>>(handler));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is removed.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public MultiMapEventHandlers<TKey, TValue> EntryRemoved(Action<IHMultiMap<TKey, TValue>, MapEntryRemovedEventArgs<TKey, TValue>> handler)
        {
            Add(new MapEntryRemovedEventHandler<TKey, TValue, IHMultiMap<TKey, TValue>>(handler.AsAsync()));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is removed.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public MultiMapEventHandlers<TKey, TValue> EntryRemoved(Func<IHMultiMap<TKey, TValue>, MapEntryRemovedEventArgs<TKey, TValue>, CancellationToken, ValueTask> handler)
        {
            Add(new MapEntryRemovedEventHandler<TKey, TValue, IHMultiMap<TKey, TValue>>(handler));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is added.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public MultiMapEventHandlers<TKey, TValue> EntryAdded(Action<IHMultiMap<TKey, TValue>, MapEntryAddedEventArgs<TKey, TValue>> handler)
        {
            Add(new MapEntryAddedEventHandler<TKey, TValue, IHMultiMap<TKey, TValue>>(handler.AsAsync()));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is added.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public MultiMapEventHandlers<TKey, TValue> EntryAdded(Func<IHMultiMap<TKey, TValue>, MapEntryAddedEventArgs<TKey, TValue>, CancellationToken, ValueTask> handler)
        {
            Add(new MapEntryAddedEventHandler<TKey, TValue, IHMultiMap<TKey, TValue>>(handler));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is merged.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public MultiMapEventHandlers<TKey, TValue> EntryMerged(Action<IHMultiMap<TKey, TValue>, MapEntryMergedEventArgs<TKey, TValue>> handler)
        {
            Add(new MapEntryMergedEventHandler<TKey, TValue, IHMultiMap<TKey, TValue>>(handler.AsAsync()));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is merged.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public MultiMapEventHandlers<TKey, TValue> EntryMerged(Func<IHMultiMap<TKey, TValue>, MapEntryMergedEventArgs<TKey, TValue>, CancellationToken, ValueTask> handler)
        {
            Add(new MapEntryMergedEventHandler<TKey, TValue, IHMultiMap<TKey, TValue>>(handler));
            return this;
        }
    }
}