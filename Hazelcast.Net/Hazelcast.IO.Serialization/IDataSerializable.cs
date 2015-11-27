// Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.IO.Serialization
{
    /// <summary>DataSerializable is a serialization method as an alternative to standard serialization.
    /// 	</summary>
    /// <remarks>
    /// DataSerializable is a serialization method as an alternative to standard serialization.
    /// </remarks>
    /// <seealso cref="IIdentifiedDataSerializable">IIdentifiedDataSerializable</seealso>
    /// <seealso cref="IPortable">IPortable</seealso>
    /// <seealso cref="IVersionedPortable">IVersionedPortable</seealso>
    [Obsolete("IDataSerializable is no longer supported in the .NET client. Please use IIdentifiedDataSerializable instead.")]
    public interface IDataSerializable
    {
        /// <summary>
        /// Get Server Java Impl class full name
        /// </summary>
        /// <returns>full java class name</returns>
        string GetJavaClassName();

        /// <summary>Reads fields from the input stream</summary>
        /// <param name="input">input</param>
        /// <exception cref="System.IO.IOException">System.IO.IOException</exception>
        void ReadData(IObjectDataInput input);

        /// <summary>Writes object fields to output stream</summary>
        /// <param name="output">output</param>
        /// <exception cref="System.IO.IOException">System.IO.IOException</exception>
        void WriteData(IObjectDataOutput output);
    }
}
