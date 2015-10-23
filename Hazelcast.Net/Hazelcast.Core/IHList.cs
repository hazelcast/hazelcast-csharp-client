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

using System.Collections.Generic;

namespace Hazelcast.Core
{
    /// <summary>
    ///     Concurrent, distributed implementation of <see cref="IList{T}"/>IList 
    /// </summary>
    public interface IHList<T> : IList<T>, IHCollection<T>
    {
        /// <summary>
        /// Inserts the specified element at the specified position in this list.
        /// Shifts the element currently at that position
        /// (if any) and any subsequent elements to the right (adds one to their
        /// indices).
        /// </summary>
        /// <param name="index">index at which the specified element is to be inserted</param>
        /// <param name="element">element to be inserted</param>
        void Add(int index, T element);

        /// <summary>
        /// Inserts all of the elements in the specified collection into this
        /// list at the specified position (optional operation).  Shifts the
        /// element currently at that position (if any) and any subsequent
        /// elements to the right (increases their indices).  The new elements
        /// will appear in this list in the order that they are returned by the
        /// specified collection's iterator.  The behavior of this operation is
        /// undefined if the specified collection is modified while the
        /// operation is in progress.  (Note that this will occur if the specified
        /// collection is this list, and it's nonempty.)
        /// </summary>
        /// <param name="index">index at which to insert the first element from the specified collection</param>
        /// <param name="c">collection containing elements to be added to this list</param>
        /// <typeparam name="TE"></typeparam>
        /// <returns><tt>true</tt> if this list changed as a result of the call</returns>
        bool AddAll<TE>(int index, ICollection<TE> c) where TE : T;

        /// <summary>
        /// Returns the element in the specified position in this list
        /// </summary>
        /// <param name="index">index of the element to return</param>
        /// <returns>the element at the specified position in the list</returns>
        T Get(int index);

        /// <summary>
        /// Returns the index of the last occurrence of the specified element
        /// in this list, or -1 if this list does not contain the element.
        /// More formally, returns the highest index <tt>i</tt> such that
        /// <tt>(o == null ? get(i) == null : o.equals(get(i)))</tt>
        /// or -1 if there is no such index.
        /// </summary>
        /// <param name="o">element to search for</param>
        /// <returns>the index of the last occurrence of the specified element in
        ///  this list, or -1 if this list does not contain the element</returns>
        int LastIndexOf(T o);

        /// <summary>
        /// Removes the first occurrence of the specified element from this list,
        /// if it is present (optional operation).  If this list does not contain
        /// the element, it is unchanged.  More formally, removes the element with
        /// the lowest index <tt>i</tt> such that
        /// <tt>(o==null ? get(i)==null : o.equals(get(i)))</tt>
        /// (if such an element exists).  Returns <tt>true</tt> if this list
        /// contained the specified element (or equivalently, if this list changed
        /// as a result of the call).
        /// </summary>
        /// <param name="index">element to be removed from this list, if present</param>
        /// <returns><tt>true</tt> if this list contained the specified element</returns>
        T Remove(int index);

        /// <summary>
        /// Replaces the element at the specified position in this list with the
        /// specified element.
        /// </summary>
        /// <param name="index">index index of the element to replace</param>
        /// <param name="element">element to be stored at the specified position</param>
        /// <returns></returns>
        T Set(int index, T element);

        /// <summary>
        /// Returns a view of the portion of this list between the specified
        /// <tt>fromIndex</tt>, inclusive, and <tt>toIndex</tt>, exclusive.  (If
        /// <tt>fromIndex</tt> and <tt>toIndex</tt> are equal, the returned list is
        /// empty.) 
        /// </summary>
        /// <param name="fromIndex">low endpoint (inclusive) of the subList</param>
        /// <param name="toIndex">high endpoint (exclusive) of the subList</param>
        /// <returns>a view of the specified range within this list</returns>
        IList<T> SubList(int fromIndex, int toIndex);
    }
}