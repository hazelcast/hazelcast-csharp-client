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
using System.Reflection;
using Hazelcast.Configuration;
using Hazelcast.Exceptions;

#nullable enable

namespace Hazelcast.Serialization.Compact
{
    /// <summary>
    /// (preview) Represents the compact serialization options.
    /// </summary>
    /// <remarks>
    /// <para>During the preview period, compact serialization is not enabled by default.</para>
    /// <para>The options represented by this class may change in breaking ways in the future.</para>
    /// </remarks>
    public sealed class CompactOptions
    {
        // note: these are very temporary for the compact serialization preview
        // and compact options are expected to be refactored at some point in time.
        private readonly HashSet<string> _names;
        private readonly HashSet<Type> _types;

        private HashSet<Assembly>? _assemblies;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompactOptions"/> class.
        /// </summary>
        public CompactOptions()
        {
            _names = new HashSet<string>();
            _types = new HashSet<Type>();
            Registrations = new List<CompactRegistration>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompactOptions"/> class.
        /// </summary>
        private CompactOptions(CompactOptions other)
        {
            Enabled = other.Enabled;
            _names = new HashSet<string>(other._names);
            _types = new HashSet<Type>(other._types);
            _assemblies = other._assemblies == null ? null : new HashSet<Assembly>(other._assemblies);
            Registrations = new List<CompactRegistration>(other.Registrations);
            ReflectionSerializer = other.ReflectionSerializer;
        }

        /// <summary>
        /// (preview) Whether compact serialization is enabled.
        /// </summary>
        /// <remarks>
        /// <para>During the preview period, compact serialization is not enabled by default.</para>
        /// </remarks>
        public bool Enabled { get; set; }

        // FIXME - consider non-generic overloads

        /// <summary>
        /// Registers a type for compact serialization.
        /// </summary>
        /// <typeparam name="TSerialized">The type.</typeparam>
        /// <param name="serializer">The compact serializer.</param>
        /// <param name="schema">The schema corresponding to the type.</param>
        /// <param name="isClusterSchema">Whether the schema is considered to be, at configuration time, already known by the cluster.</param>
        /// <remarks>
        /// <para>When <paramref name="isClusterSchema"/> is <c>false</c>, the compact serialization service will make
        /// sure to send the schema to the cluster before sending any data relying on that schema.</para>
        /// </remarks>
        public void Register<TSerialized>(ICompactSerializer<TSerialized> serializer, Schema schema, bool isClusterSchema)
        {
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));
            if (schema == null) throw new ArgumentNullException(nameof(schema));

            // 1 unique registration per type-name
            if (_names.Contains(schema.TypeName))
                throw new ConfigurationException($"A type with type name {schema.TypeName} has already been registered.");
            _names.Add(schema.TypeName);

            // 1 unique registration per type
            if (_types.Contains(typeof(TSerialized)))
                throw new ConfigurationException($"A serializer for type {typeof(TSerialized)} has already been registered.");
            _types.Add(typeof(TSerialized));

            Registrations.Add(new CompactRegistration(schema, typeof (TSerialized), CompactSerializerWrapper.Create(serializer), isClusterSchema));
        }

