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
using System.Collections.Generic;
using System.Reflection;

namespace Hazelcast.Serialization.Compact
{
    internal sealed class CompactSerializer : IStreamSerializer<object>
    {
        private readonly ConcurrentDictionary<Type, CompactSerializableRegistration> _registrationsByType = new ConcurrentDictionary<Type, CompactSerializableRegistration>();
        private readonly ConcurrentDictionary<long, CompactSerializableRegistration> _registrationsById = new ConcurrentDictionary<long, CompactSerializableRegistration>();
        private readonly ConcurrentDictionary<Type, Schema> _schemasMap = new ConcurrentDictionary<Type, Schema>();
        private readonly ISchemas _schemas;
        private readonly CompactSerializerWrapper _reflectionSerializer = new ReflectionSerializer();

        public CompactSerializer(CompactOptions options, ISchemas schemas, Func<byte[], IObjectDataInput> createInput, Func<IObjectDataOutput> createOutput)
        {
            // note: createInput and createOutput are required to handle generic records, which we don't have yet

            _schemas = schemas;

            foreach (var option in options.Registrations)
            {
                switch (option)
                {
                    case CompactOptions.RegistrationWithSchema withSchema:
                    {
                        var registration = new CompactSerializableRegistration(withSchema.Schema.TypeName, option.Type, option.Serializer, option.Published);
                        _registrationsById[withSchema.Schema.Id] = registration;
                        _registrationsByType[withSchema.Type] = registration;
                        _schemas.Add(withSchema.Schema, withSchema.Published);
                        _schemasMap[withSchema.Type] = withSchema.Schema;
                        break;
                    }
                    case CompactOptions.RegistrationWithTypeName withTypeName:
                    {
                        var registration = new CompactSerializableRegistration(withTypeName.TypeName, option.Type, option.Serializer, option.Published);
                        _registrationsByType[withTypeName.Type] = registration;
                        break;
                    }
                }
            }
        }

        public int TypeId => SerializationConstants.ConstantTypeCompact;

        // for tests
        public ISchemas Schemas => _schemas;

        public bool HasRegistrationForType(Type type) => _registrationsByType.ContainsKey(type);

        public void Dispose()
        {
            // note: ISchemas is not IDisposable because we don't have background tasks
        }

        public object Read(IObjectDataInput input)
            => Read(input, false);

        public object Read(IObjectDataInput input, bool withSchema)
        {
            if (!(input is ObjectDataInput inputInstance))
                throw new ArgumentException("Input must be an ObjectDataInput instance.", nameof(input));

            return ReadObject(inputInstance, withSchema);
        }

        public T Read<T>(IObjectDataInput input, bool withSchema)
            => SerializationService.CastObject<T>(Read(input, withSchema), true);

        public void Write(IObjectDataOutput output, object obj)
            => Write(output, obj, false);

        public void Write(IObjectDataOutput output, object obj, bool withSchema)
        {
            if (!(output is ObjectDataOutput outputInstance))
                throw new ArgumentException("Output must be an ObjectDataOutput instance.", nameof(output));

            WriteObject(outputInstance, obj, withSchema);
        }

        // invoked when writing out an object and we need its registration, ie its serializer
        // either a serializer has been registered already for the type, or the type is ICompactable
        // and can provide its own serializer, or we need to fall back to reflection-based serialization.
        // note: if we were to implement code generation, the generated serializers would be registered.
        private CompactSerializableRegistration GetOrCreateRegistration(object obj)
        {
            var typeOfObj = obj.GetType();
            return _registrationsByType.GetOrAdd(typeOfObj, type =>
            {
                // TODO: alternatively with .NET could we annotate the type with the serializer type?

                CompactSerializerWrapper? serializerWrapper = null;
                string? typeName = null;
                bool published;

                var attr = obj.GetType().GetCustomAttribute<CompactableAttribute>();

                if (obj is ICompactable compactable)
                {
                    if (attr != null) throw new SerializationException(""); // FIXME message cannot be both
                    serializerWrapper = CompactSerializerWrapper.Create(compactable);
                    typeName = compactable.TypeName ?? typeOfObj.Name;
                    published = compactable.PublishedSchema ?? false;
                }
                else
                {
                    if (attr != null)
                    {
                        serializerWrapper = CompactSerializerWrapper.Create(attr.SerializerType);
                        typeName = attr.TypeName ?? typeOfObj.Name;
                        published = attr.PublishedSchema;
                    }
                    else
                    {
                        serializerWrapper = _reflectionSerializer;
                        typeName = typeOfObj.Name;
                        published = false;
                    }
                }

                return new CompactSerializableRegistration(typeName, typeOfObj, serializerWrapper, published);
            });
        }

