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
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Hazelcast.Core;

namespace Hazelcast.Serialization.Compact
{
    /// <summary>
    /// Represents the <see cref="ISerializer"/> that supports compact serialization.
    /// </summary>
    internal sealed class CompactSerializationSerializer : IStreamSerializer<object>, IReadWriteObjectsFromIObjectDataInputOutput
    {
        private readonly ConcurrentDictionary<Type, CompactRegistration> _registrationsByType = new ConcurrentDictionary<Type, CompactRegistration>();
        private readonly ConcurrentDictionary<long, CompactRegistration> _registrationsById = new ConcurrentDictionary<long, CompactRegistration>();
        private readonly ConcurrentDictionary<Type, Schema> _schemasMap = new ConcurrentDictionary<Type, Schema>();
        private readonly ISchemas _schemas;
        private readonly CompactSerializerAdapter _reflectionSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompactSerializationSerializer"/> class.
        /// </summary>
        /// <param name="options">Compact serialization options.</param>
        /// <param name="schemas">A schema-management service instance.</param>
        public CompactSerializationSerializer(CompactOptions options, ISchemas schemas)
        {
            _schemas = schemas;
            _reflectionSerializer = CompactSerializerAdapter.Create(options.ReflectionSerializer ?? new ReflectionSerializer());

            foreach (var registration in options.GetRegistrations(_reflectionSerializer))
            {
                // note: options ensure that registrations are safe / that there are no collisions

                _registrationsByType[registration.SerializedType] = registration;

                if (registration.HasSchema)
                {
                    var schema = registration.Schema!;
                    _registrationsById[schema.Id] = registration;
                    _schemas.Add(schema, registration.IsClusterSchema);
                    _schemasMap[registration.SerializedType] = schema;
                }
            }
        }

        /// <inheritdoc />
        public int TypeId => SerializationConstants.ConstantTypeCompact;

        // for tests
        public ISchemas Schemas => _schemas;

        public bool HasRegistrationForType(Type type) => _registrationsByType.ContainsKey(type);

        public void Dispose()
        {
            // note: ISchemas is not IDisposable because we don't have background tasks
        }

        /// <summary>
        /// Gets the default type name used by compact serialization for a type.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <returns>The default type name used by compact serialization for the specified type.</returns>
        public static string GetTypeName<T>() => GetTypeName(typeof (T));

        /// <summary>
        /// Gets the default type name used by compact serialization for a type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The default type name used by compact serialization for the specified type.</returns>
        public static string GetTypeName(Type type) => type.GetQualifiedTypeName() ?? 
                                                       throw new SerializationException($"Failed to obtain {type} assembly qualified name.");

        public object Read(IObjectDataInput input)
        {
            if (!(input is ObjectDataInput inputInstance))
                throw new ArgumentException("Input must be an ObjectDataInput instance.", nameof(input));

            return ReadObject(inputInstance);
        }

        public T Read<T>(IObjectDataInput input)
            => SerializationService.CastObject<T>(Read(input), true);

        public void Write(IObjectDataOutput output, object obj)
        {
            if (!(output is ObjectDataOutput outputInstance))
                throw new ArgumentException("Output must be an ObjectDataOutput instance.", nameof(output));

            WriteObject(outputInstance, obj);
        }

        // invoked when writing out an object and we need its registration, ie its serializer
        // either a serializer has been registered already for the type, or the type is ICompactable
        // and can provide its own serializer, or we need to fall back to reflection-based serialization.
        // note: if we were to implement code generation, the generated serializers would be registered.
        private CompactRegistration GetOrCreateRegistration(object obj)
        {
            var typeOfObj = obj.GetType();

            // last-chance, really - go for reflection serializer
            // have to assume that the schema is unknown from the cluster
            return _registrationsByType.GetOrAdd(typeOfObj, type 
                => new CompactRegistration(typeOfObj, _reflectionSerializer, GetTypeName(typeOfObj), false));
        }

        // invoked when reading an object and we need its registration, ie its serializer, and all
        // we have is the schema - FIXME JAVA?
        // Java does getOrCreateRegistration(schema.getTypeName()) which means it can only handle
        // one single schema per type name and how is this supposed to work at all?!
        private CompactRegistration GetOrCreateRegistration(Schema schema)
        {
            if (_registrationsById.TryGetValue(schema.Id, out var registration)) return registration;

            // otherwise, all we have is the type-name and we need a registration
            // note: no race cond. here, everything will TryAdd anyways

            registration = GetOrCreateRegistration_ByTypeName(schema) ??
                           GetOrCreateRegistration_ByClrType(schema);

            // Java supports returning null here, and then falls back to GenericRecord.
            // we do not support GenericRecord in .NET and therefore can throw here.

            if (registration != null) return registration;

            throw new SerializationException($"Could not find a compact serializer for schema {schema.Id}.");
        }

        private CompactRegistration? GetOrCreateRegistration_ByTypeName(Schema schema)
        {
            // maybe we have configured a registration for the type name without a schema
            // and now we have the schema => see if we can bind them all together

            // look up for the (necessarily unique) registration for that type name
            var registration = _registrationsByType.Values
                .FirstOrDefault(x => x.TypeName == schema.TypeName);

            if (registration != null)
            {
                // found it, now we can bind it to a schema.id
                _registrationsById.TryAdd(schema.Id, registration);

                // do *not* add to _schemasMap - it may be one schema for the type, but not
                // the canonical schema that the client should use for serializing.
            }

            return registration;
        }

