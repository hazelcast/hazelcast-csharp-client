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
    public class ReplicatedDictionaryEventHandlers<TKey, TValue> : EventHandlersBase<IDictionaryEventHandlerBase>
    {
        /// <summary>
        /// Adds an handler which runs when the map is cleared.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public ReplicatedDictionaryEventHandlers<TKey, TValue> Cleared(Action<IHReplicatedDictionary<TKey, TValue>, DictionaryClearedEventArgs> handler)
        {
            Add(new DictionaryClearedEventHandler<TKey, TValue, IHReplicatedDictionary<TKey, TValue>>(handler.AsAsync()));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when the map is cleared.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public ReplicatedDictionaryEventHandlers<TKey, TValue> Cleared(Func<IHReplicatedDictionary<TKey, TValue>, DictionaryClearedEventArgs, ValueTask> handler)
        {
            Add(new DictionaryClearedEventHandler<TKey, TValue, IHReplicatedDictionary<TKey, TValue>>(handler));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is updated.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public ReplicatedDictionaryEventHandlers<TKey, TValue> EntryUpdated(Action<IHReplicatedDictionary<TKey, TValue>, DictionaryEntryUpdatedEventArgs<TKey, TValue>> handler)
        {
            Add(new DictionaryEntryUpdatedEventHandler<TKey, TValue, IHReplicatedDictionary<TKey, TValue>>(handler.AsAsync()));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is updated.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public ReplicatedDictionaryEventHandlers<TKey, TValue> EntryUpdated(Func<IHReplicatedDictionary<TKey, TValue>, DictionaryEntryUpdatedEventArgs<TKey, TValue>, ValueTask> handler)
        {
            Add(new DictionaryEntryUpdatedEventHandler<TKey, TValue, IHReplicatedDictionary<TKey, TValue>>(handler));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is removed.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public ReplicatedDictionaryEventHandlers<TKey, TValue> EntryRemoved(Action<IHReplicatedDictionary<TKey, TValue>, DictionaryEntryRemovedEventArgs<TKey, TValue>> handler)
        {
            Add(new DictionaryEntryRemovedEventHandler<TKey, TValue, IHReplicatedDictionary<TKey, TValue>>(handler.AsAsync()));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is removed.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public ReplicatedDictionaryEventHandlers<TKey, TValue> EntryRemoved(Func<IHReplicatedDictionary<TKey, TValue>, DictionaryEntryRemovedEventArgs<TKey, TValue>, ValueTask> handler)
        {
            Add(new DictionaryEntryRemovedEventHandler<TKey, TValue, IHReplicatedDictionary<TKey, TValue>>(handler));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is added.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public ReplicatedDictionaryEventHandlers<TKey, TValue> EntryAdded(Action<IHReplicatedDictionary<TKey, TValue>, DictionaryEntryAddedEventArgs<TKey, TValue>> handler)
        {
            Add(new DictionaryEntryAddedEventHandler<TKey, TValue, IHReplicatedDictionary<TKey, TValue>>(handler.AsAsync()));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is added.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public ReplicatedDictionaryEventHandlers<TKey, TValue> EntryAdded(Func<IHReplicatedDictionary<TKey, TValue>, DictionaryEntryAddedEventArgs<TKey, TValue>, ValueTask> handler)
        {
            Add(new DictionaryEntryAddedEventHandler<TKey, TValue, IHReplicatedDictionary<TKey, TValue>>(handler));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is merged.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public ReplicatedDictionaryEventHandlers<TKey, TValue> EntryMerged(Action<IHReplicatedDictionary<TKey, TValue>, DictionaryEntryMergedEventArgs<TKey, TValue>> handler)
        {
            Add(new DictionaryEntryMergedEventHandler<TKey, TValue, IHReplicatedDictionary<TKey, TValue>>(handler.AsAsync()));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a map entry is merged.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public ReplicatedDictionaryEventHandlers<TKey, TValue> EntryMerged(Func<IHReplicatedDictionary<TKey, TValue>, DictionaryEntryMergedEventArgs<TKey, TValue>, ValueTask> handler)
        {
            Add(new DictionaryEntryMergedEventHandler<TKey, TValue, IHReplicatedDictionary<TKey, TValue>>(handler));
            return this;
        }
    }
}
