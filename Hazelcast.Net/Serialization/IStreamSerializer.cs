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
    /// <summary>A base class for custom serialization.</summary>
    /// <remarks>
    /// A base class for custom serialization. User can register custom serializer.
    /// </remarks>
    public interface IStreamSerializer<T> : ISerializer
    {
        /// <summary>Reads object from objectDataInputStream</summary>
        /// <param name="input">ObjectDataInput stream that object will read from</param>
        /// <returns>read object</returns>
        /// <exception cref="System.IO.IOException">in case of failure to read</exception>
        T Read(IObjectDataInput input);

        /// <summary>This method writes object to ObjectDataOutput</summary>
        /// <param name="output">ObjectDataOutput stream that object will be written to</param>
        /// <param name="obj">that will be written to out</param>
        /// <exception cref="System.IO.IOException">in case of failure to write</exception>
        void Write(IObjectDataOutput output, T obj);
    }
}