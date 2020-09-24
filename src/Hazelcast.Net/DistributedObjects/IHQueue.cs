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
    /// Defines a concurrent, blocking, distributed, and observable queue.
    /// </summary>
    public interface IHQueue<T> : IHCollection<T>
    {
        /// <summary>
        /// Inserts the specified element into this queue, waiting if necessary for space to become available.
        /// </summary>
        /// <param name="item">the element to Add</param>
        Task EnqueueAsync(T item);

        /// <summary>
        /// Inserts the specified element into this queue if it is possible to do
        /// so immediately without violating capacity restrictions, returning
        /// <c>true</c> upon success and <c>false</c> if no space is currently
        /// available.
        /// </summary>
        /// <remarks>
        /// Inserts the specified element into this queue if it is possible to do
        /// so immediately without violating capacity restrictions, returning
        /// <c>true</c> upon success and <c>false</c> if no space is currently
        /// available.
        /// </remarks>
        /// <param name="item">the element to Add</param>
        /// <param name="timeToWait">how long to wait before giving up</param>
        /// <returns>
        /// <c>true</c> if the element was added to this queue, else <c>false</c>
        /// </returns>
        Task<bool> TryEnqueueAsync(T item, TimeSpan timeToWait=default);

        //getting
        /// <summary>
        /// Retrieves and removes the head of this queue, waiting if necessary until an element becomes available.
        /// </summary>
        /// <returns>the head of this queue</returns>
        Task<T> DequeueAsync();

        /// <summary>
        /// Retrieves and removes the head of this queue, waiting up to the
        /// specified wait time if necessary for an element to become available.
        /// </summary>
        /// <remarks>
        /// Retrieves and removes the head of this queue, waiting up to the
        /// specified wait time if necessary for an element to become available.
        /// </remarks>
        /// <param name="timeToWait">how long to wait before giving up</param>
        /// <returns>
        /// the head of this queue, or <c>null</c> if the
        /// specified waiting time elapses before an element is available
        /// </returns>
        Task<T> TryDequeueAsync(TimeSpan timeToWait=default);

        /// <summary>
        /// Removes at most the given number of available elements from
        /// this queue and adds them to the given collection.  A failure
        /// encountered while attempting to Add elements to
        /// collection <c>c</c> may result in elements being in neither,
        /// either or both collections when the associated exception is
        /// thrown.  Attempts to drain a queue to itself result in
        /// <c>IllegalArgumentException</c>. Further, the behavior of
        /// this operation is undefined if the specified collection is
        /// modified while the operation is in progress.
        /// </summary>
        /// <typeparam name="TItem">type of elements</typeparam>
        /// <param name="items">the collection to transfer elements into</param>
        /// <param name="maxElements">the maximum number of elements to transfer</param>
        /// <returns>the number of elements transferred</returns>
        Task<int> DrainToAsync<TItem>(ICollection<TItem> items, int maxElements = int.MaxValue) where TItem : T;

        /// <summary>
        /// Retrieves, but does not remove, the head of this queue.
        /// </summary>
        /// <remarks>
        /// This method differs from <see cref="TryPeekAsync"/> only in that it throws an exception if this queue is empty.
        /// </remarks>
        /// <returns>the head of this queue</returns>
        Task<T> PeekAsync();

        /// <summary>
        /// Retrieves, but does not remove, the head of this queue, or returns <c>null</c> if this queue is empty.
        /// </summary>
        /// <returns>the head of this queue, or <c>null</c> if this queue is empty</returns>
        Task<T> TryPeekAsync();

        /// <summary>
        /// Returns the number of additional elements that this queue can ideally
        /// (in the absence of memory or resource constraints) accept without
        /// blocking, or <c>Int64.MaxValue</c> if there is no intrinsic
        /// limit.
        ///</summary>
        /// <remarks>
        /// <para>Note that you <em>cannot</em> always tell if an attempt to insert
        /// an element will succeed by inspecting <c>remainingCapacity</c>
        /// because it may be the case that another thread is about to
        /// insert or remove an element.
        ///</para>
        /// </remarks>
        ///<returns>the remaining capacity </returns>
        Task<int> GetRemainingCapacityAsync();
    }
}
