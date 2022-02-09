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
    /// Specifies that a compact serializer should be generated at compile time for a type.
    /// </summary>
    /// <remarks>
    /// <para>This is identical to marking the actual class with the <see cref="CompactSerializableAttribute"/>.</para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public sealed class CompactSerializableTypeAttribute : Attribute
    {
        /// <summary>
        /// Specifies that a compact serializer should be generated at compile time for a type.
        /// </summary>
        /// <param name="serializedType">The serialized type.</param>
        public CompactSerializableTypeAttribute(Type serializedType)
        {
            SerializedType = serializedType ?? throw new ArgumentNullException(nameof(serializedType));
        }

        /// <summary>
        /// Gets the serialized type.
        /// </summary>
        public Type SerializedType { get; }
    }
}
