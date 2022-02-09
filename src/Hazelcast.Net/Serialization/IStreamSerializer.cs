﻿// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
    /// Defines a custom serializer that operates over <see cref="IObjectDataInput"/> and <see cref="IObjectDataOutput"/>.
    /// </summary>
    /// <typeparam name="T">The type of the serialized objects.</typeparam>
    public interface IStreamSerializer<T> : ISerializer
    {
        /// <summary>
        /// Reads an object.
        /// </summary>
        /// <param name="input">Input data.</param>
        /// <returns>The object.</returns>
        T Read(IObjectDataInput input);

        /// <summary>
        /// Writes an object.
        /// </summary>
        /// <param name="output">Output data.</param>
        /// <param name="obj">The object.</param>
        /// <returns><c>true</c> if, after the object has been serialized, the serialization service
        /// is ready and serialized data can be immediately sent to the cluster; <c>false</c> if the
        /// serialization service requires to complete some work before data can be sent to the
        /// cluster (i.e. <see cref="SerializationService.Meh"/> FIXME must be invoked).</returns>
        void Write(IObjectDataOutput output, T obj);
    }

    /// <summary>
    /// Defines a non-generic custom serializer that operates over <see cref="IObjectDataInput"/> and <see cref="IObjectDataOutput"/>.
    /// </summary>
    internal interface IStreamSerializer : IStreamSerializer<object>
    { }
}
