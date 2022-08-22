// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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

using Hazelcast.Partitioning.Strategies;

namespace Hazelcast.Serialization
{
    internal partial class SerializationService
    {
        /// <summary>
        /// Calculates the partition hash of an object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="strategy">An optional strategy.</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>If no <paramref name="strategy"/> is provided, use the <see cref="_globalPartitioningStrategy"/>.</para>
        /// </remarks>
        internal int CalculatePartitionHash(object obj, IPartitioningStrategy strategy)
        {
            // strategy: obj -> partitionKey
            var partitioningStrategy = strategy ?? _globalPartitioningStrategy;
            var partitionKey = partitioningStrategy?.GetPartitionKey(obj);

            // returning 0 here means that we're going to create a HeapData that:
            // - has HasPartitionHash == false
            // - returns PartitionHash = GetHashCode() which is overriden with a Murmur3 hasher

            if (partitionKey is null) return 0; // no partition key
            if (partitionKey == obj) return 0; // obj is *not* IData (else we wouldn't be here), so this is a dead end

            // fast: partitionKey *is* a hash
            // TODO: consider implementing this?
            //if (partitionKey is int hash) return hash;

            // fast: partitionKey provides a hash
            if (partitionKey is IData data) return data.PartitionHash;

            // slow: partitionKey wants to be hashed
            //   create a HeapData with NullPartitioningStrategy => CalculatePartitionHash returns zero,
            //   HasPartitionHash is false, and therefore PartitionHash is obtained by Murmur3-hashing
            //   the serialized partitionKey bytes - i.e. "hashing partitionKey"
            var slow = ToData(partitionKey, NullPartitioningStrategy);
            if (slow != null) return slow.PartitionHash;

            // duh - should never happen, really
            return 0;

            // NOTE: this is an exact copy of the Java client algorithm and in case partitionKey is e.g. a string,
            // it probably is not especially efficient (allocates a new HeapData etc) - it would probably be better
            // for the partition strategy to return a dummy IData with the pre-computed hash...
        }
    }
}
