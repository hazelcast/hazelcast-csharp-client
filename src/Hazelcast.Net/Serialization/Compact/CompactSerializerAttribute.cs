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
    /// Declares a compact serializer type.
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
        /// Declares a compact serializer type.
        /// </summary>
        /// <param name="serializerType">The compact serializer type.</param>
        public CompactSerializerAttribute(Type serializerType)
        {
            if (serializerType == null) throw new ArgumentNullException(nameof(serializerType));

            if (!serializerType.IsICompactSerializerOfTSerialized(out _))
                throw new ArgumentException("Type does not implement ICompactSerializer<TSerialized>.", nameof(serializerType));

            SerializerType = serializerType;
        }

        /// <summary>
        /// Gets the compact serializer type.
        /// </summary>
        public Type SerializerType { get; }
    }
}
