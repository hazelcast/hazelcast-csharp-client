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
    /// Represents a compact serializer registration.
    /// </summary>
    internal sealed class CompactRegistration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompactRegistration"/> class.
        /// </summary>
        /// <param name="typeName">The schema type name of the serialized type.</param>
        /// <param name="type">The serialized type.</param>
        /// <param name="isClusterSchema">Whether the schema is considered to be, at configuration time, already known by the cluster.</param>
        public CompactRegistration(string typeName, Type type, bool isClusterSchema)
        {
            Schema = null;
            TypeName = typeName;
            Type = type;
            IsClusterSchema = isClusterSchema;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompactRegistration"/> class.
        /// </summary>
        /// <param name="schema">The schema for the serialized type.</param>
        /// <param name="type">The serialized type.</param>
        /// <param name="isClusterSchema">Whether the schema is considered to be, at configuration time, already known by the cluster.</param>
        public CompactRegistration(Schema schema, Type type, bool isClusterSchema)
        {
            Schema = schema;
            TypeName = schema.TypeName;
            Type = type;
            IsClusterSchema = isClusterSchema;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompactRegistration"/> class.
        /// </summary>
        /// <param name="typeName">The schema type name of the serialized type.</param>
        /// <param name="type">The serialized type.</param>
        /// <param name="serializer">The serializer for the serialized type.</param>
        /// <param name="isClusterSchema">Whether the schema is considered to be, at configuration time, already known by the cluster.</param>
        public CompactRegistration(string typeName, Type type, CompactSerializerWrapper serializer, bool isClusterSchema)
        {
            Schema = null;
            TypeName = typeName;
            Type = type;
            Serializer = serializer;
            IsClusterSchema = isClusterSchema;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompactRegistration"/> class.
        /// </summary>
        /// <param name="schema">The schema for the serialized type.</param>
        /// <param name="type">The serialized type.</param>
        /// <param name="serializer">The serializer for the serialized type.</param>
        /// <param name="isClusterSchema">Whether the schema is considered to be, at configuration time, already known by the cluster.</param>
        public CompactRegistration(Schema schema, Type type, CompactSerializerWrapper serializer, bool isClusterSchema)
        {
            Schema = schema;
            TypeName = schema.TypeName;
            Type = type;
            Serializer = serializer;
            IsClusterSchema = isClusterSchema;
        }

        /// <summary>
        /// Gets the schema type name of the serialized type.
        /// </summary>
        public string TypeName { get; }

        /// <summary>
        /// Gets the serialized type.
        /// </summary>
        public Type Type { get; }

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
        public CompactSerializerWrapper? Serializer { get; set; }

        /// <summary>
        /// Whether the registration provides a serializer.
        /// </summary>
        public bool HasSerializer => Serializer != null;

        /// <summary>
        /// Whether the schema is considered to be, at configuration time, already known by the cluster.
        /// </summary>
        public bool IsClusterSchema { get; }
    }
}
