// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.IO.Serialization
{
    /// <summary>Portable provides an alternative serialization method.</summary>
    /// <remarks>
    /// Portable provides an alternative serialization method. Instead of relying on reflection, each Portable is
    /// created by a registere <see cref="IPortableFactory" />.
    /// <p />
    /// Portable serialization that have the following advantages:
    /// <ul>
    /// <li>Support multiversion of the same object type.
    /// (See
    /// <see cref="Hazelcast.Config.SerializationConfig.SetPortableVersion(int)" />
    /// )</li>
    /// <li>Fetching individual fields without having to rely on reflection.</li>
    /// <li>Querying and indexing support without de-serialization and/or reflection.</li>
    /// </ul>
    /// </remarks>
    /// <seealso cref="IPortableFactory">IPortableFactory</seealso>
    /// <seealso cref="IPortableWriter">IPortableWriter</seealso>
    /// <seealso cref="IPortableReader">IPortableReader</seealso>
    /// <seealso cref="IClassDefinition">IClassDefinition</seealso>
    /// <seealso cref="IIdentifiedDataSerializable">IIdentifiedDataSerializable</seealso>
    /// <seealso cref="Hazelcast.Config.SerializationConfig">Hazelcast.Config.SerializationConfig
    /// 	</seealso>
    public interface IPortable
    {
        /// <summary>Returns class identifier for this portable class.</summary>
        /// <remarks>Returns class identifier for this portable class. Class id should be unique per PortableFactory.
        /// 	</remarks>
        /// <returns>class id</returns>
        int GetClassId();

        /// <summary>Returns PortableFactory id for this portable class</summary>
        /// <returns>factory id</returns>
        int GetFactoryId();

        /// <summary>Read portable fields using PortableReader</summary>
        /// <param name="reader">PortableReader</param>
        /// <exception cref="System.IO.IOException">System.IO.IOException</exception>
        void ReadPortable(IPortableReader reader);

        /// <summary>Serialize this portable object using PortableWriter</summary>
        /// <param name="writer">PortableWriter</param>
        /// <exception cref="System.IO.IOException">System.IO.IOException</exception>
        void WritePortable(IPortableWriter writer);
    }
}