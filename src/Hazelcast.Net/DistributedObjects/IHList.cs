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

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hazelcast.DistributedObjects
{
    /// <summary>
    /// Defines a concurrent, distributed, non-partitioned and listenable list
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Hazelcast <c>IHList</c> is not a partitioned data-structure. Entire contents
    /// of an <c>IHList</c> is stored on a single machine (and in the backup). The <c>IHList</c>
    /// will not scale by adding more members to the cluster.
    /// </para>
    /// </remarks>
     public interface IHList<T> : IHCollection<T>
    {
        /// <summary>
        /// Inserts the specified element at the specified position in this list.
        /// Shifts the element currently at that position
        /// (if any) and any subsequent elements to the right (adds one to their
        /// indices).
        /// </summary>
        /// <param name="index">index at which the specified element is to be inserted</param>
        /// <param name="item">element to be inserted</param>
        Task InsertAsync(int index, T item);

        /// <summary>
        /// Inserts all of the elements in the specified collection into this
        /// list at the specified position.
        /// </summary>
        /// <param name="index">index at which to insert the first element from the specified collection</param>
        /// <param name="items">collection containing elements to be added to this list</param>
        /// <typeparam name="TItem"></typeparam>
        /// <returns><c>true</c> if this list changed as a result of the call</returns>
        /// <remarks>
        /// Shifts the element currently at that position (if any) and any subsequent
        /// elements to the right (increases their indices).  The new elements
        /// will appear in this list in the order that they are returned by the
        /// specified collection's iterator.  The behavior of this operation is
        /// undefined if the specified collection is modified while the
        /// operation is in progress.  (Note that this will occur if the specified
        /// collection is this list, and it's nonempty.)
        /// </remarks>
        Task<bool> InsertRangeAsync<TItem>(int index, ICollection<TItem> items) where TItem : T;

        /// <summary>
        /// Replaces the element at the specified position in this list with the specified element.
        /// </summary>
        /// <param name="index">index index of the element to replace</param>
        /// <param name="item">element to be stored at the specified position</param>
        /// <returns>The element previously at the specified position</returns>
        Task<T> GetAndSetAsync(int index, T item);

        //Getting
        /// <summary>
        /// Returns the element in the specified position in this list
        /// </summary>
        /// <param name="index">index of the element to return</param>
        /// <returns>the element at the specified position in the list</returns>
        Task<T> GetAsync(int index);

        /// <summary>
        /// Returns a view of the portion of this list between the specified
        /// <c>fromIndex</c>, inclusive, and <c>toIndex</c>, exclusive.
        /// </summary>
        /// <param name="fromIndex">low endpoint (inclusive) of the subList</param>
        /// <param name="toIndex">high endpoint (exclusive) of the subList</param>
        /// <returns>a view of the specified range within this list</returns>
        /// <remarks>
        /// If  <c>fromIndex</c> and <tt>toIndex</tt> are equal, the returned list is empty.
        /// </remarks>
        Task<IReadOnlyList<T>> GetRangeAsync(int fromIndex, int toIndex);

        /// <summary>
        /// Returns the zero-based index of the first occurrence of a specific item in this list.
        /// </summary>
        /// <param name="item">element to search for</param>
        /// <returns>the index of the first occurrence of the specified element in
        ///  this list, or -1 if this list does not contain the element</returns>
        Task<int> IndexOfAsync(T item);

        /// <summary>
        /// Returns the index of the last occurrence of the specified element
        /// in this list, or -1 if this list does not contain the element.
        /// </summary>
        /// <param name="item">element to search for</param>
        /// <returns>the index of the last occurrence of the specified element in
        ///  this list, or -1 if this list does not contain the element</returns>
        Task<int> LastIndexOfAsync(T item);

        //Removing
        /// <summary>
        /// Removes the first occurrence of the specified element from this list, if it is present.
        /// If this list does not contain the element, it is unchanged.
        /// </summary>
        /// <param name="index">element to be removed from this list, if present</param>
        /// <returns>the element previously at the specified position</returns>
        Task<T> GetAndRemoveAtAsync(int index);
    }
}