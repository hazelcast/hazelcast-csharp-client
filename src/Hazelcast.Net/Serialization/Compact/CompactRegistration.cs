// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
    /// Represents a compact serializer registration.
    /// </summary>
    internal sealed class CompactRegistration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompactRegistration"/> class.
        /// </summary>
        /// <param name="serializedType">The serialized type.</param>
        /// <param name="serializer">The serializer for the serialized type.</param>
        /// <param name="schema">The optional schema for the serialized type.</param>
        /// <param name="typeName">The schema type name of the serialized type.</param>
        /// <param name="isClusterSchema">Whether the schema is considered to be, at configuration time, already known by the cluster.</param>
        private CompactRegistration(Type serializedType, CompactSerializerAdapter serializer, Schema? schema, string typeName, bool isClusterSchema)
        {
            // assume args are valid, *we* create the registrations

            SerializedType = serializedType;
            Serializer = serializer;
            Schema = schema;
            TypeName = typeName;
            IsClusterSchema = isClusterSchema;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompactRegistration"/> class.
        /// </summary>
        /// <param name="serializedType">The serialized type.</param>
        /// <param name="serializer">The serializer for the serialized type.</param>
        /// <param name="typeName">The schema type name of the serialized type.</param>
        /// <param name="isClusterSchema">Whether the schema is considered to be, at configuration time, already known by the cluster.</param>
        public CompactRegistration(Type serializedType, CompactSerializerAdapter serializer, string typeName, bool isClusterSchema)
            : this(serializedType, serializer, null, typeName, isClusterSchema)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompactRegistration"/> class.
        /// </summary>
        /// <param name="serializedType">The serialized type.</param>
        /// <param name="serializer">The serializer for the serialized type.</param>
        /// <param name="schema">The schema for the serialized type.</param>
        /// <param name="isClusterSchema">Whether the schema is considered to be, at configuration time, already known by the cluster.</param>
        public CompactRegistration(Type serializedType, CompactSerializerAdapter serializer, Schema schema, bool isClusterSchema)
            : this(serializedType, serializer, schema, schema.TypeName, isClusterSchema)
        { }

        /// <summary>
        /// Gets the schema type name of the serialized type.
        /// </summary>
        public string TypeName { get; }

        /// <summary>
        /// Gets the serialized type.
        /// </summary>
        public Type SerializedType { get; }

        /// <summary>
        /// Gets the schema, if any.
        /// </summary>
        public Schema? Schema { get; }

        /// <summary>
        /// Whether the registration provides a schema.
        /// </summary>
        public bool HasSchema => Schema != null;

        /// <summary>
        /// Gets the serializer for the serialized type, if any.
        /// </summary>
        public CompactSerializerAdapter Serializer { get; }

        /// <summary>
        /// Whether the schema is considered to be, at configuration time, already known by the cluster.
        /// </summary>
        public bool IsClusterSchema { get; }
    }
}