        /// <summary>
        /// Registers a type for compact serialization.
        /// </summary>
        /// <typeparam name="TSerialized">The type.</typeparam>
        /// <param name="serializer">The compact serializer.</param>
        /// <param name="typeName">The schema type name.</param>
        /// <param name="isClusterSchema">Whether the schema is considered to be, at configuration time, already known by the cluster.</param>
        /// <remarks>
        /// <para>The type schema will be generated at runtime from the provided serializer. Note that the serializer plainly write
        /// all fields without omitting any, i.e. not skip some fields for optimization purposes, as this would result in a
        /// corrupt schema.</para>
        /// <para>When <paramref name="isClusterSchema"/> is <c>false</c>, the compact serialization service will make
        /// sure to send the schema to the cluster before sending any data relying on that schema.</para>
        /// </remarks>
        public void Register<TSerialized>(ICompactSerializer<TSerialized> serializer, string typeName, bool isClusterSchema)
        {
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));
            if (string.IsNullOrWhiteSpace(typeName)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(typeName));

            // 1 unique registration per type-name
            if (_names.Contains(typeName))
                throw new ConfigurationException($"A type with type name {typeName} has already been registered.");

            // 1 unique registration per type
            if (_types.Contains(typeof(TSerialized)))
                throw new ConfigurationException($"A serializer for type {typeof(TSerialized)} has already been registered.");

            _names.Add(typeName);
            _types.Add(typeof(TSerialized));

            Registrations.Add(new CompactRegistration(typeName, typeof(TSerialized), CompactSerializerWrapper.Create(serializer), isClusterSchema));
        }

        /// <summary>
        /// Registers a type for compact serialization.
        /// </summary>
        /// <typeparam name="TSerialized">The type.</typeparam>
        /// <param name="serializer">The compact serializer.</param>
        /// <param name="isClusterSchema">Whether the schema is considered to be, at configuration time, already known by the cluster.</param>
        /// <remarks>
        /// <para>The type name will be the fully-qualified name of the .NET type.</para>
        /// <para>The type schema will be generated at runtime from the provided serializer. Note that the serializer plainly write
        /// all fields without omitting any, i.e. not skip some fields for optimization purposes, as this would result in a
        /// corrupt schema.</para>
        /// <para>When <paramref name="isClusterSchema"/> is <c>false</c>, the compact serialization service will make
        /// sure to send the schema to the cluster before sending any data relying on that schema.</para>
        /// </remarks>
        public void Register<TSerialized>(ICompactSerializer<TSerialized> serializer, bool isClusterSchema)
            => Register(serializer, CompactSerializer.GetTypeName<TSerialized>(), isClusterSchema);

        /// <summary>
        /// Registers a type for compact serialization.
        /// </summary>
        /// <typeparam name="TSerialized">The type.</typeparam>
        /// <param name="schema">The schema corresponding to the type.</param>
        /// <param name="isClusterSchema">Whether the schema is considered to be, at configuration time, already known by the cluster.</param>
        /// <remarks>
        /// <para>When a type is registered for compact serialization without an explicit serializer, a runtime reflection-
        /// based serializer will be used. It will handle all properties which expose both a public setter and a
        /// public getter. Unless the type has been specified as compactable with the <see cref="CompactSerializableAttribute"/>
        /// or the <see cref="CompactSerializableTypeAttribute"/> and a corresponding serializer has been generated at
        /// compile time.</para>
        /// <para>When <paramref name="isClusterSchema"/> is <c>false</c>, the compact serialization service will make
        /// sure to send the schema to the cluster before sending any data relying on that schema.</para>
        /// </remarks>
        public void Register<TSerialized>(Schema schema, bool isClusterSchema)
        {
            if (schema == null) throw new ArgumentNullException(nameof(schema));

            // 1 unique registration per type-name
            if (_names.Contains(schema.TypeName))
                throw new ConfigurationException($"A type with type name {schema.TypeName} has already been registered.");
            _names.Add(schema.TypeName);

            // 1 unique registration per type
            if (_types.Contains(typeof(TSerialized)))
                throw new ConfigurationException($"A serializer for type {typeof(TSerialized)} has already been registered.");
            _types.Add(typeof(TSerialized));

            Registrations.Add(new CompactRegistration(schema, typeof(TSerialized), isClusterSchema));
        }

        /// <summary>
        /// Registers a type for compact serialization.
        /// </summary>
        /// <typeparam name="TSerialized">The type.</typeparam>
        /// <param name="typeName">The schema type name.</param>
        /// <param name="isClusterSchema">Whether the schema is considered to be, at configuration time, already known by the cluster.</param>
        /// <remarks>
        /// <para>The type schema will be generated at runtime from the provided serializer. Note that the serializer plainly write
        /// all fields without omitting any, i.e. not skip some fields for optimization purposes, as this would result in a
        /// corrupt schema.</para>
        /// <para>When a type is registered for compact serialization without an explicit serializer, a runtime reflection-
        /// based serializer will be used. It will handle all properties which expose both a public setter and a
        /// public getter. Unless the type has been specified as compactable with the <see cref="CompactSerializableAttribute"/>
        /// or the <see cref="CompactSerializableTypeAttribute"/> and a corresponding serializer has been generated at
        /// compile time.</para>
        /// <para>When <paramref name="isClusterSchema"/> is <c>false</c>, the compact serialization service will make
        /// sure to send the schema to the cluster before sending any data relying on that schema.</para>
        /// </remarks>
        public void Register<TSerialized>(string typeName, bool isClusterSchema)
        {
            if (string.IsNullOrWhiteSpace(typeName)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(typeName));

            // 1 unique registration per type-name
            if (_names.Contains(typeName))
                throw new ConfigurationException($"A type with type name {typeName} has already been registered.");

            // 1 unique registration per type
            if (_types.Contains(typeof(TSerialized)))
                throw new ConfigurationException($"A serializer for type {typeof(TSerialized)} has already been registered.");

            _names.Add(typeName);
            _types.Add(typeof(TSerialized));

            Registrations.Add(new CompactRegistration(typeName, typeof(TSerialized), isClusterSchema));
        }

        /// <summary>
        /// Registers a type for compact serialization.
        /// </summary>
        /// <typeparam name="TSerialized">The type.</typeparam>
        /// <param name="isClusterSchema">Whether the schema is considered to be, at configuration time, already known by the cluster.</param>
        /// <remarks>
        /// <para>The type name will be the fully-qualified name of the .NET type.</para>
        /// <para>The type schema will be generated at runtime from the provided serializer. Note that the serializer plainly write
        /// all fields without omitting any, i.e. not skip some fields for optimization purposes, as this would result in a
        /// corrupt schema.</para>
        /// <para>When a type is registered for compact serialization without an explicit serializer, a runtime reflection-
        /// based serializer will be used. It will handle all properties which expose both a public setter and a
        /// public getter. Unless the type has been specified as compactable with the <see cref="CompactSerializableAttribute"/>
        /// or the <see cref="CompactSerializableTypeAttribute"/> and a corresponding serializer has been generated at
        /// compile time.</para>
        /// <para>When <paramref name="isClusterSchema"/> is <c>false</c>, the compact serialization service will make
        /// sure to send the schema to the cluster before sending any data relying on that schema.</para>
        /// </remarks>
        public void Register<TSerialized>(bool isClusterSchema)
            => Register<TSerialized>(CompactSerializer.GetTypeName<TSerialized>(), isClusterSchema);

        /// <summary>
        /// Registers an assembly for compact serialization.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <remarks>
        /// <para>When an assembly is registered for compact serialization, the client scans the assembly for assembly-
        /// level <see cref="CompactSerializerAttribute"/> and registers the serialized type and serializer as
        /// specified by the attribute.</para>
        /// <para>Note that it remains possible to also register the type explicitly, in order to specify a type name,
        /// a full schema, or whether the schema is a cluster schema. Only, no serializer should be provided, as the
        /// serializer is derived from the assembly-level attribute.</para>
        /// </remarks>
        public void Register(Assembly assembly)
        {
            (_assemblies ??= new HashSet<Assembly>()).Add(assembly);
        }

        /// <summary>
        /// Gets the registrations.
        /// </summary>
        internal List<CompactRegistration> Registrations { get; }

        /// <summary>
        /// Gets the assemblies.
        /// </summary>
        /// <exception cref="InvalidOperationException">No assembly has been registered.</exception>
        internal ISet<Assembly> Assemblies => _assemblies ?? throw new InvalidOperationException("No assembly has been registered.");

        /// <summary>
        /// Whether assemblies have been registered.
        /// </summary>
        internal bool HasAssemblies => _assemblies != null;

        /// <summary>
        /// Gets or sets the reflection serializer.
        /// </summary>
        /// <remarks>
        /// <para>This is used internally to inject different implementations of the reflection
        /// serializer into the compact serializer, for tests and benchmarks purposes. It should
        /// be left <c>null</c> to use the default, production reflection serializer.</para>
        /// </remarks>
        internal ICompactSerializer<object>? ReflectionSerializer { get; set; }

        /// <summary>
        /// Clones the options.
        /// </summary>
        public CompactOptions Clone() => new CompactOptions(this);
    }
}
