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

namespace Hazelcast.IO.Serialization
{
    /// <summary>
    /// IIdentifiedDataSerializable is a serialization method as an alternative to standard serialization.
    /// It uses a factory to avoid reflection during deserialization.
    /// <see cref="IDataSerializableFactory">IDataSerializableFactory</see>
    /// </summary>
    /// <seealso cref="IPortable">IPortable</seealso>
    /// <seealso cref="IDataSerializableFactory">IDataSerializableFactory</seealso>
    public interface IIdentifiedDataSerializable
    {
        /// <summary>Reads fields from the input stream</summary>
        /// <param name="input">input</param>
        /// <exception cref="System.IO.IOException">System.IO.IOException</exception>
        void ReadData(IObjectDataInput input);

        /// <summary>Writes object fields to output stream</summary>
        /// <param name="output">output</param>
        /// <exception cref="System.IO.IOException">System.IO.IOException</exception>
        void WriteData(IObjectDataOutput output);

        /// <summary>Returns DataSerializableFactory factory id for this class.</summary>
        /// <remarks>Returns DataSerializableFactory factory id for this class.</remarks>
        /// <returns>factory id</returns>
        int GetFactoryId();

        /// <summary>Returns type identifier for this class.</summary>
        /// <remarks>Returns type identifier for this class. Id should be unique per DataSerializableFactory.
        /// 	</remarks>
        /// <returns>type id</returns>
        int GetId();
    }
}