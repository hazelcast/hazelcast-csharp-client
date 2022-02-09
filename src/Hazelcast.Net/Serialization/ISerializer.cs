// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
    // NOTE
    //
    // ISerializer just provides a type-id = the identifier of the serialized type
    //   it is extended by:
    //     IByteArraySerializer<T> which reads/writes T objects from/to byte[]
    //     IStreamSerializer<T> which reads/writes T objects from/to IObjectDataInput/Output
    //
    // An ISerializer is what users need to implement, it is a public interface.
    // The implementations are generic so that they constrain the type that they can serialize.
    //
    //
    // The SerializationService ultimately works with plain objects, and IObjectDataInput/Output,
    // so it needs every serializer to be exposed as a plain IStreamSerializer, which is non-
    // generic, i.e. is IStreamSerializer<object>.
    //
    // The ISerializerAdapter interface is just IStreamSerializer with an added property to
    // expose the wrapped ISerializer, as that is convenient when registering serializers. It is
    // implemented by:
    //
    // - StreamSerializerAdapter<T> which wraps/adapts a IStreamSerializer<T>
    // - ByteArraySerializerAdapter<T> which wraps/adapts a IByteArraySerializer<T>

    /// <summary>
    /// Defines a custom serializer.
    /// </summary>
    public interface ISerializer : IDisposable
    {
        /// <summary>
        /// Gets the identifier of the serialized type.
        /// </summary>
        int TypeId { get; }
    }
}
