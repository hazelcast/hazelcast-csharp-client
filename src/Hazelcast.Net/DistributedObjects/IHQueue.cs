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
    /// Defines a concurrent, blocking, distributed, non-partitioned and observable queue.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Hazelcast <c>IHQueue</c> is not a partitioned data-structure. Entire contents
    /// of an <c>IHQueue</c> is stored on a single machine (and in the backup). The <c>IHQueue</c>
    /// will not scale by adding more members to the cluster.
    /// </para>
    /// </remarks>
    public interface IHQueue<T> : IHCollection<T>
    {
        // setting

        /// <summary>
        /// Enqueues an item.
        /// </summary>
        /// <param name="item">The item to enqueue.</param>
        /// <remarks>
        /// <para>If space is not immediately available, this will wait indefinitely for space to become available.</para>
        /// </remarks>
        Task PutAsync(T item);

        /// <summary>
        /// Tries to enqueue an item.
        /// </summary>
        /// <param name="item">The item to enqueue.</param>
        /// <param name="timeToWait">How long to wait for space (-1ms to wait forever; 0ms to not wait at all).</param>
        /// <returns><c>true</c> if the element was added to this queue; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// <para>If space is not immediately available, this will wait for the specified <paramref name="timeToWait"/>
        /// for space to become available. If space does not become available in time, returns <c>false</c>.
        /// If <paramref name="timeToWait"/> is -1ms, waits forever. If it is 0ms, does not wait at all.</para>
        /// </remarks>
        Task<bool> OfferAsync(T item, TimeSpan timeToWait = default);

        // getting

        /// <summary>
        /// Dequeues the head item.
        /// </summary>
        /// <returns>The head item.</returns>
        /// <remarks>
        /// <para>If an item is not immediately available, this will wait indefinitely for an item to become available.</para>
        /// </remarks>
        Task<T> TakeAsync();

        /// <summary>
        /// Tries to dequeue an item.
        /// </summary>
        /// <param name="timeToWait">How long to wait for an item (-1ms to wait forever; 0ms to not wait at all).</param>
        /// <returns>The item, or <c>null</c> if not item could be dequeued within the specified <paramref name="timeToWait"/>.</returns>
        /// <remarks>
        /// <para>If an item is not immediately available, this will wait for the specified name="timeToWait"/>
        /// for an item to become available. If an item does not become available in time, returns <c>null</c>.
        /// If <paramref name="timeToWait"/> is -1ms, waits forever. If it is 0ms, does not wait at all.</para>
        /// </remarks>
        Task<T> PollAsync(TimeSpan timeToWait = default);

        /// <summary>
        /// Removes all available elements from this queue and adds them to the given collection.
        /// </summary>
        /// <param name="items">the collection to transfer elements into</param>
        /// <returns>the number of elements transferred</returns>
        /// <remarks>
        /// A failure encountered while attempting to Add elements to
        /// collection <c>items</c> may result in elements being in neither,
        /// either or both collections when the associated exception is
        /// thrown.
        /// </remarks>
        Task<int> DrainToAsync(ICollection<T> items);

        /// <summary>
        /// Removes at most the given number of available elements from this queue and adds them to the given collection.
        /// </summary>
        /// <param name="items">the collection to transfer elements into</param>
        /// <param name="maxElements">the maximum number of elements to transfer</param>
        /// <returns>the number of elements transferred</returns>
        /// <remarks>
        /// A failure encountered while attempting to Add elements to
        /// collection <c>items</c> may result in elements being in neither,
        /// either or both collections when the associated exception is
        /// thrown.
        /// </remarks>
        Task<int> DrainToAsync(ICollection<T> items, int maxElements);

        /// <summary>
        /// Retrieves, but does not remove, the head of this queue, or returns <c>null</c> if this queue is empty.
        /// </summary>
        /// <returns>the head of this queue, or <c>null</c> if this queue is empty</returns>
        Task<T> PeekAsync();

        /// <summary>
        /// Retrieves, but does not remove, the head of this queue, or throws if this queue is empty.
        /// </summary>
        /// <returns>the head of this queue</returns>
        Task<T> GetElementAsync();

        /// <summary>
        /// Returns the number of additional elements that this queue can ideally
        /// (in the absence of memory or resource constraints) accept without
        /// blocking, or <see cref="int.MaxValue"/> if there is no intrinsic
        /// limit.
        /// </summary>
        /// <returns>the remaining capacity </returns>
        /// <remarks>
        /// <para>Note that you <em>cannot</em> always tell if an attempt to insert
        /// an element will succeed by inspecting <see cref="GetRemainingCapacityAsync"/>
        /// because it may be the case that another thread is about to
        /// insert or remove an element.
        /// </para>
        /// </remarks>
        Task<int> GetRemainingCapacityAsync();
    }
}