        // invoked when reading an object and we need its registration, ie its serializer, and all
        // we have is the schema - FIXME JAVA?
        // Java does getOrCreateRegistration(schema.getTypeName()) which means it can only handle
        // one single schema per type name and how is this supposed to work at all?!
        private CompactSerializableRegistration GetOrCreateRegistration(Schema schema)
        {
            if (_registrationsById.TryGetValue(schema.Id, out var registration)) return registration;

            // JAVA
            //
            // tries to load a class by the name of schema.TypeName
            // if it fails, returns null
            // otherwise, instantiate an object of that class, and go GetOrCreateRegistration(object obj)
            //
            // and then,
            // if the registration is null, goes the gener... UH FIXME? returns a READER?
            // at that point I honestly want to cry
            // OK, the generic record extends the reader - or, the reader extends the record and WTF?
            // also, can we be leaking ObjectDataInput here?! somebody kill me please.

            // TODO: create registrations on the fly (e.g. generic record etc).
            throw new SerializationException($"Could not find a compact serializer for schema {schema.Id}.");
        }

        public void WriteObject(ObjectDataOutput output, object obj, bool withSchema)
        {
            var typeOfObj = obj.GetType();
            var registration = GetOrCreateRegistration(obj);
            if (!_schemasMap.TryGetValue(typeOfObj, out var schema))
            {
                // no schema was registered for this type, so we are going to serialize
                // the object, capture the fields, and generate a schema for it - this
                // assumes that the serializer is not "clever" and does not omit fields
                // for optimization reasons.
                schema = BuildSchema(registration, obj);
                _schemasMap[typeOfObj] = schema;

                // and now, since we know the schema identifier, we can update our
                // registrations map
                _registrationsById.TryAdd(schema.Id, registration);

                // and now, we need to publish this new schema - which we have to assume
                // is not published yet, but if withSchema is true, the schema is about
                // to be sent, and can we assume this means it is published.
                //
                // OTOH if it is not published... it will just be added, not even sent
                // immediately to the cluster, so ToData and WriteObject remains fully
                // synchronous - but, before sending any message to the cluster, callers
                // should validate that the serialization service is ready.
                //
                // FIXME - what-if sending the withSchema message eventually fails?
                // we would end up with a local 'published' schema, which is actually not
                // known by the cluster, and will never published. should we send some
                // data with withSchema==false, what happens?
                // and, can that ever happen? what determines the withSchema value in Java?

                _schemas.Add(schema, published: withSchema || registration.PublishedSchema);
            }

            WriteSchema(output, schema, withSchema);
            var writer = new CompactWriter(this, output, schema, withSchema);
            registration.Serializer.Write(writer, obj);
            writer.Complete();
        }

        private static void WriteSchema(ObjectDataOutput output, Schema schema, bool withSchema)
        {
            output.WriteLong(schema.Id);
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
        }

        private static Schema BuildSchema(CompactSerializableRegistration registration, object obj)
        {
            var builder = new SchemaBuilderWriter(registration.TypeName);
            registration.Serializer.Write(builder, obj);
            return builder.Build();
        }

        private object ReadObject(ObjectDataInput input, bool withSchema)
        {
            var schema = ReadSchema(input, withSchema);
            var registration = GetOrCreateRegistration(schema);

            // see note in GetOrCreateRegistration - no idea what is going on here

            var reader = new CompactReader(this, input, schema, withSchema);
            var obj = registration.Serializer.Read(reader);
            if (obj == null) throw new SerializationException("Read illegal null object.");
            return obj;
        }

        private Schema ReadSchema(ObjectDataInput input, bool withSchema)
        {
            var schemaId = input.ReadLong();
            if (_schemas.TryGet(schemaId, out var schema)) return schema;
            if (!withSchema)
            {
                // FIXME - throw or return null?
                // and then properly bubble the error up so we can try to fetch from cluster etc?
                //
                // so *this* is where we either
                // - throw a CompactSchemaMissingException(schemaId) for caller to catch, fetch from
                //   server, and then retry to de-serializer => no change for ToObject at all?
                // - return a flag, a boolean, a ValueTask, anything representing that we are in need
                //   of a schema.
                //
                throw new SerializationException("Failed.");
            }

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
        }
    }
}
