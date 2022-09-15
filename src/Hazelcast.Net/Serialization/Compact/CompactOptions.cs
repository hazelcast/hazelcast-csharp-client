// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
using System.Linq;
using System.Reflection;
using Hazelcast.Configuration;
using Hazelcast.Core;
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
        // we have, by design:
        //   serializer -(unique)-> type_name
        //   schema -(unique)-> type_name
        //
        // we enforce:
        //   serialized_type -(unique)-> type_name -(unique)-> serializer
        //                                         -(unique)-> schema
        //
        // it is still possible to have:
        //   type_name -(multiple)-> serialized_type
        // however, the reflection-based serialized cannot handle it, since it has no way
        // of determining which type to produce during deserialization. we validate that
        // we have an explicit serializer, and that all serialized_types can be handled
        // by that serializer, when producing the registrations

        // ReSharper disable InconsistentNaming
        private readonly HashSet<Type> _serializedType;
        private readonly Dictionary<Type, string> _serializedType_typeName;
        private readonly Dictionary<string, Schema> _typeName_schema;
        private readonly Dictionary<string, ICompactSerializer> _typeName_serializer;
        private readonly Dictionary<Type, bool> _serializedType_isClusterSchema;
        private readonly Dictionary<string, bool> _typeName_isClusterSchema;
        // ReSharper restore InconsistentNaming

        private CompactSerializerAdapter? _reflectionSerializerAdapter;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompactOptions"/> class.
        /// </summary>
        public CompactOptions()
        {
            _serializedType = new HashSet<Type>();
            _serializedType_typeName = new Dictionary<Type, string>();
            _typeName_schema = new Dictionary<string, Schema>();
            _typeName_serializer = new Dictionary<string, ICompactSerializer>();
            _serializedType_isClusterSchema = new Dictionary<Type, bool>();
            _typeName_isClusterSchema = new Dictionary<string, bool>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompactOptions"/> class.
        /// </summary>
        private CompactOptions(CompactOptions other)
        {
            _serializedType = new HashSet<Type>(other._serializedType);
            _serializedType_typeName = new Dictionary<Type, string>(other._serializedType_typeName);
            _typeName_schema = new Dictionary<string, Schema>(other._typeName_schema);
            _typeName_serializer = new Dictionary<string, ICompactSerializer>(other._typeName_serializer);
            _serializedType_isClusterSchema = new Dictionary<Type, bool>(other._serializedType_isClusterSchema);
            _typeName_isClusterSchema = new Dictionary<string, bool>(other._typeName_isClusterSchema);
            ReflectionSerializer = other.ReflectionSerializer;
        }

        private void EnsureUniqueTypeNamePerSerializedType(Type serializedType, string typeName)
        {
            // if the serialized type already exists, it must be with the same type name
            if (_serializedType_typeName.TryGetValue(serializedType, out var existing) && existing != typeName)
                throw new ConfigurationException($"A different type name for serialized type {serializedType} has already been provided.");
        }

        private void EnsureUniqueSchemaPerTypeName(Schema schema)
        {
            // at runtime we may end up with several schemas (received from the cluster) for the same type name
            // but NOT at configuration time, we need one unique schema to use when serializing
            if (_typeName_schema.TryGetValue(schema.TypeName, out var existing) && existing != schema)
                throw new ConfigurationException($"A different schema for type name {schema.TypeName} has already been provided.");
        }

        private void EnsureUniqueSerializerPerTypeName(ICompactSerializer serializer)
        {
            // if a serializer already exists for the type name, it must be the same
            if (_typeName_serializer.TryGetValue(serializer.TypeName, out var existing) && existing != serializer)
                throw new ConfigurationException($"A different serializer for type name {serializer.TypeName} has already been provided.");
        }

        private void EnsureTypeNameSerializerCanSerializeType(string typeName, Type serializedType)
        {
            // if a serializer exists for the type name, it must be able to serialize the type
            if (_typeName_serializer.TryGetValue(typeName, out var serializer) && !serializer.GetSerializedType().IsAssignableFrom(serializedType))
                throw new ConfigurationException($"A serializer has been provided for type name {typeName} which serializes {serializer.GetSerializedType()}, and cannot serializer {serializedType}.");
        }

        private void EnsureSerializerCanSerializeTypes(ICompactSerializer serializer)
        {
            // if serialized types have been assigned the serializer type name
            // then the serializer must be able to serialize them
            foreach (var (st, tn) in _serializedType_typeName)
            {
                if (tn != serializer.TypeName) continue;
                if (!serializer.GetSerializedType().IsAssignableFrom(st))
                    throw new ConfigurationException($"Serialized type {st} has been assigned the serializer type name ({tn}) but the serializer cannot serialize that type.");
            }
        }

        /// <summary>
        /// Adds a type.
        /// </summary>
        /// <typeparam name="T">The type to add.</typeparam>
        /// <remarks>
        /// <para>Use this method to declare that a type, which is not implicitly declared to compact
        /// serialization via any of the other available methods (such as registering a serializer for that
        /// type), should nevertheless be compact-serialized, even though another serialization method may
        /// also apply.</para>
        /// <para>All object types are implicitly compact-serialized, but only if not other serialization
        /// method applies. An <see cref="IPortable"/> object would be portable-serialized by default.
        /// Use this method to bypass serialization methods detection and force compact serialization.</para>
        /// </remarks>
        public void AddType<T>() => AddType(typeof(T));

        /// <remarks>
        /// <para>Use this method to declare that a type, which is not implicitly declared to compact
        /// serialization via any of the other available methods (such as registering a serializer for that
        /// type), should nevertheless be compact-serialized, even though another serialization method may
        /// also apply.</para>
        /// <para>All object types are implicitly compact-serialized, but only if not other serialization
        /// method applies. An <see cref="IPortable"/> object would be portable-serialized by default.
        /// Use this method to bypass serialization methods detection and force compact serialization.</para>
        /// </remarks>
        public void AddType(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            _serializedType.Add(type);
        }

        /// <summary>
        /// Adds a serializer.
        /// </summary>
        /// <typeparam name="TSerialized">The serialized type.</typeparam>
        /// <param name="serializer">The compact serializer.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="serializer"/> is <c>null</c>.</exception>
        /// <exception cref="ConfigurationException">The operation conflicts with information that were already provided.</exception>
        public void AddSerializer<TSerialized>(ICompactSerializer<TSerialized> serializer) where TSerialized : notnull
        {
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));

            AddSerializer(typeof(TSerialized), serializer);
        }

        /// <summary>
        /// Adds a serializer.
        /// </summary>
        /// <param name="serializer">The serialized type.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="serializer"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">The <paramref name="serializer"/> does not implement <see cref="ICompactSerializer{TSerialized}"/>.</exception>
        /// <exception cref="ConfigurationException">The operation conflicts with information that were already provided.</exception>
        public void AddSerializer(ICompactSerializer serializer)
        {
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));

            // validate serializer
            var serializerType = serializer.GetType();
            if (!serializerType.IsICompactSerializerOfTSerialized(out var serializedType))
                throw new ArgumentException("Serializer does not implement ICompactSerializer<TSerialized>.");

            AddSerializer(serializedType, serializer);
        }

        private void AddSerializer(Type serializedType, ICompactSerializer serializer)
        {
            // private method: args cannot be null + serializer *is* ICompactSerializer<serializedType>

            EnsureUniqueTypeNamePerSerializedType(serializedType, serializer.TypeName);
            EnsureUniqueSerializerPerTypeName(serializer);
            EnsureSerializerCanSerializeTypes(serializer);

            _serializedType.Add(serializedType);
            _serializedType_typeName[serializedType] = serializer.TypeName;
            _typeName_serializer[serializer.TypeName] = serializer;
        }

        /// <summary>
        /// Adds a serializer.
        /// </summary>
        /// <typeparam name="TSerializerSerialized">The type serialized by the serializer.</typeparam>
        /// <typeparam name="TSerialized">The type for which the serializer is added.</typeparam>
        /// <param name="serializer">The serializer.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="serializer"/> is <c>null</c>.</exception>
        /// <exception cref="ConfigurationException">The operation conflicts with information that were already provided.</exception>
        public void AddSerializer<TSerializerSerialized, TSerialized>(ICompactSerializer<TSerializerSerialized> serializer)
            where TSerialized : TSerializerSerialized
            where TSerializerSerialized : notnull
        {
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));

            var serializedType = typeof (TSerialized);

            EnsureUniqueTypeNamePerSerializedType(serializedType, serializer.TypeName);
            EnsureUniqueSerializerPerTypeName(serializer);
            EnsureSerializerCanSerializeTypes(serializer);

            _serializedType.Add(serializedType);
            _serializedType_typeName[serializedType] = serializer.TypeName;
            _typeName_serializer[serializer.TypeName] = serializer;
        }

        // TODO: consider public SetTypeName

        /// <summary>
        /// Sets the type-name for a serialized type.
        /// </summary>
        /// <typeparam name="TSerialized">The serialized type.</typeparam>
        /// <param name="typeName">The type-name.</param>
        /// <exception cref="ArgumentException"><paramref name="typeName"/> is <c>null</c> or an empty string.</exception>
        /// <exception cref="ConfigurationException">The operation conflicts with information that were already provided.</exception>
        internal void SetTypeName<TSerialized>(string typeName) => SetTypeName(typeof (TSerialized), typeName);

        /// <summary>
        /// Sets the type-name for a serialized type.
        /// </summary>
        /// <param name="serializedType">The serialized type.</param>
        /// <param name="typeName">The type-name.</param>
        /// <exception cref="ArgumentNullException"><paramref name="serializedType"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="typeName"/> is <c>null</c> or an empty string.</exception>
        /// <exception cref="ConfigurationException">The operation conflicts with information that were already provided.</exception>
        internal void SetTypeName(Type serializedType, string typeName)
        {
            if (serializedType == null) throw new ArgumentNullException(nameof(serializedType));
            if (string.IsNullOrWhiteSpace(typeName)) throw new ArgumentException(ExceptionMessages.NullOrEmpty);

            EnsureUniqueTypeNamePerSerializedType(serializedType, typeName);
            EnsureTypeNameSerializerCanSerializeType(typeName, serializedType);

            _serializedType.Add(serializedType);
            _serializedType_typeName[serializedType] = typeName;
        }

        // TODO: consider public SetSchema

        /// <summary>
        /// Configures a schema for a serialized type.
        /// </summary>
        /// <typeparam name="TSerialized">The serialized type.</typeparam>
        /// <param name="schema">The schema.</param>
        /// <param name="isClusterSchema">Whether the schema is known by the cluster already.</param>
        /// <exception cref="ArgumentNullException"><paramref name="schema"/> is <c>null</c>.</exception>
        /// <exception cref="ConfigurationException">The operation conflicts with information that were already provided.</exception>
        internal void SetSchema<TSerialized>(Schema schema, bool isClusterSchema) => SetSchema(typeof (TSerialized), schema, isClusterSchema);

        /// <summary>
        /// Configures a schema for a serialized type.
        /// </summary>
        /// <typeparam name="TSerialized">The serialized type.</typeparam>
        /// <param name="isClusterSchema">Whether the schema is known by the cluster already.</param>
        internal void SetSchema<TSerialized>(bool isClusterSchema) => SetSchema(typeof(TSerialized), isClusterSchema);

        /// <summary>
        /// Configures a schema for a serialized type.
        /// </summary>
        /// <param name="serializedType">The serialized type.</param>
        /// <param name="schema">The schema.</param>
        /// <param name="isClusterSchema">Whether the schema is known by the cluster already.</param>
        /// <exception cref="ArgumentNullException"><paramref name="serializedType"/> or <paramref name="schema"/> is <c>null</c>.</exception>
        /// <exception cref="ConfigurationException">The operation conflicts with information that were already provided.</exception>
        internal void SetSchema(Type serializedType, Schema schema, bool isClusterSchema)
        {
            if (serializedType == null) throw new ArgumentNullException(nameof(serializedType));
            if (schema == null) throw new ArgumentNullException(nameof(schema));

            var typeName = schema.TypeName;

            EnsureUniqueTypeNamePerSerializedType(serializedType, typeName);
            EnsureUniqueSchemaPerTypeName(schema);
            EnsureTypeNameSerializerCanSerializeType(typeName, serializedType);

            _serializedType.Add(serializedType);
            _serializedType_typeName[serializedType] = typeName;
            _typeName_schema[schema.TypeName] = schema;

            _typeName_isClusterSchema[schema.TypeName] = isClusterSchema;
            _serializedType_isClusterSchema[serializedType] = isClusterSchema;
        }

        /// <summary>
        /// Configures a schema for a serialized type.
        /// </summary>
        /// <param name="serializedType">The serialized type.</param>
        /// <param name="isClusterSchema">Whether the schema is known by the cluster already.</param>
        /// <exception cref="ArgumentNullException"><paramref name="serializedType"/> is <c>null</c>.</exception>
        internal void SetSchema(Type serializedType, bool isClusterSchema)
        {
            if (serializedType == null) throw new ArgumentNullException(nameof(serializedType));

            _serializedType.Add(serializedType);
            _serializedType_isClusterSchema[serializedType] = isClusterSchema;
        }

        /// <summary>
        /// Configures a schema.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <param name="isClusterSchema">Whether the schema is known by the cluster already.</param>
        /// <exception cref="ArgumentNullException"><paramref name="schema"/> is <c>null</c>.</exception>
        /// <exception cref="ConfigurationException">The operation conflicts with information that were already provided.</exception>
        internal void SetSchema(Schema schema, bool isClusterSchema)
        {
            if (schema == null) throw new ArgumentNullException(nameof(schema));

            EnsureUniqueSchemaPerTypeName(schema);

            _typeName_schema[schema.TypeName] = schema;
            _typeName_isClusterSchema[schema.TypeName] = isClusterSchema;
        }

        private bool IsClusterSchema(Type serializedType, string typeName)
        {
            var isClusterSchema = false;
            if (_serializedType_isClusterSchema.TryGetValue(serializedType, out var ics1))
                isClusterSchema |= ics1;
            if (_typeName_isClusterSchema.TryGetValue(typeName, out var ics2))
                isClusterSchema |= ics2;
            return isClusterSchema;
        }

        /// <summary>
        /// Creates and returns the serializer registrations corresponding to options.
        /// </summary>
        /// <returns>The serializer registrations corresponding to options.</returns>
        /// <exception cref="ConfigurationException">The options contain conflicts that prevent the registrations from being created.</exception>
        internal IEnumerable<CompactRegistration> GetRegistrations()
        {
            // ReSharper disable once InconsistentNaming
            var typeName_serializedTypes = _serializedType_typeName
                .GroupBy(x => x.Value)
                .ToDictionary(
                    x => x.Key,
                    x => x.Select(y => y.Key).ToList());

            // validate
            foreach (var (typeName, serializedTypes) in typeName_serializedTypes)
            {
                // if more than 1 type share the same type name, then a custom serializer is required
                if (serializedTypes.Count > 1 && !_typeName_serializer.ContainsKey(typeName))
                    throw new ConfigurationException($"More that one type have been assigned the type name {typeName}, " +
                                                     "but no serializer for that type name has been provided, " +
                                                     "and the built-in reflection-based serializer cannot handle that situation.");
            }

            // process the serializers (all other registrations below will use the reflection serializer)
            foreach (var serializer in _typeName_serializer.Values)
            {
                var isClusterSchema = IsClusterSchema(serializer.GetSerializedType(), serializer.TypeName);
                var withSchema = _typeName_schema.TryGetValue(serializer.TypeName, out var schema);

                // since the serializer has been declared, there *has* to be at least one serialized type for the type name
                // but there may be more than one, and we need a registration for each type - registrations work at actual type level
                foreach (var serializedType in typeName_serializedTypes[serializer.TypeName])
                {
                    yield return withSchema
                        ? new CompactRegistration(serializedType, CompactSerializerAdapter.Create(serializer), schema!, isClusterSchema)
                        : new CompactRegistration(serializedType, CompactSerializerAdapter.Create(serializer), serializer.TypeName, isClusterSchema);
                }
            }

            // process the type names that don't have an associated serializer but have a serialized type, and may have a schema
            foreach (var (serializedType, typeName) in _serializedType_typeName)
            {
                if (_typeName_serializer.ContainsKey(typeName)) continue; // already yielded above

                var isClusterSchema = IsClusterSchema(serializedType, typeName);

                yield return _typeName_schema.TryGetValue(typeName, out var schema)
                    ? new CompactRegistration(serializedType, ReflectionSerializerAdapter, schema, isClusterSchema)
                    : new CompactRegistration(serializedType, ReflectionSerializerAdapter, typeName, isClusterSchema);
            }

            // process the schemas that don't have an associated serialized type
            foreach (var schema in _typeName_schema.Values)
            {
                if (_serializedType_typeName.ContainsValue(schema.TypeName)) continue; // already yielded above

                var serializedType = Type.GetType(schema.TypeName);
                if (serializedType == null)
                    throw new ConfigurationException($"A schema was provided for type name {schema.TypeName}, but that type name does not match any CLR type.");

                var isClusterSchema = IsClusterSchema(serializedType, schema.TypeName);
                yield return new CompactRegistration(serializedType, ReflectionSerializerAdapter, schema, isClusterSchema);
            }

            foreach (var serializedType in _serializedType)
            {
                if (_serializedType_typeName.ContainsKey(serializedType)) continue; // already yielded above

                var typeName = GetDefaultTypeName(serializedType);
                var isClusterSchema = IsClusterSchema(serializedType, typeName);
                yield return new CompactRegistration(serializedType, ReflectionSerializerAdapter, GetDefaultTypeName(serializedType), isClusterSchema);
            }
        }

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
        /// Gets the reflection serializer adapter.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ConfigurationException">No reflection serializer has been configured.</exception>
        internal CompactSerializerAdapter ReflectionSerializerAdapter
            => _reflectionSerializerAdapter ??= CompactSerializerAdapter.Create(ReflectionSerializer ?? throw new ConfigurationException("Missing a serializer.")); 

        /// <summary>
        /// Gets the default type name used by compact serialization for a type.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <returns>The default type name used by compact serialization for the specified type.</returns>
        internal static string GetDefaultTypeName<T>() => GetDefaultTypeName(typeof(T));

        /// <summary>
        /// Gets the default type name used by compact serialization for a type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The default type name used by compact serialization for the specified type.</returns>
        internal static string GetDefaultTypeName(Type type) => type.GetQualifiedTypeName() ??
            throw new SerializationException($"Failed to obtain {type} assembly qualified name.");

        /// <summary>
        /// Clones the options.
        /// </summary>
        public CompactOptions Clone() => new(this);
    }
}
