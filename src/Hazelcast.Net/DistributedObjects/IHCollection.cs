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
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hazelcast.DistributedObjects
{
    /// <summary>
    /// Defines a concurrent, distributed, and listenable collection.
    /// </summary>
    /// <remarks>
    /// <para>This is not a partitioned data-structure. Entire contents
    /// is stored on a single machine (and in the backup). It will not
    /// scale by adding more members to the cluster.
    /// </para>
    /// </remarks>
    /// <typeparam name="T">The type of the items in the collection</typeparam>
    public interface IHCollection<T> : IDistributedObject, IAsyncEnumerable<T>
    {
        //setting
        /// <summary>
        /// Adds an item to the collection.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>Whether the item was added.</returns>
        Task<bool> AddAsync(T item);

        /// <summary>
        /// Adds all.
        /// </summary>
        /// <typeparam name="TItem">type of elements</typeparam>
        /// <param name="items">element collection</param>
        /// <returns><c>true</c> if this collection changed, <c>false</c> otherwise.</returns>
        Task<bool> AddRangeAsync<TItem>(ICollection<TItem> items) where TItem : T;

        //getting
        /// <summary>
        /// Gets the collection items.
        /// </summary>
        /// <returns>The collection items.</returns>
        Task<IReadOnlyList<T>> GetAllAsync();

        /// <summary>
        /// Gets the number of items in the collection.
        /// </summary>
        /// <returns>The number of items in the collection.</returns>
        Task<int> CountAsync();

        /// <summary>
        /// Determines whether this instance is empty.
        /// </summary>
        /// <returns><c>true</c> if this instance is empty; otherwise, <c>false</c>.</returns>
        Task<bool> IsEmptyAsync();

        /// <summary>
        /// Determines whether this collection contains all of the elements in the specified collection.
        /// </summary>
        /// <typeparam name="TItem">type of elements</typeparam>
        /// <param name="items">The collection</param>
        /// <returns><c>true</c> if this collection contains all of the elements in the specified collection; otherwise, <c>false</c>.</returns>
        Task<bool> ContainsAllAsync<TItem>(ICollection<TItem> items) where TItem : T;

        /// <summary>
        /// Determines whether the collection contains an item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns><c>true</c> if the collection contains the item; otherwise <c>false</c>.</returns>
        Task<bool> ContainsAsync(T item);

        //removing
        /// <summary>
        /// Removes an item from the collection.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns><c>true</c> if the item was removed; otherwise <c>false</c>.</returns>
        Task<bool> RemoveAsync(T item);

        /// <summary>
        /// Removes all of the elements in the specified collection from this collection.
        /// </summary>
        /// <typeparam name="TItem">type of elements</typeparam>
        /// <param name="items">element collection to be removed</param>
        /// <returns><c>true</c> if all removed, <c>false</c> otherwise.</returns>
        Task<bool> RemoveAllAsync<TItem>(ICollection<TItem> items) where TItem : T;

        /// <summary>
        /// Retains only the elements in this collection that are contained in the specified collection.
        /// </summary>
        /// <remarks>
        /// In other words, removes from this collection all of its elements that are not contained in the specified collection.
        /// </remarks>
        /// <typeparam name="TItem">type of elements</typeparam>
        /// <param name="items">The c.</param>
        /// <returns><c>true</c> if this collection changed, <c>false</c> otherwise.</returns>
        Task<bool> RetainAllAsync<TItem>(ICollection<TItem> items) where TItem : T;

        /// <summary>
        /// Clears the collection.
        /// </summary>
        /// <returns>A task that will complete when the collection has been cleared.</returns>
        Task ClearAsync();

        /// <summary>
        /// Subscribes to events.
        /// </summary>
        /// <param name="events">An event handlers collection builder.</param>
        /// <param name="includeValue">Whether to include values in event arguments.</param>
        /// <param name="state">A state object that will be passed to handlers.</param>
        /// <returns>The unique identifier of the subscription.</returns>
        Task<Guid> SubscribeAsync(Action<CollectionItemEventHandlers<T>> events, bool includeValue = true, object state = null);

        /// <summary>
        /// Unsubscribe from events.
        /// </summary>
        /// <param name="subscriptionId">The unique identifier of the subscription.</param>
        /// <returns>Whether the operation was successful.</returns>
        /// <remarks>
        /// <para>Once this method has been invoked, and whatever its result, the subscription is
        /// de-activated, which means that no events will trigger anymore, even if the client
        /// receives event messages from the servers.</para>
        /// <para>If this method returns <c>false</c>, then one or more client connection has not
        /// been able to get its server to remove the subscription. Even though no events will
        /// trigger anymore, the server may keep sending (ignored) event messages. It is therefore
        /// recommended to retry unsubscribing until it is successful.</para>
        /// </remarks>
        ValueTask<bool> UnsubscribeAsync(Guid subscriptionId);
    }
}
