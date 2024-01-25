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

namespace Hazelcast.CP;

/// <summary>
/// Represents a countdown latch which is a backed-up distributed alternative to the
/// java.util.concurrent.CountDownLatch. It is a cluster-wide synchronization aid
/// that allows one or more threads to wait until a set of operations being
/// performed in other threads completes.
/// It works on top of the Raft consensus algorithm. It offers linearizability
/// during crash failures and network partitions. It is CP with respect to the CAP
/// principle. If a network partition occurs, it remains available on at most one
/// side of the partition.
/// </summary>
public interface ICountDownLatch : ICPDistributedObject
{
    /// <summary>
    /// Waits until the latch has counted down to zero, or the specified timeout
    /// waiting time has expired.
    /// </summary>
    /// <param name="timeout">The wait timeout.</param>
    /// <returns>Whether the count reached zero within the specified timeout
    /// waiting time.</returns>
    Task<bool> AwaitAsync(TimeSpan timeout);

    /// <summary>
    /// Decrements the count of the latch.
    /// </summary>
    Task CountDownAsync();

    /// <summary>
    /// Gets the current count of the latch.
    /// </summary>
    /// <returns>The current count of the latch.</returns>
    Task<int> GetCountAsync();

    /// <summary>
    /// Sets the count to the specified value if it is zero.
    /// </summary>
    /// <param name="count">The new count.</param>
    /// <returns>Whether the count was set.</returns>
    Task<bool> TrySetCountAsync(int count);
}