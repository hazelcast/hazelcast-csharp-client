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

using Hazelcast.Transaction;

namespace Hazelcast.Core
{
    /// <summary>Base interface for all distributed objects.</summary>
    /// <remarks>Base interface for all distributed objects.</remarks>
    /// <seealso cref="IMap{K,V}">IMap&lt;K, V&gt;</seealso>
    /// <seealso cref="IQueue{E}">IQueue&lt;E&gt;</seealso>
    /// <seealso cref="IMultiMap{K,V}">IMultiMap&lt;K, V&gt;</seealso>
    /// <seealso cref="ITopic{E}">ITopic&lt;E&gt;</seealso>
    /// <seealso cref="ILock">ILock</seealso>
    /// <seealso cref="IExecutorService">IExecutorService</seealso>
    /// <seealso cref="ITransactionalMap{TKey,TValue}">ITransactionalMap&lt;K, V&gt;</seealso>
    /// <seealso cref="ITransactionalQueue{E}">ITransactionalQueue&lt;E&gt;</seealso>
    /// <seealso cref="ITransactionalMultiMap{K,V}">ITransactionalMultiMap&lt;K, V&gt;</seealso>
    public interface IDistributedObject
    {
        /// <summary>Destroys this object cluster-wide.</summary>
        /// <remarks>
        ///     Destroys this object cluster-wide.
        ///     Clears and releases all resources for this object.
        /// </remarks>
        void Destroy();

        /// <summary>Returns the unique name for this IDistributedObject.</summary>
        /// <remarks>Returns the unique name for this IDistributedObject. Returned value will never be null.</remarks>
        /// <value>the unique name for this object.</value>
        string Name { get; }

        /// <summary>Returns the key of partition this IDistributedObject is assigned to.</summary>
        /// <remarks>
        ///     Returns the key of partition this IDistributedObject is assigned to. The returned value only has meaning
        ///     for a non partitioned data-structure like an IAtomicLong. For a partitioned data-structure like an IMap
        ///     the returned value will not be null, but otherwise undefined.
        /// </remarks>
        /// <returns>the partition key.</returns>
        string GetPartitionKey();

        /// <summary>Returns the service name for this object.</summary>
        /// <remarks>Returns the service name for this object.</remarks>
        string ServiceName { get; }
    }
}