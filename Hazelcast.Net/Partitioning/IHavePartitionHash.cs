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

using Hazelcast.Partitioning.Strategies;

namespace Hazelcast.Partitioning
{
    /// <summary>
    /// Defines objects that provide their own partition hash.
    /// </summary>
    public interface IHavePartitionHash
    {
        /// <summary>Gets the partition hash of the serialized object.</summary>
        /// <remarks>
        /// <para>The partition hash is used to determine the partition of the data and is
        /// calculated using an <see cref="IPartitionAware"/> during serialization.</para>
        /// <para>If the partition hash is not set, then the standard hash code is used.</para>
        /// </remarks>
        /// <returns>The partition hash.</returns>
        int PartitionHash { get; }
    }
}
