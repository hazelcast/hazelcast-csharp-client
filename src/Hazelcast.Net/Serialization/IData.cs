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

namespace Hazelcast.Serialization
{
    /// <summary>
    /// Represents the basic unit of serialization.
    /// </summary>
    internal interface IData
    {
        /// <summary>Gets the size of the data contained in this instance.</summary>
        int DataSize { get; }

        /// <summary>Gets the total size of this instance in bytes.</summary>
        int TotalSize { get; }

        /// <summary>Gets the approximate heap cost of this instance in bytes.</summary>
        int HeapCost { get; }

        /// <summary>Returns serialization type of binary form.</summary>
        /// <remarks>
        /// Returns serialization type of binary form. It's defined by
        /// <see cref="ISerializer.GetTypeId()"/>
        /// </remarks>
        /// <returns>serializer type id</returns>
        int TypeId { get; }

        /// <summary>Determines whether this instance has a partition hash.</summary>
        bool HasPartitionHash { get; }

        /// <summary>
        /// Gets the partition hash.
        /// </summary>
        /// <returns>The partition hash.</returns>
        int PartitionHash { get; }

        /// <summary>Determines whether this instance was created from an <see cref="IPortable{T}"/> instance.</summary>
        bool IsPortable { get; }

        /// <summary>Gets the byte array representation of this instance.</summary>
        /// <remarks>Can return an empty array but never <c>null</c>.</remarks>
        byte[] ToByteArray();
    }
}
