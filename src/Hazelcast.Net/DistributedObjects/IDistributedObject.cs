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

namespace Hazelcast.DistributedObjects
{
    /// <summary>
    /// Defines the base contract for all Hazelcast distributed objects.
    /// </summary>
    public interface IDistributedObject : IAsyncDisposable
    {
        /// <summary>
        /// Gets the name of the service managing this object.
        /// </summary>
        string ServiceName { get; }

        /// <summary>
        /// Gets the unique name of the object.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the partition key of this object.
        /// </summary>
        /// <returns>The partition key.</returns>
        /// <remarks>
        /// <para>The returned value has meaning only for non-partitioned data structures, such as
        /// IAtomicLong. For partitioned data structures such as <see cref="IHMap{TKey,TValue}"/>, the returned
        /// value is not null but has no meaning.</para>
        /// </remarks>
        string PartitionKey { get; }

        /// <summary>
        /// Destroys this distributed object.
        /// </summary>
        /// <returns>A task that will complete when the object has been destroyed.</returns>
        /// <remarks>
        /// <para>Destroying a distributed object completely deletes the object on the cluster.</para>
        /// </remarks>
        ValueTask DestroyAsync();
    }
}
