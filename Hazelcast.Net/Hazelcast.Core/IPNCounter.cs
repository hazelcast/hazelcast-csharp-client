/*
 * Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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

using System;
using Hazelcast.Client.Spi;

namespace Hazelcast.Core
{
    /// <summary>
    /// PN (Positive-Negative) CRDT counter.
    /// </summary>
    /// <remarks>
    /// The counter supports adding and subtracting values as well as retrieving the current counter value.
    /// Each replica of this counter can perform operations locally without coordination with the other replicas, 
    /// thus increasing availability.
    /// The counter guarantees that whenever two nodes have received the same set of updates, 
    /// possibly in a different order, their state is identical, and any conflicting updates are merged automatically.
    /// If no new updates are made to the shared state, all nodes that can communicate will eventually have the same data.
    ///
    /// The updates to this counter are applied locally when invoked on a CRDT replica. 
    /// A replica can be any hazelcast instance which is not a client or a lite member. 
    /// The number of replicas in the cluster is determined by the PNCounterConfig configuration value.
    ///
    /// When invoking updates from non-replica instance, the invocation is remote.
    /// This may lead to indeterminate state - the update may be applied but the response has not been received.
    /// In this case, the caller will be notified with a <exception cref="TargetDisconnectedException"/> when invoking from a client.
    ///
    /// The read and write methods provide monotonic read and RYW(read - your - write) guarantees.
    /// These guarantees are session guarantees which means that if no replica with the previously observed state is reachable,
    /// the session guarantees are lost and the method invocation will throw a <exception cref="ConsistencyLostException"/>. 
    /// This does not mean that an update is lost.All of the updates are part of some replica 
    /// and will be eventually reflected in the state of all other replicas. This exception just means 
    /// that you cannot observe your own writes because all replicas that contain your updates are currently unreachable.
    /// After you have received a <exception cref="ConsistencyLostException"/>, you can either
    /// wait for a sufficiently up - to - date replica to become reachable in which
    /// case the session can be continued or you can reset the session by calling the <see cref="Reset()"/> method.
    /// If you have called the <see cref="Reset()"/> method, a new session is started with the next invocation 
    /// to a CRDT replica.
    /// <b> NOTE:</b>
    /// The CRDT state is kept entirely on non-lite(data) members. 
    /// If there aren't any and the methods here are invoked on a lite member, they will fail with an <exception cref="NoDataMemberInClusterException"/>
    /// </remarks>
    public interface IPNCounter : IDistributedObject
    {
        /// <summary>
        /// Returns the current value of the counter.
        /// </summary>
        /// <returns>The current value</returns>
        /// <exception cref="NoDataMemberInClusterException">if the cluster does not contain any data members</exception>
        /// <exception cref="NotSupportedException">if the cluster version is less than 3.10</exception>
        /// <exception cref="ConsistencyLostException">if the session guarantees have been lost</exception>
        long Get();

        /// <summary>
        /// Adds the given value to the current value.
        /// </summary>
        /// <param name="delta">the value to add</param>
        /// <returns>The previous value</returns>
        /// <exception cref="NoDataMemberInClusterException">if the cluster does not contain any data members</exception>
        /// <exception cref="UnsupportedOperationException">if the cluster version is less than 3.10</exception>
        /// <exception cref="ConsistencyLostException">if the session guarantees have been lost</exception>
        long GetAndAdd(long delta);

        /// <summary>
        /// Adds the given value to the current value
        /// </summary>
        /// <param name="delta">the value to add</param>
        /// <returns>the updated value</returns>
        /// <exception cref="NoDataMemberInClusterException">if the cluster does not contain any data members</exception>
        /// <exception cref="UnsupportedOperationException">if the cluster version is less than 3.10</exception>
        /// <exception cref="ConsistencyLostException">if the session guarantees have been lost</exception>
        long AddAndGet(long delta);

        /// <summary>
        /// Subtracts the given value from the current value.
        /// </summary>
        /// <param name="delta">the value to add</param>
        /// <returns>the previous value</returns>
        /// <exception cref="NoDataMemberInClusterException">if the cluster does not contain any data members</exception>
        /// <exception cref="UnsupportedOperationException">if the cluster version is less than 3.10</exception>
        /// <exception cref="ConsistencyLostException">if the session guarantees have been lost</exception>
        long GetAndSubtract(long delta);

        /// <summary>
        /// Subtracts the given value from the current value.
        /// </summary>
        /// <param name="delta">the value to subtract</param>
        /// <returns>the updated value</returns>
        /// <exception cref="NoDataMemberInClusterException">if the cluster does not contain any data members</exception>
        /// <exception cref="UnsupportedOperationException">if the cluster version is less than 3.10</exception>
        /// <exception cref="ConsistencyLostException">if the session guarantees have been lost</exception>
        long SubtractAndGet(long delta);

        /// <summary>
        /// Decrements by one the current value.
        /// </summary>
        /// <returns>the updated value</returns>
        /// <exception cref="NoDataMemberInClusterException">if the cluster does not contain any data members</exception>
        /// <exception cref="UnsupportedOperationException">if the cluster version is less than 3.10</exception>
        /// <exception cref="ConsistencyLostException">if the session guarantees have been lost</exception>
        long DecrementAndGet();

        /// <summary>
        /// Increments by one the current value.
        /// </summary>
        /// <returns>the updated value</returns>
        /// <exception cref="NoDataMemberInClusterException">if the cluster does not contain any data members</exception>
        /// <exception cref="UnsupportedOperationException">if the cluster version is less than 3.10</exception>
        /// <exception cref="ConsistencyLostException">if the session guarantees have been lost</exception>
        long IncrementAndGet();

        /// <summary>
        /// Decrements by one the current value.
        /// </summary>
        /// <returns>the previous value</returns>
        /// <exception cref="NoDataMemberInClusterException">if the cluster does not contain any data members</exception>
        /// <exception cref="UnsupportedOperationException">if the cluster version is less than 3.10</exception>
        /// <exception cref="ConsistencyLostException">if the session guarantees have been lost</exception>
        long GetAndDecrement();

        /// <summary>
        /// Increments by one the current value.
        /// </summary>
        /// <returns>the previous value</returns>
        /// <exception cref="NoDataMemberInClusterException">if the cluster does not contain any data members</exception>
        /// <exception cref="UnsupportedOperationException">if the cluster version is less than 3.10</exception>
        /// <exception cref="ConsistencyLostException">if the session guarantees have been lost</exception>
        long GetAndIncrement();

        /// <summary>
        /// Resets the observed state by this PN counter. 
        /// This method may be used after a method invocation has thrown a <exception cref="ConsistencyLostException"/>
        /// to reset the proxy and to be able to start a new session.
        /// </summary>
        void Reset();
    }
}

