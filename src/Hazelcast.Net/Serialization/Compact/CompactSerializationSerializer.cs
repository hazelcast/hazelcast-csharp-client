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

#nullable enable

using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Messaging;

namespace Hazelcast.Serialization.Compact
{
    /// <summary>
    /// Represents the <see cref="ISerializer"/> that supports compact serialization.
    /// </summary>
    internal sealed class CompactSerializationSerializer : IStreamSerializer<object>, IReadObjectsFromObjectDataInput, IWriteObjectsToObjectDataOutput
    {
        private readonly CompactSerializerAdapter _genericRecordSerializer;
        private readonly ConcurrentDictionary<Type, CompactRegistration> _registrationsByType = new();
        private readonly ConcurrentDictionary<long, CompactRegistration> _registrationsById = new();
        private readonly ConcurrentDictionary<Type, Schema> _schemasMap = new();
        private readonly CompactOptions _options;
        private readonly ISchemas _schemas;
        private readonly Endianness _endianness;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompactSerializationSerializer"/> class.
        /// </summary>
        /// <param name="options">Compact serialization options.</param>
        /// <param name="schemas">A schema-management service instance.</param>
        /// <param name="endianness">The endianness.</param>
        public CompactSerializationSerializer(CompactOptions options, ISchemas schemas, Endianness endianness)
        {
            _options = options;
            _schemas = schemas;
            _endianness = endianness;

            _genericRecordSerializer = CompactSerializerAdapter.Create(new CompactGenericRecordSerializer());

            foreach (var registration in options.GetRegistrations())
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

        public bool TryGetSerializer(Type type, out ICompactSerializer? serializer)
        {
            var hasSerializer = _registrationsByType.TryGetValue(type, out var registration);
            serializer = hasSerializer ? registration!.Serializer.Serializer : null;
            return hasSerializer;
        }

        public void Dispose()
        {
            // note: ISchemas is IDisposable but is owned by the global SerializationService
        }

        /// <inheritdoc cref="IStreamSerializer{T}.Read"/>
        public object Read(IObjectDataInput input)
            => ReadObject(input.MustBe<ObjectDataInput>(nameof(input)), typeof(object));

        public object Read(IObjectDataInput input, Type type)
            => SerializationService.CastObject(ReadObject(input.MustBe<ObjectDataInput>(nameof(input)), type), type, true);

        /// <inheritdoc cref="IStreamSerializer{T}.Read"/>
        public T Read<T>(IObjectDataInput input)
            => SerializationService.CastObject<T>(Read(input, typeof(T)), true);

        public void Write(IObjectDataOutput output, object obj)
            => WriteObject(output.MustBe<ObjectDataOutput>(nameof(output)), obj);

        // invoked when writing out an object and we need its registration, ie its serializer
        // either a serializer has been registered already for the type, or we need to fall back
        // to reflection-based serialization.
        // note: if we were to implement code generation, the generated serializers would be registered.
        private CompactRegistration GetOrCreateRegistration(object obj)
        {
            var typeOfObj = obj.GetType();

            // last-chance, really - go for reflection serializer
            // have to assume that the schema is unknown from the cluster
            return _registrationsByType.GetOrAdd(typeOfObj, type 
                => new CompactRegistration(type, _options.ReflectionSerializerAdapter, CompactOptions.GetDefaultTypeName(type), false));
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
            Schema? schema;
            CompactSerializerAdapter serializer;

            if (obj is CompactGenericRecordBase genericRecord)
            {
                _schemas.Add(schema = genericRecord.Schema, false);
                serializer = _genericRecordSerializer;
            }
            else
            {
                var typeOfObj = obj.GetType();
                var registration = GetOrCreateRegistration(obj);
                serializer = registration.Serializer;
                if (!_schemasMap.TryGetValue(typeOfObj, out schema))
                {
                    // no schema was registered for this type, so we are going to serialize the
                    // object, capture the fields, and generate a schema for it - this requires and
                    // assumes that the serializer is not "clever" and does not omit fields for
                    // optimization reasons.
                    schema = registration.HasSchema ? registration.Schema! : BuildSchema(registration, obj);

                    if (!registration.IsClusterSchema)
                    {
                        // if the schema is not supposed to exist on the cluster, yet...
                        // that schema will need to be published before we can send any data that is
                        // using it - so we register it with the data output - and magic will happen
                        output.SchemaIds.Add(schema.Id);
                    }

                    // update our maps with the schema and its identifier
                    _schemasMap[typeOfObj] = schema;
                    _registrationsById.TryAdd(schema.Id, registration);

                    // and register it with the schema service
                    _schemas.Add(schema, registration.IsClusterSchema);
                }
                else if (!_schemas.IsPublished(schema.Id))
                {
                    // schema is registered but not published yet, maybe publication failed,
                    // needs to be published, so register it too
                    output.SchemaIds.Add(schema.Id);
                }
            }

            WriteSchema(output, schema);
            var writer = new CompactWriter(this, output, schema);
            serializer.Write(writer, obj);
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

        public bool TryRead(IObjectDataInput input, Type type, out object? obj, out long missingSchemaId)
        {
            if (input is not ObjectDataInput inputInstance)
                throw new ArgumentException("Input must be an ObjectDataInput instance.", nameof(input));
            if (type == null) throw new ArgumentNullException(nameof(type));

            return TryReadObject(inputInstance, type, out obj, out missingSchemaId);
        }

        private bool TryReadObject(ObjectDataInput input, Type type, [NotNullWhen(true)] out object? obj, out long missingSchemaId)
        {
            var schemaId = input.ReadLong();
            if (!_schemas.TryGet(schemaId, out var schema))
            {
                obj = null;
                missingSchemaId = schemaId;
                return false;
            }

            missingSchemaId = 0;
            ICompactReader reader;
            CompactSerializerAdapter serializer;

            if (type == typeof(IGenericRecord))
            {
                reader = new CompactReader(this, input, schema, typeof(IGenericRecord));
                serializer = _genericRecordSerializer;
            }
            else
            {
                var registration = GetOrCreateRegistration(schema);
                reader = new CompactReader(this, input, schema, registration.SerializedType);
                serializer = registration.Serializer;
            }

            obj = serializer.Read(reader);

            return true;
        }

        private object ReadObject(ObjectDataInput input, Type type)
        {
            if (TryReadObject(input, type, out var obj, out var missingSchemaId))
                return obj;

            // trying to de-serialize an unknown schema - nothing we can do here, the
            // situation should have been handled previously by TryReadObject etc.
            throw new UnknownCompactSchemaException(missingSchemaId);
        }

        public async Task<bool> FetchSchema(long schemaId)
        {
            // fetch the schema - if successful, it's now in _schemas, and the next call
            // to ReadObject will find it via GetOrCreateRegistration - nothing more to
            // do here.
            var schema = await _schemas.GetOrFetchAsync(schemaId).CfAwait();
            return schema != null;
        }

        public async ValueTask EnsureSchemas(byte[] input, int position)
        {
            // input contains a compact-serialized object
            // position points to the start of the object

            // get the schema
            var schemaId = input.ReadLong(position, _endianness);
            var schema = await _schemas.GetOrFetchAsync(schemaId).CfAwait();
            if (schema == null) throw new UnknownCompactSchemaException(schemaId);

            // fast exit: no reference fields = no nested schema
            if (!schema.HasReferenceFields) return;

            position += BytesExtensions.SizeOfLong; // skip schemaId

            var start = -1;
            var dataLength = -1;
            var offsetPosition = -1;
            Func<byte[], int, int, int>? offsetReader = null;

            foreach (var field in schema.Fields)
            {
                if (field.Kind != FieldKind.Compact && field.Kind != FieldKind.ArrayOfCompact)
                    continue; // does not contain an object with a schema

                if (dataLength < 0)
                {
                    // initialize once and only if needed
                    dataLength = input.ReadInt(position, _endianness);
                    start = position + BytesExtensions.SizeOfInt;
                    offsetPosition = start + dataLength;
                    offsetReader = GetOffsetReader(dataLength);
                }

                // jump to field position
                var offset = offsetReader!(input, offsetPosition, field.Index);
                if (offset < 0) continue; // null value
                position = start + offset;

                if (field.Kind == FieldKind.Compact)
                {
                    // field is a simple object, scan
                    await EnsureSchemas(input, position).CfAwait();
                }
                else
                {
                    // field is an array, navigate the array and scan each item
                    var arrayDataLength = input.ReadInt(position, _endianness);
                    position += BytesExtensions.SizeOfInt;
                    var arrayCount = input.ReadInt(position, _endianness);
                    position += BytesExtensions.SizeOfInt;
                    var arrayStart = position;
                    var arrayOffsetPosition = arrayStart + arrayDataLength;
                    var arrayOffsetReader = GetOffsetReader(arrayDataLength);
                    for (var i = 0; i < arrayCount; i++)
                    {
                        var itemOffset = arrayOffsetReader(input, arrayOffsetPosition, i);
                        if (itemOffset < 0) continue; // null value
                        position = arrayStart + itemOffset;
                        await EnsureSchemas(input, position).CfAwait();
                    }
                }
            }
        }

        private Func<byte[], int, int, int> GetOffsetReader(int dataLength)
        {
            if (dataLength < byte.MaxValue) return (input, start, index) =>
            {
                var offset = input.ReadByte(start + index * BytesExtensions.SizeOfByte);
                return offset == byte.MaxValue ? -1 : offset;
            };

            if (dataLength < ushort.MaxValue) return (input, start, index) =>
            {
                var offset = input.ReadUShort(start + index * BytesExtensions.SizeOfShort, _endianness);
                return offset == ushort.MaxValue ? -1 : offset;
            };

            return (input, start, index) =>
                input.ReadInt(start + index * BytesExtensions.SizeOfInt, _endianness); // specs say "otherwise offset are i32"
        }

        public ValueTask BeforeSendingMessage(ClientMessage message)
            => message.HasSchemas ? Schemas.PublishAsync(message.SchemaIds) : default;
    }
}
