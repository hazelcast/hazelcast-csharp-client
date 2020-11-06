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
    /// Represents multi distributed dictionary event handlers.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    public class MultiDictionaryEventHandlers<TKey, TValue> : EventHandlersBase<IMapEventHandlerBase>
    {
        /// <summary>
        /// Adds an handler which runs when the map is cleared.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public MultiDictionaryEventHandlers<TKey, TValue> Cleared(Action<IHMultiDictionary<TKey, TValue>, MapClearedEventArgs> handler)
        {
            Add(new MapClearedEventHandler<TKey, TValue, IHMultiDictionary<TKey, TValue>>(handler.AsAsync()));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when the map is cleared.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public MultiDictionaryEventHandlers<TKey, TValue> Cleared(Func<IHMultiDictionary<TKey, TValue>, MapClearedEventArgs, ValueTask> handler)
        {
            Add(new MapClearedEventHandler<TKey, TValue, IHMultiDictionary<TKey, TValue>>(handler));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is updated.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public MultiDictionaryEventHandlers<TKey, TValue> EntryUpdated(Action<IHMultiDictionary<TKey, TValue>, MapEntryUpdatedEventArgs<TKey, TValue>> handler)
        {
            Add(new MapEntryUpdatedEventHandler<TKey, TValue, IHMultiDictionary<TKey, TValue>>(handler.AsAsync()));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is updated.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public MultiDictionaryEventHandlers<TKey, TValue> EntryUpdated(Func<IHMultiDictionary<TKey, TValue>, MapEntryUpdatedEventArgs<TKey, TValue>, ValueTask> handler)
        {
            Add(new MapEntryUpdatedEventHandler<TKey, TValue, IHMultiDictionary<TKey, TValue>>(handler));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is removed.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public MultiDictionaryEventHandlers<TKey, TValue> EntryRemoved(Action<IHMultiDictionary<TKey, TValue>, MapEntryRemovedEventArgs<TKey, TValue>> handler)
        {
            Add(new MapEntryRemovedEventHandler<TKey, TValue, IHMultiDictionary<TKey, TValue>>(handler.AsAsync()));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is removed.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public MultiDictionaryEventHandlers<TKey, TValue> EntryRemoved(Func<IHMultiDictionary<TKey, TValue>, MapEntryRemovedEventArgs<TKey, TValue>, ValueTask> handler)
        {
            Add(new MapEntryRemovedEventHandler<TKey, TValue, IHMultiDictionary<TKey, TValue>>(handler));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is added.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public MultiDictionaryEventHandlers<TKey, TValue> EntryAdded(Action<IHMultiDictionary<TKey, TValue>, MapEntryAddedEventArgs<TKey, TValue>> handler)
        {
            Add(new MapEntryAddedEventHandler<TKey, TValue, IHMultiDictionary<TKey, TValue>>(handler.AsAsync()));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is added.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public MultiDictionaryEventHandlers<TKey, TValue> EntryAdded(Func<IHMultiDictionary<TKey, TValue>, MapEntryAddedEventArgs<TKey, TValue>, ValueTask> handler)
        {
            Add(new MapEntryAddedEventHandler<TKey, TValue, IHMultiDictionary<TKey, TValue>>(handler));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is merged.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public MultiDictionaryEventHandlers<TKey, TValue> EntryMerged(Action<IHMultiDictionary<TKey, TValue>, MapEntryMergedEventArgs<TKey, TValue>> handler)
        {
            Add(new MapEntryMergedEventHandler<TKey, TValue, IHMultiDictionary<TKey, TValue>>(handler.AsAsync()));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is merged.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public MultiDictionaryEventHandlers<TKey, TValue> EntryMerged(Func<IHMultiDictionary<TKey, TValue>, MapEntryMergedEventArgs<TKey, TValue>, ValueTask> handler)
        {
            Add(new MapEntryMergedEventHandler<TKey, TValue, IHMultiDictionary<TKey, TValue>>(handler));
            return this;
        }
    }
}
