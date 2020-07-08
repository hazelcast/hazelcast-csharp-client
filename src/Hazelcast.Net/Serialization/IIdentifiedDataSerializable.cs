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

using System;

namespace Hazelcast.Serialization
{
    /// <summary>
    /// Defines the interface that classes of objects can implement to take control of their
    /// own serialization.
    /// </summary>
    /// <remarks>
    /// <para>Classes that implement <see cref="IIdentifiedDataSerializable"/> rely on a declared
    /// factory to create instances, and deserialize fields, thus avoiding the costs otherwise
    /// associated with reflection.</para>
    /// </remarks>
    public interface IIdentifiedDataSerializable // FIXME name? ISerializationHandler
    {
        /// <summary>
        /// Deserializes the object by reading from an <see cref="IObjectDataInput"/>.
        /// </summary>
        /// <param name="input">The input serialized data.</param>
        void ReadData(IObjectDataInput input);

        /// <summary>
        /// Serializes the object by writing to an <see cref="IObjectDataOutput"/>.
        /// </summary>
        /// <param name="output">The output serialized data.</param>
        void WriteData(IObjectDataOutput output);

        /*
        /// <summary>Returns DataSerializableFactory factory id for this class.</summary>
        /// <remarks>Returns DataSerializableFactory factory id for this class.</remarks>
        /// <returns>factory id</returns>
        */
        /// <summary>
        /// Gets the identifier of the <see cref="IDataSerializableFactory"/> that can create instances of the class.
        /// </summary>
        /// <returns>The identifier of the factory.</returns>
        int FactoryId { get; }

        /// <summary>
        /// Gets the identifier of the class.
        /// </summary>
        /// <returns>The identifier of the class.</returns>
        /// <remarks>
        /// <para>The identifier is used to uniquely identify the class, i.e. the <see cref="Type"/>,
        /// so that the corresponding <see cref="IDataSerializableFactory"/> can re-create the
        /// proper instances. The identifier should therefore be unique per factory.</para>
        /// </remarks>
        int ClassId { get; }
    }
}