        private CompactRegistration? GetOrCreateRegistration_ByClrType(Schema schema)
        {
            // check whether the type-name is a valid C# type that we can instantiate
            var t = Type.GetType(schema.TypeName);
            if (t == null) return null;

            object? obj;
            try
            {
                obj = Activator.CreateInstance(t);
            }
            catch
            {
                obj = null;
            }

            if (obj == null) return null;

            // will handle everything, including Compactable, eventually falling back to reflection
            var registration = GetOrCreateRegistration(obj); // non-null, adds to _registrationsByType
            _registrationsById.TryAdd(schema.Id, registration);

            // do *not* add to _schemasMap - it may be one schema for the type, but not
            // the canonical schema that the client should use for serializing.

            return registration;
        }

        public void WriteObject(ObjectDataOutput output, object obj)
        {
            var typeOfObj = obj.GetType();
            var registration = GetOrCreateRegistration(obj);
            if (!_schemasMap.TryGetValue(typeOfObj, out var schema))
            {
                // no schema was registered for this type, so we are going to serialize the
                // object, capture the fields, and generate a schema for it - this requires and
                // assumes that the serializer is not "clever" and does not omit fields for
                // optimization reasons.
                schema = BuildSchema(registration, obj);
                _schemasMap[typeOfObj] = schema;

                // now that we know the schema identifier, we can update our registrations map
                _registrationsById.TryAdd(schema.Id, registration);

                // now that we have a new schema, we need to publish it, unless specified
                // otherwise by the registration.
                _schemas.Add(schema, registration.IsClusterSchema);
            }

            WriteSchema(output, schema);
            var writer = new CompactWriter(this, output, schema);
            registration.Serializer.Write(writer, obj);
            writer.Complete();
        }

        private static void WriteSchema(ObjectDataOutput output, Schema schema)
        {
            output.WriteLong(schema.Id);

            // note: this is the code we would use if we were to send the schema alongside
            // the data, and we keep it here for reference only, just in case one day...

            /*
            if (!withSchema) return;

            var startPosition = output.Position;
            output.WriteInt(0);
            var schemaPosition = output.Position;

            // note: don't output.WriteObject(schema) else it's serialized as identified serializable
            output.WriteString(schema.TypeName);
            output.WriteInt(schema.Fields.Count);
            foreach (var field in schema.Fields)
            {
                output.WriteString(field.FieldName);
                output.WriteInt((int)field.Kind);
            }

            var position = output.Position;
            output.MoveTo(startPosition);
            output.WriteInt(position - schemaPosition);
            output.MoveTo(position);
            */
        }

        private static Schema BuildSchema(CompactRegistration registration, object obj)
        {
            var builder = new SchemaBuilderWriter(registration.TypeName);
            registration.Serializer.Write(builder, obj);
            return builder.Build();
        }

        private object ReadObject(ObjectDataInput input)
        {
            var schema = ReadSchema(input);
            var registration = GetOrCreateRegistration(schema);

            var reader = new CompactReader(this, input, schema, registration.SerializedType);
            return registration.Serializer.Read(reader);
        }

        private Schema ReadSchema(ObjectDataInput input)
        {
            var schemaId = input.ReadLong();
            if (_schemas.TryGet(schemaId, out var schema)) return schema;

            // trying to de-serialize an unknown schema - not going to do anything about it here,
            // just report the situation via a dedicated exception. the caller is expected to
            // catch it, fetch the schema, and retry.
            
            // FIXME generally not happy with handling of metadata 
            
            // ToObject: if a schema is missing, a MissingCompactSchemaException is thrown, thus
            //   indicating that the data cannot be deserialized. the user is expected to await
            //   exception.FetchSchemaAsync() and retry. this needs to happen *everywhere*
            //   ToObject is invoked.
            //
            // using exceptions here is not pretty. the alternative would be to return a state
            // of some sort, anything representing that we are in need of a schema - but then,
            // that state would need to bubble along the API up to ToObject and? 

            // ToData: if a new schema is created along the way, it is queued for publication
            //   and before we send *anything* to the server, we publish schemas that need
            //   to be published. could be published by another request and then...?

            throw new MissingCompactSchemaException(schemaId, id => _schemas.GetOrFetchAsync(id));

            // note: this is the code we would use if we were to receive the schema alongside
            // the data, and we keep it here for reference only, just in case one day...

            /*
            // note: don't input.ReadObject<Schema>() else it's serialized as identified serializable
            var typeName = input.ReadString();
            var fieldCount = input.ReadInt();
            var fields = new SchemaField[fieldCount];
            for (var i = 0; i < fieldCount; i++)
            {
                var fieldName = input.ReadString();
                var kind = FieldKindEnum.Parse(input.ReadInt());
                fields[i] = new SchemaField(fieldName, kind);
            }

            schema = new Schema(typeName, fields);
            _schemas.Add(schema, true); // comes from cluster so, is published
            return schema;
            */
        }

        public async Task<bool> FetchSchema(long schemaId)
        {
            // fetch the schema - if successful, it's now in _schemas, and the next call
            // to ReadObject will find it via GetOrCreateRegistration - nothing more to
            // do here.
            var schema = await _schemas.GetOrFetchAsync(schemaId).CfAwait();
            return schema != null;
        }
    }
}
