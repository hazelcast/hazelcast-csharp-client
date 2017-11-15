// Copyright (c) 2008-2017, Hazelcast, Inc. All Rights Reserved.
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
    /// <typeparam name="TKey">the type of key</typeparam>
    /// <typeparam name="TValue">the type of value</typeparam>
    public interface IEntryListener<TKey, TValue> : EntryAddedListener<TKey, TValue>,
        EntryUpdatedListener<TKey, TValue>, EntryRemovedListener<TKey, TValue>, EntryEvictedListener<TKey, TValue>,
        MapClearedListener, MapEvictedListener

    {
    }

    /// <summary>
    /// Invoked when an entry is added.
    /// </summary>
    /// <typeparam name="TKey">the type of key</typeparam>
    /// <typeparam name="TValue">the type of value</typeparam>
    public interface EntryAddedListener<TKey, TValue> : MapListener
    {
        /// <summary>Invoked when an entry is added.</summary>
        /// <remarks>Invoked when an entry is added.</remarks>
        /// <param name="event">entry event</param>
        void EntryAdded(EntryEvent<TKey, TValue> @event);
    }

    /// <summary>
    /// Invoked when an entry is updated.
    /// </summary>
    /// <typeparam name="TKey">the type of key</typeparam>
    /// <typeparam name="TValue">the type of value</typeparam>
    public interface EntryUpdatedListener<TKey, TValue> : MapListener
    {
        /// <summary>Invoked when an entry is updated.</summary>
        /// <remarks>Invoked when an entry is updated.</remarks>
        /// <param name="event">entry event</param>
        void EntryUpdated(EntryEvent<TKey, TValue> @event);
    }

    /// <summary>
    /// Invoked when an entry is removed.
    /// </summary>
    /// <typeparam name="TKey">the type of key</typeparam>
    /// <typeparam name="TValue">the type of value</typeparam>
    public interface EntryRemovedListener<TKey, TValue> : MapListener
    {
        /// <summary>Invoked when an entry is removed.</summary>
        /// <remarks>Invoked when an entry is removed.</remarks>
        /// <param name="event">entry event</param>
        void EntryRemoved(EntryEvent<TKey, TValue> @event);
    }

    /// <summary>
    /// Invoked when an entry is evicted.
    /// </summary>
    /// <typeparam name="TKey">the type of key</typeparam>
    /// <typeparam name="TValue">the type of value</typeparam>
    public interface EntryEvictedListener<TKey, TValue> : MapListener
    {
        /// <summary>Invoked when an entry is evicted.</summary>
        /// <remarks>Invoked when an entry is evicted.</remarks>
        /// <param name="event">entry event</param>
        void EntryEvicted(EntryEvent<TKey, TValue> @event);
    }

    /// <summary>
    /// Invoked after WAN replicated entry is merged.
    /// </summary>
    /// <typeparam name="TKey">the type of key</typeparam>
    /// <typeparam name="TValue">the type of value</typeparam>
    public interface EntryMergedListener<TKey, TValue> : MapListener
    {
        /// <summary>
        /// Invoked after WAN replicated entry is merged.
        /// </summary>
        /// <param name="event">entry event</param>
        void EntryMerged(EntryEvent<TKey, TValue> @event);
    }

    /// <summary>
    /// Listener which is notified after removal of an entry due to the expiration-based-eviction.
    /// </summary>
    /// <remarks>
    /// There are two sources of expiration based eviction, they are max-idle-seconds and time-to-live-seconds.
    /// 
    /// Note that if your listener implements both <c>EntryExpiredListener</c> and <c>EntryEvictedListener</c> together,
    ///  there is a probability that the listener may receive both expiration and eviction events for the same entry. 
    /// This is because, size-based-eviction removes entries regardless of whether entries expired or not.
    /// </remarks>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public interface EntryExpiredListener<TKey, TValue> : MapListener
    {
        /// <summary>
        /// Invoked upon expiration of an entry.
        /// </summary>
        /// <param name="event">event the event invoked when an entry is expired.</param>
        void EntryExpired(EntryEvent<TKey, TValue> @event);
    }

    /// <summary>
    /// Invoked when all entries are removed.
    /// </summary>
    public interface MapClearedListener : MapListener
    {
        /// <summary>Invoked when all entries are removed.</summary>
        /// <remarks>Invoked when all entries are removed.</remarks>
        /// <param name="event">entry event</param>
        void MapCleared(MapEvent @event);
    }

    /// <summary>
    /// Invoked when all entries are evicted.
    /// </summary>
    public interface MapEvictedListener : MapListener
    {
        /// <summary>Invoked when all entries are evicted.</summary>
        /// <remarks>Invoked when all entries are evicted.</remarks>
        /// <param name="event">entry event</param>
        void MapEvicted(MapEvent @event);
    }

    /// <summary>
    /// A marker interface which is used to get notified upon a map or an entry event.
    /// </summary>
    /// <remarks>
    /// <lu>
    /// <li>
    /// A map event is fired as a result of a map-wide operations like e.g. <see cref="EntryEventType.ClearAll"/> or 
    /// <see cref="EntryEventType.EvictAll"/>
    /// </li>
    /// <li>
    /// An entry event is fired after the changes that affects a specific entry e.g. <see cref="EntryEventType.Added"/>,
    ///  <see cref="EntryEventType.Updated"/>
    /// </li>
    /// An implementer of this interface should extend one of the sub-interfaces of it to receive a corresponding event.
    /// </remarks>
    /// <seealso cref="EntryAddedListener{TKey,TValue}"/>
    /// <seealso cref="EntryUpdatedListener{TKey,TValue}"/>
    /// <seealso cref="EntryRemovedListener{TKey,TValue}"/>
    /// <seealso cref="EntryEvictedListener{TKey,TValue}"/>
    /// <seealso cref="EntryExpiredListener{TKey,TValue}"/>
    /// <seealso cref="MapClearedListener"/>
    /// <seealso cref="MapEvictedListener"/>
    public interface MapListener : IEventListener
    {
    }
}