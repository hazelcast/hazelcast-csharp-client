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

#nullable enable

using System;

namespace Hazelcast.Serialization.Compact
{
    /// <summary>
    /// Specifies a compact serializer type for a type.
    /// </summary>
    /// <remarks>
    /// <para>This attribute is used when a compact serializer is generated at compile time
    /// for a type which has been marked with the <see cref="CompactSerializableAttribute"/>
    /// or declared with the <see cref="CompactSerializableTypeAttribute"/>.</para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public sealed class CompactSerializerAttribute : Attribute
    {
        /// <summary>
        /// Specifies a compact serializer type for a type.
        /// </summary>
        /// <param name="serializedType">The serialized type.</param>
        /// <param name="serializerType">The compact serializer type for the <paramref name="serializedType"/>.</param>
        public CompactSerializerAttribute(Type serializedType, Type serializerType)
        {
            SerializedType = serializedType ?? throw new ArgumentNullException(nameof(serializedType));
            SerializerType = serializerType ?? throw new ArgumentNullException(nameof(serializerType));
        }

        /// <summary>
        /// Gets the serialized type.
        /// </summary>
        public Type SerializedType { get; }

        /// <summary>
        /// Gets the compact serializer type.
        /// </summary>
        public Type SerializerType { get; }
    }
}
