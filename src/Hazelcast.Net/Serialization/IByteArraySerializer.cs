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

namespace Hazelcast.Serialization
{
    /// <summary>
    /// Defines a generic custom serializer that operates over byte arrays.
    /// </summary>
    /// <typeparam name="T">The type of the serialized object.</typeparam>
    public interface IByteArraySerializer<T> : ISerializer
    {
        /// <summary>
        /// Reads an object.
        /// </summary>
        /// <param name="buffer">Input data.</param>
        /// <returns>The object.</returns>
        T Read(byte[] buffer);

        /// <summary>
        /// Writes an object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>The serialized object.</returns>
        byte[] Write(T obj);
    }
}
