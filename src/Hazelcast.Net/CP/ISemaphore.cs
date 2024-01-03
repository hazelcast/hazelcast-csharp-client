// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading.Tasks;
using Hazelcast.Core;

namespace Hazelcast.CP;

/// <summary>
/// Defines a CP distributed semaphore.
/// </summary>
/// 
/// <remarks>
/// 
/// <para>ISemaphore is a fault-tolerant distributed semaphore. Semaphores are often used
/// to restrict the number of threads than can access some physical or logical resource.</para>
/// <para>ISemaphore is a cluster-wide counting semaphore. Conceptually, it maintains a set
/// of permits. Permits are acquired with the <see cref="AcquireAsync"/> and <see cref="TryAcquireAsync"/>
/// functions, and released with the <see cref="ReleaseAsync"/> function. No actual permit objects
/// are used: the semaphore just keeps a count of the number available and acts accordingly.</para>
/// 
/// <para>Correct usage of a semaphore is established by programming convention in the
/// application. A semaphore is obtained via <code>client.CPSubsystem.GetSemaphoreAsync(name)</code>
/// and works on top of the Raft consensus algorithm. It offers linearizability during crash
/// failures and network partitions. It is CP with respect to the CAP principle. If a network
/// partition occurs, it remains available on at most one side of the partition.</para>
///
/// <para>There are two variations of the <see cref="ISemaphore"/> interface.</para>
///
/// <para>The default implementation is session-aware. When a caller interacts with the semaphore
/// for the first time, it starts a new CP session within the underlying CP group. Liveliness of
/// the session is then tracked via this CP session. Should the caller fails, permits acquired by
/// this client are automatically and safely released.</para>
/// <para>Note however that a session-aware semaphore cannot release permits that have not been
/// acquired beforehand; in other words it can only release previously acquired permits. It is
/// possible to acquire a permit on one lock context, and release it on another lock context
/// from the same client, but not from different clients.</para>
///
/// <para>The second impl offered by { @link CPSubsystem} is session-less. This implementation
/// does not perform auto-cleanup of acquired permits on failures. Acquired permits are not bound to
/// a client and permits can be released without being acquired first. This would be more
/// compatible with Java's semaphore release mode. It can be enabled by enabling JDK compatibility
/// when configuring the semaphore.</para>
/// <para>Note however that the user needs to handle failed permit owners on their own. If a server
/// or a client fails while holding some permits, they will not be automatically released.</para>
///
/// <para>There is a subtle difference between the lock and semaphore abstractions.</para>
/// <para>A lock can be assigned to at most one endpoint at a time, so we have a total order among
/// its holders. On the other hand, permits of a semaphore can be assigned to multiple endpoints
/// at a time, which implies that we may not have a total order among permit holders. In fact,
/// permit holders are partially ordered.</para>
/// 
/// </remarks>
public interface ISemaphore : ICPDistributedObject
{
    /// <summary>
    /// Tries to initialize this semaphore instance with the specified number of permits.
    /// </summary>
    /// <param name="permits">The number of permits.</param>
    /// <returns><c>true</c> if initialization succeeded; otherwise <c>false</c> (when already initialized).</returns>
    Task<bool> InitializeAsync(int permits = 1);

    /// <summary>
    /// Acquires permits immediately, if enough are available, and returns immediately.
    /// </summary>
    /// <remarks>
    /// <para>Reduces the number of available permits accordingly.</para>
    /// </remarks>
    /// <param name="permits">The number of permits to acquire.</param>
    Task AcquireAsync(int permits = 1);

    /// <summary>
    /// Tries to acquire permits, if enough are available, within the specified timeout duration.
    /// </summary>
    /// <param name="permits">The number of permits to acquire.</param>
    /// <param name="timeoutMs">The timeout duration, in milliseconds.</param>
    /// <returns><c>true</c> if the permits were acquired; otherwise <c>false</c>.</returns>
    Task<bool> TryAcquireAsync(int permits = 1, long timeoutMs = 0);

    /// <summary>
    /// Releases previously acquired permits.
    /// </summary>
    /// <param name="permits">The number of permits to release.</param>
    Task ReleaseAsync(int permits = 1);

    /// <summary>
    /// Gets the number of available permits.
    /// </summary>
    /// <returns>The number of available permits.</returns>
    Task<int> GetAvailablePermitsAsync();

    /// <summary>
    /// Acquires and returns all permits that are available.
    /// </summary>
    /// <returns>The number of permits that were drained.</returns>
    Task<int> DrainPermitsAsync();

    /// <summary>
    /// Reduces the number of available permits.
    /// </summary>
    /// <param name="delta">The number of permits to reduce.</param>
    /// <remarks>
    /// <para>This method differs from <see cref="AcquireAsync"/> as it does not block until
    /// permits become available. Similarly, if the caller has acquired some permits,
    /// they are not released with this call. In other words, this method arbitrarily
    /// decreases the available permits count (can even make it negative) without affecting
    /// the already acquired permits.</para>
    /// </remarks>
    Task ReducePermitsAsync(int delta);

    /// <summary>
    /// Increases the number of available permits.
    /// </summary>
    /// <param name="delta">The number of permits to increase.</param>
    /// <remarks>
    /// <para>If there are some threads waiting for permits to become available, they
    /// will be notified. Moreover, if the caller has acquired some permits,
    /// they are not released with this call. In other words, this method arbitrarily
    /// increases the available permits count and, if some acquire operations are pending,
    /// satisfies them.</para>
    /// </remarks>
    Task IncreasePermitsAsync(int delta);
}
