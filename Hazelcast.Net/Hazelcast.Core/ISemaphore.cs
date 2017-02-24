// Copyright (c) 2008, Hazelcast, Inc. All Rights Reserved.
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

using System.Threading;

namespace Hazelcast.Core
{
    /// <summary>
    ///     ISemaphore is a backed-up distributed alternative to the
    ///     <see cref="System.Threading.Semaphore">System.Threading.Semaphore</see>
    ///     .
    /// </summary>
    /// <remarks>
    ///     <p />
    ///     ISemaphore is a cluster-wide counting semaphore.  Conceptually,
    ///     it maintains a set of permits.  Each
    ///     <see cref="Acquire()">Acquire()</see>
    ///     blocks if necessary until
    ///     a permit is available, and then takes it.  Each
    ///     <see cref="Release()">Release()</see>
    ///     adds a permit,
    ///     potentially releasing a blocking acquirer. However, no actual permit objects are
    ///     used; the semaphore just keeps a count of the number available and acts accordingly.
    ///     <p/>The Hazelcast distributed semaphore implementation guarantees that
    ///     threads invoking any of the
    ///     <see cref="Acquire()">acquire</see>
    ///     methods are selected
    ///     to obtain permits in the order in which their invocation of those methods
    ///     was processed(first-in-first-out; FIFO).  Note that FIFO ordering necessarily
    ///     applies to specific internal points of execution within the cluster.  So,
    ///     it is possible for one member to invoke
    ///     <c>acquire</c>
    ///     before another, but reach
    ///     the ordering point after the other, and similarly upon return from the method.
    ///     <p />This class also provides convenience methods to
    ///     <see cref="Acquire(int)">acquire</see>
    ///     and
    ///     <see cref="Release(int)">release</see>
    ///     multiple
    ///     permits at a time.  Beware of the increased risk of indefinite
    ///     postponement when using the multiple acquire.  If a single permit is
    ///     released to a semaphore that is currently blocking, a thread waiting
    ///     for one permit will acquire it before a thread waiting for multiple
    ///     permits regardless of the call order.
    ///     <p />
    ///     <ul>
    ///         <li>Correct usage of a semaphore is established by programming convention in the application.</li>
    ///     </ul>
    /// </remarks>
    public interface ISemaphore : IDistributedObject
    {
        /// <summary>
        ///         Acquires a permit, if one is available and returns immediately,
        ///         reducing the number of available permits by one.
        /// </summary>
        /// <remarks>
        ///         Acquires a permit, if one is available and returns immediately,
        ///         reducing the number of available permits by one.
        ///         <p />
        ///             If no permit is available then the current thread becomes
        ///             disabled for thread scheduling purposes and lies dormant until
        ///             one of two things happens:
        ///             <ul>
        ///                 <li>
        ///                     Some other thread invokes one of the
        ///                     <see cref="Release()">Release()</see>
        ///                     methods for this
        ///                     semaphore and the current thread is next to be assigned a permit;
        ///                 </li>
        ///                 <li>
        ///                     This ISemaphore instance is destroyed; or
        ///                 </li>
        ///             </ul>
        /// </remarks>
        /// <exception cref="System.InvalidOperationException">if hazelcast instance is shutdown while waiting</exception>
        void Acquire();

        /// <summary>
        ///         Acquires the given number of permits, if they are available,
        ///         and returns immediately, reducing the number of available permits
        ///         by the given amount.
        /// </summary>
        /// <remarks>
        ///         Acquires the given number of permits, if they are available,
        ///         and returns immediately, reducing the number of available permits
        ///         by the given amount.
        ///         <p />
        ///             If insufficient permits are available then the current thread becomes
        ///             disabled for thread scheduling purposes and lies dormant until
        ///             one of two things happens:
        ///             <ul>
        ///                 <li>
        ///                     Some other thread invokes one of the
        ///                     <see cref="Release()">release</see>
        ///                     methods for this semaphore, the current thread is next to be assigned
        ///                     permits and the number of available permits satisfies this request;
        ///                 </li>
        ///                 <li>
        ///                     This ISemaphore instance is destroyed; 
        ///                 </li>
        ///            </ul>
        /// </remarks>
        /// <param name="permits">the number of permits to acquire</param>
        /// <exception cref="System.ArgumentException">
        ///     if
        ///     <c>permits</c>
        ///     is negative
        /// </exception>
        /// <exception cref="System.InvalidOperationException">if hazelcast instance is shutdown while waiting</exception>
        void Acquire(int permits);

