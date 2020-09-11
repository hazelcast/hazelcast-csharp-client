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
    /// Represents distributed dictionary event handlers.
    /// </summary>
    public sealed class DictionaryEventHandlers<TKey, TValue> : EventHandlersBase<IDictionaryEventHandlerBase>
    {
        /// <summary>
        /// Adds an handler which runs when the map is cleared.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public DictionaryEventHandlers<TKey, TValue> Cleared(Action<IHDictionary<TKey, TValue>, DictionaryClearedEventArgs> handler)
        {
            Add(new DictionaryClearedEventHandler<TKey, TValue, IHDictionary<TKey, TValue>>(handler.AsAsync()));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when the map is cleared.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public DictionaryEventHandlers<TKey, TValue> Cleared(Func<IHDictionary<TKey, TValue>, DictionaryClearedEventArgs, ValueTask> handler)
        {
            Add(new DictionaryClearedEventHandler<TKey, TValue, IHDictionary<TKey, TValue>>(handler));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when the map is evicted.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public DictionaryEventHandlers<TKey, TValue> Evicted(Action<IHDictionary<TKey, TValue>, DictionaryEvictedEventArgs> handler)
        {
            Add(new DictionaryEvictedEventHandler<TKey, TValue, IHDictionary<TKey, TValue>>(handler.AsAsync()));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when the map is evicted.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public DictionaryEventHandlers<TKey, TValue> Evicted(Func<IHDictionary<TKey, TValue>, DictionaryEvictedEventArgs, ValueTask> handler)
        {
            Add(new DictionaryEvictedEventHandler<TKey, TValue, IHDictionary<TKey, TValue>>(handler));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is updated.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public DictionaryEventHandlers<TKey, TValue> EntryUpdated(Action<IHDictionary<TKey, TValue>, DictionaryEntryUpdatedEventArgs<TKey, TValue>> handler)
        {
            Add(new DictionaryEntryUpdatedEventHandler<TKey, TValue, IHDictionary<TKey, TValue>>(handler.AsAsync()));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is updated.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public DictionaryEventHandlers<TKey, TValue> EntryUpdated(Func<IHDictionary<TKey, TValue>, DictionaryEntryUpdatedEventArgs<TKey, TValue>, ValueTask> handler)
        {
            Add(new DictionaryEntryUpdatedEventHandler<TKey, TValue, IHDictionary<TKey, TValue>>(handler));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is removed.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public DictionaryEventHandlers<TKey, TValue> EntryRemoved(Action<IHDictionary<TKey, TValue>, DictionaryEntryRemovedEventArgs<TKey, TValue>> handler)
        {
            Add(new DictionaryEntryRemovedEventHandler<TKey, TValue, IHDictionary<TKey, TValue>>(handler.AsAsync()));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is removed.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public DictionaryEventHandlers<TKey, TValue> EntryRemoved(Func<IHDictionary<TKey, TValue>, DictionaryEntryRemovedEventArgs<TKey, TValue>, ValueTask> handler)
        {
            Add(new DictionaryEntryRemovedEventHandler<TKey, TValue, IHDictionary<TKey, TValue>>(handler));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is added.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public DictionaryEventHandlers<TKey, TValue> EntryAdded(Action<IHDictionary<TKey, TValue>, DictionaryEntryAddedEventArgs<TKey, TValue>> handler)
        {
            Add(new DictionaryEntryAddedEventHandler<TKey, TValue, IHDictionary<TKey, TValue>>(handler.AsAsync()));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is added.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public DictionaryEventHandlers<TKey, TValue> EntryAdded(Func<IHDictionary<TKey, TValue>, DictionaryEntryAddedEventArgs<TKey, TValue>, ValueTask> handler)
        {
            Add(new DictionaryEntryAddedEventHandler<TKey, TValue, IHDictionary<TKey, TValue>>(handler));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is evicted.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public DictionaryEventHandlers<TKey, TValue> EntryEvicted(Action<IHDictionary<TKey, TValue>, DictionaryEntryEvictedEventArgs<TKey, TValue>> handler)
        {
            Add(new DictionaryEntryEvictedEventHandler<TKey, TValue, IHDictionary<TKey, TValue>>(handler.AsAsync()));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is evicted.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public DictionaryEventHandlers<TKey, TValue> EntryEvicted(Func<IHDictionary<TKey, TValue>, DictionaryEntryEvictedEventArgs<TKey, TValue>, ValueTask> handler)
        {
            Add(new DictionaryEntryEvictedEventHandler<TKey, TValue, IHDictionary<TKey, TValue>>(handler));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is expired.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public DictionaryEventHandlers<TKey, TValue> EntryExpired(Action<IHDictionary<TKey, TValue>, DictionaryEntryExpiredEventArgs<TKey, TValue>> handler)
        {
            Add(new DictionaryEntryExpiredEventHandler<TKey, TValue, IHDictionary<TKey, TValue>>(handler.AsAsync()));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is expired.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public DictionaryEventHandlers<TKey, TValue> EntryExpired(Func<IHDictionary<TKey, TValue>, DictionaryEntryExpiredEventArgs<TKey, TValue>, ValueTask> handler)
        {
            Add(new DictionaryEntryExpiredEventHandler<TKey, TValue, IHDictionary<TKey, TValue>>(handler));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is invalidated.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public DictionaryEventHandlers<TKey, TValue> EntryInvalidated(Action<IHDictionary<TKey, TValue>, DictionaryEntryInvalidatedEventArgs<TKey, TValue>> handler)
        {
            Add(new DictionaryEntryInvalidatedEventHandler<TKey, TValue, IHDictionary<TKey, TValue>>(handler.AsAsync()));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is invalidated.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public DictionaryEventHandlers<TKey, TValue> EntryInvalidated(Func<IHDictionary<TKey, TValue>, DictionaryEntryInvalidatedEventArgs<TKey, TValue>, ValueTask> handler)
        {
            Add(new DictionaryEntryInvalidatedEventHandler<TKey, TValue, IHDictionary<TKey, TValue>>(handler));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is loaded.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public DictionaryEventHandlers<TKey, TValue> EntryLoaded(Action<IHDictionary<TKey, TValue>, DictionaryEntryLoadedEventArgs<TKey, TValue>> handler)
        {
            Add(new DictionaryEntryLoadedEventHandler<TKey, TValue, IHDictionary<TKey, TValue>>(handler.AsAsync()));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is loaded.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public DictionaryEventHandlers<TKey, TValue> EntryLoaded(Func<IHDictionary<TKey, TValue>, DictionaryEntryLoadedEventArgs<TKey, TValue>, ValueTask> handler)
        {
            Add(new DictionaryEntryLoadedEventHandler<TKey, TValue, IHDictionary<TKey, TValue>>(handler));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is merged.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public DictionaryEventHandlers<TKey, TValue> EntryMerged(Action<IHDictionary<TKey, TValue>, DictionaryEntryMergedEventArgs<TKey, TValue>> handler)
        {
            Add(new DictionaryEntryMergedEventHandler<TKey, TValue, IHDictionary<TKey, TValue>>(handler.AsAsync()));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is merged.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public DictionaryEventHandlers<TKey, TValue> EntryMerged(Func<IHDictionary<TKey, TValue>, DictionaryEntryMergedEventArgs<TKey, TValue>, ValueTask> handler)
        {
            Add(new DictionaryEntryMergedEventHandler<TKey, TValue, IHDictionary<TKey, TValue>>(handler));
            return this;
        }
    }
}
