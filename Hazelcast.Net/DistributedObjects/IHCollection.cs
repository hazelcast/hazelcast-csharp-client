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
using System.Threading;
using System.Threading.Tasks;

namespace Hazelcast.DistributedObjects
{
    /// <summary>Concurrent, distributed, partitioned, listenable collection.</summary>
    /// <remarks>Concurrent, distributed, partitioned, listenable collection.</remarks>
    public interface IHCollection<T> : IDistributedObject
    {
        // these mimics ICollection<T>

        Task<int> CountAsync(TimeSpan timeout = default);
        Task<int> CountAsync(CancellationToken cancellationToken);

        Task<bool> AddAsync(T item, TimeSpan timeout = default);
        Task<bool> AddAsync(T item, CancellationToken cancellationToken);

        Task ClearAsync(TimeSpan timeout = default);
        Task ClearAsync(CancellationToken cancellationToken);
        
        Task<bool> ContainsAsync(T item, TimeSpan timeout = default);
        Task<bool> ContainsAsync(T item, CancellationToken cancellationToken);

        Task<bool> RemoveAsync(T item, TimeSpan timeout = default);
        Task<bool> RemoveAsync(T item, CancellationToken cancellationToken);

        Task CopyToAsync(T[] array, int arrayIndex, TimeSpan timeout = default);
        Task CopyToAsync(T[] array, int arrayIndex, CancellationToken cancellationToken);

        // rest is a mix of influences

        Task<IReadOnlyList<T>> GetAllAsync(TimeSpan timeout = default);
        Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Adds all.
        /// </summary>
        /// <typeparam name="TItem">type of elements</typeparam>
        /// <param name="c">element collection</param>
        /// <returns><c>true</c> if this collection changed, <c>false</c> otherwise.</returns>
        Task<bool> AddRangeAsync<TItem>(ICollection<TItem> items, TimeSpan timeout = default) where TItem : T;
        Task<bool> AddRangeAsync<TItem>(ICollection<TItem> items, CancellationToken cancellationToken) where TItem : T;

        /// <summary>
        /// Determines whether this collection contains all of the elements in the specified collection.
        /// </summary>
        /// <typeparam name="TItem">type of elements</typeparam>
        /// <param name="c">The collection</param>
        /// <returns><c>true</c> if this collection contains all of the elements in the specified collection; otherwise, <c>false</c>.</returns>
        Task<bool> ContainsAllAsync<TItem>(ICollection<TItem> items, TimeSpan timeout = default) where TItem : T;
        Task<bool> ContainsAllAsync<TItem>(ICollection<TItem> items, CancellationToken cancellationToken) where TItem : T;

        /// <summary>
        /// Determines whether this instance is empty.
        /// </summary>
        /// <returns><c>true</c> if this instance is empty; otherwise, <c>false</c>.</returns>
        Task<bool> IsEmptyAsync(TimeSpan timeout = default);
        Task<bool> IsEmptyAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Removes all of the elements in the specified collection from this collection.
        /// </summary>
        /// <typeparam name="TItem">type of elements</typeparam>
        /// <param name="c">element collection to be removed</param>
        /// <returns><c>true</c> if all removed, <c>false</c> otherwise.</returns>
        Task<bool> RemoveAllAsync<TItem>(ICollection<TItem> items, TimeSpan timeout = default) where TItem : T;
        Task<bool> RemoveAllAsync<TItem>(ICollection<TItem> items, CancellationToken cancellationToken) where TItem : T;

        /// <summary>
        /// Retains only the elements in this collection that are contained in the specified collection (optional operation).
        /// </summary>
        /// <remarks>
        /// Retains only the elements in this collection that are contained in the specified collection (optional operation).
        /// In other words, removes from this collection all of its elements that are not contained in the specified collection
        /// </remarks>
        /// <typeparam name="TItem">type of elements</typeparam>
        /// <param name="c">The c.</param>
        /// <returns><c>true</c> if this collection changed, <c>false</c> otherwise.</returns>
        Task<bool> RetainAllAsync<TItem>(ICollection<TItem> items, TimeSpan timeout = default) where TItem : T;
        Task<bool> RetainAllAsync<TItem>(ICollection<TItem> items, CancellationToken cancellationToken) where TItem : T;

        /// <summary>
        /// Returns an array containing all of the elements in this collection.
        /// </summary>
        /// <returns>an array containing all of the elements in this collection.</returns>
        Task<T[]> ToArrayAsync(TimeSpan timeout = default);
        Task<T[]> ToArrayAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Returns an array containing all of the elements in this collection
        /// the runtime type of the returned array is that of the specified array
        /// </summary>
        /// <typeparam name="TItem">return array type</typeparam>
        /// <param name="a">the array into which the elements of this collection are to be
        /// stored, if it is big enough; otherwise, a new array of the same
        /// runtime type is allocated for this purpose</param>
        /// <returns>an array containing all of the elements in this collection</returns>
        Task<TItem[]> ToArrayAsync<TItem>(TItem[] array, TimeSpan timeout = default) where TItem : T;
        Task<TItem[]> ToArrayAsync<TItem>(TItem[] array, CancellationToken cancellationToken) where TItem : T;

        Task<Guid> SubscribeAsync(bool includeValue, Action<CollectionItemEventHandlers<T>> on, TimeSpan timeout = default);
        Task<Guid> SubscribeAsync(bool includeValue, Action<CollectionItemEventHandlers<T>> on, CancellationToken cancellationToken);
    }
}