        /// <summary>Returns the current number of permits currently available in this semaphore.</summary>
        /// <remarks>
        ///     Returns the current number of permits currently available in this semaphore.
        ///     <p />
        ///     <ul>
        ///         <li>This method is typically used for debugging and testing purposes.</li>
        ///     </ul>
        /// </remarks>
        /// <returns>the number of permits available in this semaphore</returns>
        int AvailablePermits();

        /// <summary>Acquires and returns all permits that are immediately available.</summary>
        /// <remarks>Acquires and returns all permits that are immediately available.</remarks>
        /// <returns>the number of permits drained</returns>
        int DrainPermits();

        ///// <summary>Returns the name of this ISemaphore instance.</summary>
        ///// <remarks>Returns the name of this ISemaphore instance.</remarks>
        ///// <returns>name of this instance</returns>
        //string GetName();

        /// <summary>Try to initialize this ISemaphore instance with given permit count</summary>
        /// <returns>true if initialization success</returns>
        bool Init(int permits);

        /// <summary>
        ///     Shrinks the number of available permits by the indicated
        ///     reduction.
        /// </summary>
        /// <remarks>
        ///     Shrinks the number of available permits by the indicated
        ///     reduction. This method differs from
        ///     <c>acquire</c>
        ///     in that it does not
        ///     block waiting for permits to become available.
        /// </remarks>
        /// <param name="reduction">the number of permits to remove</param>
        /// <exception cref="System.ArgumentException">
        ///     if
        ///     <c>reduction</c>
        ///     is negative
        /// </exception>
        void ReducePermits(int reduction);

        /// <summary>
        ///     Releases a permit, increasing the number of available permits by
        ///     one.
        /// </summary>
        /// <remarks>
        ///     Releases a permit, increasing the number of available permits by
        ///     one.  If any threads in the cluster are trying to acquire a permit,
        ///     then one is selected and given the permit that was just released.
        ///     <p />
        ///     There is no requirement that a thread that releases a permit must
        ///     have acquired that permit by calling one of the
        ///     <see cref="Acquire()">acquire</see>
        ///     methods.
        ///     Correct usage of a semaphore is established by programming convention
        ///     in the application.
        /// </remarks>
        void Release();

        /// <summary>
        ///     Releases the given number of permits, increasing the number of
        ///     available permits by that amount.
        /// </summary>
        /// <remarks>
        ///     Releases the given number of permits, increasing the number of
        ///     available permits by that amount.
        ///     <p />
        ///     There is no requirement that a thread that releases a permit must
        ///     have acquired that permit by calling one of the
        ///     <see cref="Acquire()">acquire</see>
        ///     methods.
        ///     Correct usage of a semaphore is established by programming convention
        ///     in the application.
        /// </remarks>
        /// <param name="permits">the number of permits to release</param>
        /// <exception cref="System.ArgumentException">
        ///     if
        ///     <c>permits</c>
        ///     is negative
        /// </exception>
        void Release(int permits);

        /// <summary>
        ///     Acquires a permit, if one is available and returns immediately,
        ///     with the value  <c>true</c>, reducing the number of available permits by one.
        /// </summary>
        /// <remarks>
        ///     Acquires a permit, if one is available and returns immediately,
        ///     with the value  <c>true</c>, reducing the number of available permits by one.
        ///     <p />
        ///     If no permit is available then this method will return
        ///     immediately with the value <c>false</c>.
        /// </remarks>
        /// <returns>
        ///     <c>true</c>
        ///     if a permit was acquired and
        ///     <c>false</c>
        ///     otherwise
        /// </returns>
        bool TryAcquire();

        /// <summary>
        ///     Acquires the given number of permits, if they are available, and
        ///     returns immediately, with the value <c>true</c>,
        ///     reducing the number of available permits by the given amount.
        /// </summary>
        /// <remarks>
        ///     Acquires the given number of permits, if they are available, and
        ///     returns immediately, with the value <c>true</c>,
        ///     reducing the number of available permits by the given amount.
        /// <p/>
        ///        If insufficient permits are available then this method will return
        ///        immediately with the value
        ///        <c>false</c>
        ///        and the number of available
        ///        permits is unchanged.
        /// </remarks>
        /// <param name="permits">the number of permits to acquire</param>
        /// <returns>
        ///     <c>true</c>
        ///     if the permits were acquired and
        ///     <c>false</c>
        ///     otherwise
        /// </returns>
        /// <exception cref="System.ArgumentException">
        ///     if
        ///     <c>permits</c>
        ///     is negative
        /// </exception>
        bool TryAcquire(int permits);

