﻿// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
            => ReadObject(input.MustBe<ObjectDataInput>(nameof(input)), typeof(object), false);

        public object Read(IObjectDataInput input, bool withSchemas)
            => ReadObject(input.MustBe<ObjectDataInput>(nameof(input)), typeof(object), withSchemas);

        public object Read(IObjectDataInput input, Type type)
            => SerializationService.CastObject(ReadObject(input.MustBe<ObjectDataInput>(nameof(input)), type, false), type, true);

        /// <inheritdoc cref="IStreamSerializer{T}.Read"/>
        public T Read<T>(IObjectDataInput input)
            => SerializationService.CastObject<T>(Read(input, typeof(T)), true);

        public void Write(IObjectDataOutput output, object obj)
            => Write(output, obj, false);

        public void Write(IObjectDataOutput output, object obj, bool withSchemas)
            => WriteObject(output.MustBe<ObjectDataOutput>(nameof(output)), obj, withSchemas);

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

        // invoked when reading an object and we need its registration, ie its serializer,
        // and all we have is the schema
        // Java does getOrCreateRegistration(schema.getTypeName()) which means it can only handle
        // one single schema per type name and how is this supposed to work at all?!
        private CompactRegistration? GetOrCreateRegistration(Schema schema)
        {
            if (_registrationsById.TryGetValue(schema.Id, out var registration)) return registration;

            // otherwise, all we have is the type-name and we need a registration
            // note: no race cond. here, everything will TryAdd anyways

            registration = GetOrCreateRegistration_ByTypeName(schema) ??
                           GetOrCreateRegistration_ByClrType(schema);

            return registration;
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

        public string GetTypeName(Type type)
        {
            return _registrationsByType.TryGetValue(type, out var registration)
                ? registration.TypeName
                : CompactOptions.GetDefaultTypeName(type);
        }

        public void WriteObject(ObjectDataOutput output, object obj, bool withSchemas)
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
                    _schemas.Add(schema, registration.IsClusterSchema);

                    // update our maps with the schema and its identifier
                    _schemasMap[typeOfObj] = schema;
                    _registrationsById.TryAdd(schema.Id, registration);
                }
            }

            // if the schema is not published yet, register it for publishing before any data is sent
            if (!_schemas.IsPublished(schema.Id)) output.SchemaIds.Add(schema.Id);

            WriteSchema(output, schema, withSchemas);
            var writer = new CompactWriter(this, output, schema);
            serializer.Write(writer, obj);
            writer.Complete();
        }

        private static void WriteSchema(ObjectDataOutput output, Schema schema, bool withSchemas)
        {
            output.WriteLong(schema.Id);

            if (!withSchemas) return;

            var startPosition = output.Position;
            output.WriteInt(0);
            var schemaPosition = output.Position;

            // note: don't output.WriteObject(schema) else it's serialized as identified serializable
            //
            // except, Java does 'output.WriteObject(schema)' and serializes as identified
            // serializable, ignoring the originally specified way of embedding a schema. so,
            // we do the same here.

            output.WriteObject(schema);

            //output.WriteString(schema.TypeName);
            //output.WriteInt(schema.Fields.Count);
            //foreach (var field in schema.Fields)
            //{
            //    output.WriteString(field.FieldName);
            //    output.WriteInt((int)field.Kind);
            //}

            var position = output.Position;
            output.MoveTo(startPosition);
            output.WriteInt(position - schemaPosition);
            output.MoveTo(position);
        }

        private static Schema BuildSchema(CompactRegistration registration, object obj)
        {
            var builder = new SchemaBuilderWriter(registration.TypeName);
            registration.Serializer.Write(builder, obj);
            return builder.Build();
        }

        public bool TryRead(IObjectDataInput input, Type type, bool withSchemas, out object? obj, out long missingSchemaId)
        {
            if (input is not ObjectDataInput inputInstance)
                throw new ArgumentException("Input must be an ObjectDataInput instance.", nameof(input));
            if (type == null) throw new ArgumentNullException(nameof(type));

            return TryReadObject(inputInstance, type, withSchemas, out obj, out missingSchemaId);
        }

        private bool TryReadObject(ObjectDataInput input, Type type, bool withSchemas, [NotNullWhen(true)] out object? obj, out long missingSchemaId)
        {
            if (!TryReadSchema(input, withSchemas, out var schema, out missingSchemaId))
            {
                obj = null;
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

                if (registration != null)
                {
                    reader = new CompactReader(this, input, schema, registration.SerializedType);
                    serializer = registration.Serializer;
                }
                else if (type.IsAssignableFrom(typeof(IGenericRecord)))
                {
                    reader = new CompactReader(this, input, schema, typeof(IGenericRecord));
                    serializer = _genericRecordSerializer;
                }
                else
                {
                    throw new SerializationException($"Could not find a compact serializer for schema {schema.Id}.");
                }
            }

            obj = serializer.Read(reader);

            return true;
        }

        private bool TryReadSchema(ObjectDataInput input, bool withSchemas, [NotNullWhen(true)] out Schema? schema, out long missingSchemaId)
        {
            missingSchemaId = 0;

            var schemaId = input.ReadLong();
            if (_schemas.TryGet(schemaId, out schema))
            {
                if (withSchemas) // skip the schema if provided
                {
                    var schemaSize = input.ReadInt();
                    input.Position += schemaSize;
                }
                return true;
            }

            if (!withSchemas)
            {
                missingSchemaId = schemaId;
                return false;
            }

            // note: don't input.ReadObject<Schema>() else it's serialized as identified serializable

            // except... see note in WriteSchema

            input.ReadInt(); // skip the size
            schema = input.ReadObject<Schema>(); // read the schema

            //var typeName = input.ReadString();
            //var fieldCount = input.ReadInt();
            //var fields = new SchemaField[fieldCount];
            //for (var i = 0; i < fieldCount; i++)
            //{
            //    var fieldName = input.ReadString();
            //    var kind = FieldKindEnum.Parse(input.ReadInt());
            //    fields[i] = new SchemaField(fieldName, kind);
            //}

            //schema = new Schema(typeName, fields);

            _schemas.Add(schema, true);
            return true;
        }

        private object ReadObject(ObjectDataInput input, Type type, bool withSchemas)
        {
            if (TryReadObject(input, type, withSchemas, out var obj, out var missingSchemaId))
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

        public ValueTask BeforeSendingMessage(ClientMessage message, Guid targetMemberId)
            => message.HasSchemas ? Schemas.PublishAsync(message.SchemaIds) : default;
    }
}
