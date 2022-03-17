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

#nullable enable

using System;

namespace Hazelcast.Serialization.Compact
{
    /// <summary>
    /// Specifies that a compact serializer should be generated at compile time for the marked type.
    /// </summary>
    /// <remarks>
    /// <para>This is identical to using the assembly-level <see cref="CompactSerializableTypeAttribute"/>
    /// for the marked type.</para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class CompactSerializableAttribute : Attribute
    {
        /// <summary>
        /// Specifies that a compact serializer should be generated at compile time for the marked type.
        /// </summary>
        public CompactSerializableAttribute()
        { }

        /// <summary>
        /// Specifies that a compact serializer should be generated at compile time for the marked type.
        /// </summary>
        /// <param name="typeName">The type-name of the serialized type.</param>
        public CompactSerializableAttribute(string typeName)
        {
            TypeName = typeName;
        }

        /// <summary>
        /// Gets the type-name of the serialized type.
        /// </summary>
        public string? TypeName { get; }
    }
}
