/*
* Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
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

using Hazelcast.Net.Ext;

namespace Hazelcast.IO.Serialization
{
    /// <summary>Data is basic unit of serialization.</summary>
    /// <remarks>
    /// Data is basic unit of serialization. It stores binary form of an object serialized
    /// </remarks>
    public interface IData
    {
        /// <summary>Returns byte array representation of internal binary format.</summary>
        /// <returns>binary data</returns>
        byte[] ToByteArray();

        /// <summary>Returns serialization type of binary form.</summary>
        /// <remarks>
        /// Returns serialization type of binary form. It's defined by
        /// <see cref="ISerializer.GetTypeId()"/>
        /// </remarks>
        /// <returns>serializer type id</returns>
        int GetTypeId();

        /// <summary>Returns the total size of Data in bytes</summary>
        /// <returns>total size</returns>
        int TotalSize();

        /// <summary>Returns size of internal binary data in bytes</summary>
        /// <returns>internal data size</returns>
        int DataSize();

        /// <summary>Returns approximate heap cost of this Data object in bytes.</summary>
        /// <returns>approximate heap cost</returns>
        int GetHeapCost();

        /// <summary>Returns partition hash calculated for serialized object.</summary>
        /// <remarks>
        /// Returns partition hash calculated for serialized object.
        /// Partition hash is used to determine partition of a Data and is calculated using
        /// <see cref="Hazelcast.Core.IPartitioningStrategy"/>
        /// during serialization.
        /// <p/>
        /// If partition hash is not set then standard <tt>hashCode()</tt> is used.
        /// </remarks>
        /// <returns>partition hash</returns>
        /// <seealso cref="Hazelcast.Core.IPartitionAware{T}"/>
        /// <seealso cref="Hazelcast.Core.IPartitioningStrategy"/>
        /// <seealso cref="SerializationService.ToData(object, Hazelcast.Core.IPartitioningStrategy)"/>
        int GetPartitionHash();

        /// <summary>Returns true if Data has partition hash, false otherwise.</summary>
        /// <returns>true if Data has partition hash, false otherwise.</returns>
        bool HasPartitionHash();

        /// <summary>
        /// Returns true if this Data is created from a
        /// <see cref="IPortable"/>
        /// object,
        /// false otherwise.
        /// </summary>
        /// <returns>true if source object is <tt>Portable</tt>, false otherwise.</returns>
        bool IsPortable();
    }
}
