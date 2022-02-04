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
using System.Collections.Generic;
using Hazelcast.Exceptions;

#nullable enable

namespace Hazelcast.Serialization.Compact
{
    /// <summary>
    /// Represents the compact serialization options.
    /// </summary>
    public sealed class CompactOptions
    {
        // note: these are very temporary for the compact serialization preview
        // and compact options are expected to be refactored at some point in time.
        private readonly HashSet<string> _names = new HashSet<string>();
        private readonly HashSet<Type> _types = new HashSet<Type>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CompactOptions"/> class.
        /// </summary>
        public CompactOptions()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompactOptions"/> class.
        /// </summary>
        private CompactOptions(CompactOptions other)
        {
            Enabled = other.Enabled;
            _names = new HashSet<string>(other._names);
            _types = new HashSet<Type>(other._types);
            Registrations = new List<Registration>(other.Registrations);
        }

        /// <summary>
        /// (preview) Whether compact serialization is enabled.
        /// </summary>
        /// <remarks>
        /// <para>During the preview period, compact serialization is not enabled by default.</para>
        /// </remarks>
        public bool Enabled { get; set; }

        /// <summary>
        /// Registers a type to be compact-serialized.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="schema">The schema corresponding to the type.</param>
        /// <param name="serializer">The compact serializer.</param>
        public void Register<T>(Schema schema, ICompactSerializer<T> serializer)
        {
            if (schema == null) throw new ArgumentNullException(nameof(schema));
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));

            // 1 unique registration per type-name
            if (_names.Contains(schema.TypeName))
                throw new ArgumentException($"A type with type name {schema.TypeName} has already been registered.", nameof(schema));

            // 1 unique registration per type
            if (_types.Contains(typeof(T)))
                throw new ArgumentException($"A serializer for type {typeof(T)} has already been registered.", nameof(serializer));

            _names.Add(schema.TypeName);
            _types.Add(typeof (T));

            var registration = Registration.New(schema, typeof (T), CompactSerializerWrapper.Create(serializer));
            Registrations.Add(registration);
        }

        /// <summary>
        /// Registers a type to be compact-serialized.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="typeName">The schema type name.</param>
        /// <param name="serializer">The compact serializer.</param>
        public void Register<T>(string typeName, ICompactSerializer<T> serializer)
        {
            if (string.IsNullOrWhiteSpace(typeName)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(typeName));
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));

            // 1 unique registration per type-name
            if (_names.Contains(typeName))
                throw new ArgumentException($"A type with type name {typeName} has already been registered.", nameof(typeName));

            // 1 unique registration per type
            if (_types.Contains(typeof(T)))
                throw new ArgumentException($"A serializer for type {typeof(T)} has already been registered.", nameof(serializer));

            _names.Add(typeName);
            _types.Add(typeof(T));

            var registration = Registration.New(typeName, typeof(T), CompactSerializerWrapper.Create(serializer));
            Registrations.Add(registration);
        }

        /// <summary>
        /// Gets the registrations.
        /// </summary>
        internal List<Registration> Registrations { get; } = new List<Registration>();

        /// <summary>
        /// Represents a registration.
        /// </summary>
        internal abstract class Registration
        {
            protected Registration(Type type, CompactSerializerWrapper serializer)
            {
                Type = type;
                Serializer = serializer;
            }

            public static Registration New(Schema schema, Type type, CompactSerializerWrapper serializer)
                => new RegistrationWithSchema(schema, type, serializer);

            public static Registration New(string typeName, Type type, CompactSerializerWrapper serializer)
                => new RegistrationWithTypeName(typeName, type, serializer);

            /// <summary>
            /// Gets the type.
            /// </summary>
            public Type Type { get; }

            /// <summary>
            /// Gets the serializer.
            /// </summary>
            public CompactSerializerWrapper Serializer { get; }
        }

        internal class RegistrationWithSchema : Registration
        {
            public RegistrationWithSchema(Schema schema, Type type, CompactSerializerWrapper serializer)
                : base(type, serializer)
            {
                Schema = schema;
            }

            /// <summary>
            /// Gets the schema.
            /// </summary>
            public Schema Schema { get; }
        }

        internal class RegistrationWithTypeName : Registration
        {
            public RegistrationWithTypeName(string typeName, Type type, CompactSerializerWrapper serializer) 
                : base(type, serializer)
            {
                TypeName = typeName;
            }

            /// <summary>
            /// Gets the type name.
            /// </summary>
            public string TypeName { get; }
        }

        /// <summary>
        /// Clones the options.
        /// </summary>
        public CompactOptions Clone() => new CompactOptions(this);
    }
}