        /// <summary>
        ///     Acquires a permit from this semaphore, if one becomes available
        ///     within the given waiting time and the current thread has not
        ///     been <see cref="Thread.Interrupt()">interrupted</see>.
        /// </summary>
        /// <remarks>
        ///     Acquires a permit from this semaphore, if one becomes available
        ///     within the given waiting time and the current thread has not
        ///     been <see cref="Thread.Interrupt()">interrupted</see>.
        ///     <p />
        ///     Acquires a permit, if one is available and returns immediately,
        ///     with the value <c>true</c>, reducing the number of available permits by one.
        ///     <p />
        ///     If no permit is available then the current thread becomes
        ///     disabled for thread scheduling purposes and lies dormant until
        ///     one of three things happens:
        ///     <ul>
        ///         <li>
        ///             Some other thread invokes the
        ///             <see cref="Release()">Release()</see>
        ///             method for this
        ///             semaphore and the current thread is next to be assigned a permit; or
        ///         </li>
        ///         <li>
        ///             Some other thread
        ///             <see cref="Thread.Interrupt()">interrupts</see>
        ///             the current thread; or
        ///         </li>
        ///         <li>The specified waiting time elapses.</li>
        ///     </ul>
        ///     <p />
        ///     If a permit is acquired then the value <c>true</c> is returned.
        ///     <p />
        ///     If the specified waiting time elapses then the value
        ///     <c>false</c>
        ///     is returned.  If the time is less than or equal to zero, the method
        ///     will not wait at all.
        ///     <p />
        ///         If the current thread is <see cref="Thread.Interrupt()">interrupted</see>
        ///         while waiting for a permit,
        ///         then <see cref="System.Exception">System.Exception</see>
        ///         is thrown and the current thread's
        ///         interrupted status is cleared.
        /// </remarks>
        /// <param name="timeout">the maximum time to wait for a permit</param>
        /// <param name="unit">
        ///     the time unit of the
        ///     <c>timeout</c>
        ///     argument
        /// </param>
        /// <returns>
        ///     <c>true</c>
        ///     if a permit was acquired and
        ///     <c>false</c>
        ///     if the waiting time elapsed before a permit was acquired
        /// </returns>
        /// <exception cref="System.Exception">if the current thread is interrupted</exception>
        /// <exception cref="System.InvalidOperationException">if hazelcast instance is shutdown while waiting</exception>
        bool TryAcquire(long timeout, TimeUnit unit);

        /// <summary>
        ///     Acquires the given number of permits, if they are available and
        ///     returns immediately, with the value
        ///     <c>true</c>
        ///     ,
        ///     reducing the number of available permits by the given amount.
        /// </summary>
        /// <remarks>
        ///     Acquires the given number of permits, if they are available and
        ///     returns immediately, with the value
        ///     <c>true</c> , reducing the number of available permits by the given amount.
        ///     <p />
        ///     If insufficient permits are available then
        ///     the current thread becomes disabled for thread scheduling
        ///     purposes and lies dormant until one of three things happens:
        ///     <ul>
        ///         <li>
        ///             Some other thread invokes the
        ///             <see cref="Release()">Release()</see>
        ///             method for this
        ///             semaphore and the current thread is next to be assigned a permit; or
        ///         </li>
        ///         <li>
        ///             Some other thread
        ///             <see cref="Thread.Interrupt()">interrupts</see>
        ///             the current thread; or
        ///         </li>
        ///         <li>The specified waiting time elapses.</li>
        ///     </ul>
        ///     <p />
        ///     If the permits are acquired then the value
        ///     <c>true</c>
        ///     is returned.
        ///     <p />
        ///     If the specified waiting time elapses then the value
        ///     <c>false</c>
        ///     is returned.  If the time is less than or equal to zero, the method
        ///     will not wait at all.
        ///     <p />
        ///     <p>
        ///         If the current thread is <see cref="Thread.Interrupt()">interrupted</see>
        ///         while waiting for a permit,
        ///         then <see cref="System.Exception">System.Exception</see>
        ///         is thrown and the current thread's
        ///         interrupted status is cleared.
        ///     </p>
        /// </remarks>
        /// <param name="permits">the number of permits to acquire</param>
        /// <param name="timeout">the maximum time to wait for the permits</param>
        /// <param name="unit">
        ///     the time unit of the
        ///     <c>timeout</c>
        ///     argument
        /// </param>
        /// <returns>
        ///     <c>true</c>
        ///     if all permits were acquired and
        ///     <c>false</c>
        ///     if the waiting time elapsed before all permits could be acquired
        /// </returns>
        /// <exception cref="System.Exception">if the current thread is interrupted</exception>
        /// <exception cref="System.ArgumentException">
        ///     if
        ///     <c>permits</c>
        ///     is negative
        /// </exception>
        /// <exception cref="System.InvalidOperationException">if hazelcast instance is shutdown while waiting</exception>
        bool TryAcquire(int permits, long timeout, TimeUnit unit);
    }
}