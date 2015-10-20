/*
* Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

namespace Hazelcast.Core
{
    /// <summary>
    ///     Map Entry listener to get notified when a map entry
    ///     is added, removed, updated or evicted.
    /// </summary>
    /// <remarks>
    ///     Map Entry listener to get notified when a map entry
    ///     is added, removed, updated or evicted.
    /// </remarks>
    public interface IEntryListener<K, V> : IEventListener
    {
        /// <summary>Invoked when an entry is added.</summary>
        /// <remarks>Invoked when an entry is added.</remarks>
        /// <param name="event">entry event</param>
        void EntryAdded(EntryEvent<K, V> @event);

        /// <summary>Invoked when an entry is removed.</summary>
        /// <remarks>Invoked when an entry is removed.</remarks>
        /// <param name="event">entry event</param>
        void EntryRemoved(EntryEvent<K, V> @event);

        /// <summary>Invoked when an entry is updated.</summary>
        /// <remarks>Invoked when an entry is updated.</remarks>
        /// <param name="event">entry event</param>
        void EntryUpdated(EntryEvent<K, V> @event);

        /// <summary>Invoked when an entry is evicted.</summary>
        /// <remarks>Invoked when an entry is evicted.</remarks>
        /// <param name="event">entry event</param>
        void EntryEvicted(EntryEvent<K, V> @event);

        /// <summary>Invoked when all entries are evicted.</summary>
        /// <remarks>Invoked when all entries are evicted.</remarks>
        /// <param name="event">entry event</param>
        void MapEvicted(MapEvent @event);

       /// <summary>Invoked when all entries are removed.</summary>
        /// <remarks>Invoked when all entries are removed.</remarks>
        /// <param name="event">entry event</param>
        void MapCleared(MapEvent @event);

    }
}