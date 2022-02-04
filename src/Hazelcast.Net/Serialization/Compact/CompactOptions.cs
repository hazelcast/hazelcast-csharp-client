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
        private readonly List<Registration> _registrations = new List<Registration>();

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
            _registrations = new List<Registration>(other._registrations);
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
                throw new InvalidOperationException();

            // 1 unique registration per type
            if (_types.Contains(typeof(T)))
                throw new InvalidOperationException();

            _names.Add(schema.TypeName);
            _types.Add(typeof (T));

            var registration = new Registration
            (
                schema,
                typeof (T),
                CompactSerializerWrapper.Create(serializer)
            );
            _registrations.Add(registration);
        }

        /// <summary>
        /// Gets the registrations.
        /// </summary>
        internal List<Registration> Registrations => _registrations;

        /// <summary>
        /// Represents a registration.
        /// </summary>
        internal class Registration
        {
            public Registration(Schema schema, Type type, CompactSerializerWrapper serializer)
            {
                Schema = schema;
                Type = type;
                Serializer = serializer;
            }

            /// <summary>
            /// The schema.
            /// </summary>
            public Schema Schema { get; }

            /// <summary>
            /// The type.
            /// </summary>
            public Type Type { get; }

            /// <summary>
            /// The serializer.
            /// </summary>
            public CompactSerializerWrapper Serializer { get; }
        }

        /// <summary>
        /// Clones the options.
        /// </summary>
        public CompactOptions Clone() => new CompactOptions(this);
    }
}
