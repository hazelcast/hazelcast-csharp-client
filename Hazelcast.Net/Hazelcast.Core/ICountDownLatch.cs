// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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
    // TODO: missing async API
    /// <summary>
    ///     ICountDownLatch is a backed-up distributed alternative to the
    ///     .
    ///     <p />
    ///     ICountDownLatch is a cluster-wide synchronization aid
    ///     that allows one or more threads to wait until a set of operations being
    ///     performed in other threads completes.
    ///     <p />
    ///     There are a few differences compared to the
    ///     <see cref="ICountDownLatch">ICountDownLatch</see>
    ///     :
    ///     <ol>
    ///         <li>
    ///             the ICountDownLatch count can be re-set using
    ///             <see cref="TrySetCount(int)">TrySetCount(int)</see>
    ///             after a countdown
    ///             has finished but not during an active count. This allows the same latch instance to be reused.
    ///         </li>
    ///         <li>
    ///             there is no await() method to do an unbound wait since this is undesirable in a distributed
    ///             application: it can happen that for example a cluster is split or that the master and
    ///             replica's all die. So in most cases it is best to configure an explicit timeout so have the ability
    ///             to deal with these situations.
    ///         </li>
    ///     </ol>
    /// </summary>
    public interface ICountDownLatch : IDistributedObject
    {
        /// <summary>
        ///     Causes the current thread to wait until the latch has counted down to
        ///     zero, an exception is thrown, or the specified waiting time elapses.
        /// </summary>
        /// <remarks>
        ///     Causes the current thread to wait until the latch has counted down to
        ///     zero, an exception is thrown, or the specified waiting time elapses.
        ///     <p />
        ///     <p>
        ///         If the current count is zero then this method returns immediately
        ///         with the value
        ///         <code>true</code>
        ///         .
        ///     </p>
        ///     <p>
        ///         If the current count is greater than zero then the current
        ///         thread becomes disabled for thread scheduling purposes and lies
        ///         dormant until one of five things happen:
        ///         <ul>
        ///             <li>
        ///                 The count reaches zero due to invocations of the
        ///                 <see cref="CountDown()">CountDown()</see>
        ///                 method;
        ///             </li>
        ///             <li>
        ///                 This ICountDownLatch instance is destroyed;
        ///             </li>
        ///             <li>
        ///                 The countdown owner becomes disconnected;
        ///             </li>
        ///             <li>
        ///                 Some other thread
        ///                 the current thread; or
        ///             </li>
        ///             <li>
        ///                 The specified waiting time elapses.
        ///             </li>
        ///         </ul>
        ///     </p>
        ///     <p>
        ///         If the count reaches zero then the method returns with the
        ///         value
        ///         <code>true</code>
        ///         .
        ///     </p>
        ///     <p>
        ///         If the current thread:
        ///         <ul>
        ///             <li>
        ///                 has its interrupted status set on entry to this method; or
        ///             </li>
        ///             <li>
        ///                 is
        ///                 while waiting,
        ///             </li>
        ///         </ul>
        ///         then
        ///         <see cref="System.Exception">System.Exception</see>
        ///         is thrown and the current thread's
        ///         interrupted status is cleared.
        ///         <p>
        ///             If the specified waiting time elapses then the value
        ///             <code>false</code>
        ///             is returned.  If the time is less than or equal to zero, the method
        ///             will not wait at all.
        ///         </p>
        ///     </p>
        /// </remarks>
        /// <param name="timeout">the maximum time to wait</param>
        /// <param name="unit">
        ///     the time unit of the
        ///     <code>timeout</code>
        ///     argument
        /// </param>
        /// <returns>
        ///     <code>true</code>
        ///     if the count reached zero and
        ///     <code>false</code>
        ///     if the waiting time elapsed before the count reached zero
        /// </returns>
        /// <exception cref="System.Exception">if the current thread is interrupted</exception>
        /// <exception cref="System.InvalidOperationException">if hazelcast instance is shutdown while waiting</exception>
        bool Await(long timeout, TimeUnit unit);

        /// <summary>
        ///     Decrements the count of the latch, releasing all waiting threads if
        ///     the count reaches zero.
        /// </summary>
        /// <remarks>
        ///     Decrements the count of the latch, releasing all waiting threads if
        ///     the count reaches zero.
        ///     <p />
        ///     If the current count is greater than zero then it is decremented.
        ///     If the new count is zero:
        ///     <ul>
        ///         <li>
        ///             All waiting threads are re-enabled for thread scheduling purposes; and
        ///         </li>
        ///         <li>
        ///             Countdown owner is set to
        ///             <code>null</code>
        ///             .
        ///         </li>
        ///     </ul>
        ///     <p />
        ///     If the current count equals zero then nothing happens.
        /// </remarks>
        void CountDown();

        /// <summary>Returns the current count.</summary>
        /// <remarks>Returns the current count.</remarks>
        /// <returns>current count</returns>
        int GetCount();

        /// <summary>Sets the count to the given value if the current count is zero.</summary>
        /// <remarks>
        ///     Sets the count to the given value if the current count is zero.
        ///     <p />If count is not zero then this method does nothing and returns
        ///     <code>false</code>
        ///     .
        /// </remarks>
        /// <param name="count">
        ///     the number of times
        ///     <see cref="CountDown()">CountDown()</see>
        ///     must be invoked
        ///     before threads can pass through
        ///     <see cref="Await(long, TimeUnit)">Await(long, TimeUnit)</see>
        /// </param>
        /// <returns>
        ///     <code>true</code>
        ///     if the new count was set or
        ///     <code>false</code>
        ///     if the current
        ///     count is not zero
        /// </returns>
        /// <exception cref="System.ArgumentException">
        ///     if
        ///     <code>count</code>
        ///     is negative
        /// </exception>
        bool TrySetCount(int count);
    }
}