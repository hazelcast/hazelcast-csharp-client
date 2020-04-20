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
    /// For sample usage custom serialization and other way of custom serialization
    /// see
    /// <see cref="IStreamSerializer{T}">IStreamSerializer&lt;T&gt;</see>
    /// .
    /// Note that read and write methods should be compatible
    /// </summary>
    public interface IByteArraySerializer<T> : ISerializer
    {
        /// <summary>Converts given byte array to object</summary>
        /// <param name="buffer">that object will be read from</param>
        /// <returns>deserialized object</returns>
        /// <exception cref="System.IO.IOException">System.IO.IOException</exception>
        T Read(byte[] buffer);

        /// <summary>Converts given object to byte array</summary>
        /// <param name="obj">that will be serialized</param>
        /// <returns>byte array that object is serialized into</returns>
        /// <exception cref="System.IO.IOException">System.IO.IOException</exception>
        byte[] Write(T obj);
    }
}