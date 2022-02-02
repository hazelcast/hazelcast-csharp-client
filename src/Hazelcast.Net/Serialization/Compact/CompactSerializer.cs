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
using System.Collections.Generic;

namespace Hazelcast.Serialization.Compact
{
    internal sealed class CompactSerializer : IStreamSerializer<object>
    {
        private readonly Dictionary<Type, CompactSerializableRegistration> _registrationsByType = new Dictionary<Type, CompactSerializableRegistration>();
        private readonly Dictionary<long, CompactSerializableRegistration> _registrationsById = new Dictionary<long, CompactSerializableRegistration>();
        private readonly Dictionary<Type, Schema> _schemasMap = new Dictionary<Type, Schema>();
        private readonly ISchemas _schemas;

        public CompactSerializer(CompactOptions options, ISchemas schemas, Func<byte[], IObjectDataInput> createInput, Func<IObjectDataOutput> createOutput)
        {
            // note: createInput and createOutput are required to handle generic records, which we don't have yet

            _schemas = schemas;

            // FIXME - schema registration
            // we currently do *not* support creating schemas on the fly, plus they are registered as "published"
            // with ISchemas and we do not push schemas to the cluster not fetch schemas from the cluster, at all.
            //
            // this is a *big* MPV limitation as we don't have to deal with sync/async and missing schemas.

            foreach (var option in options.Registrations)
            {
                var registration = new CompactSerializableRegistration(option.Schema.TypeName, option.Type, option.Serializer);
                _registrationsById[option.Schema.Id] = registration;
                _registrationsByType[option.Type] = registration;
                _schemas.Add(option.Schema, true);
            }
        }

        public int TypeId => SerializationConstants.ConstantTypeCompact;

        public bool HasRegistrationForType(Type type) => _registrationsByType.ContainsKey(type);

        public void Dispose()
        {
            // FIXME - who's in charge of disposing _schemas which may have background tasks etc?
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

        private CompactSerializableRegistration GetOrCreateRegistration(Type type)
        {
            if (_registrationsByType.TryGetValue(type, out var registration)) return registration;

            // so... an object may have ways to indicate its serializer
            // implement ICompactable ICompactSerializable.GetCompactSerializer() : ICompactSerializer
            // be marked with [CompactSerializable(typeof(MySerializer))] attribute
            // declare the serializer in some sort of config and THEN we won't come here
            // and, finally, there is this "reflective serializer" BUT wouldn't we prefer code generation?

            // TODO: create registrations on the fly (e.g. reflective etc).
            throw new SerializationException($"Could not find a compact serializer for type {type}.");
        }

        private CompactSerializableRegistration GetOrCreateRegistration(Schema schema)
        {
            if (_registrationsById.TryGetValue(schema.Id, out var registration)) return registration;

            // TODO: create registrations on the fly (e.g. generic record etc).
            throw new SerializationException($"Could not find a compact serializer for schema {schema.Id}.");
        }

        public void WriteObject(ObjectDataOutput output, object obj, bool withSchema)
        {
            var typeOfObj = obj.GetType();
            var registration = GetOrCreateRegistration(typeOfObj);
            if (!_schemasMap.TryGetValue(typeOfObj, out var schema))
            {
                // no schema was registered for this type, so we are going to serialize
                // the object, capture the fields, and generate a schema for it
                // FIXME - implies restriction on the serialization
                // because, what-if the serializer for optimization reasons does not
                // write out all the fields?
                schema = BuildSchema(registration, obj);
                _schemasMap[typeOfObj] = schema;

                // and now, we need to publish this new schema - which we have to assume
                // is not published yet, but if withSchema is true, the schema is
                // about to be sent, and can we assume this means it is published?
                // FIXME - what-if sending the message eventually fails?
                // we end-up with a 'published' schema that will never be published ever
                // again and yet, it does not exist on the cluster maybe?
                var published = withSchema;
                _schemas.Add(schema, published);

                if (!published)
                {
                    // FIXME - Java does blocking async
                    //
                    // in order to publish the schema on the cluster but we *cannot* do
                    // this in C# and then what shall we do? if we just let the schemas
                    // service publish in the background, then we may end up sending data
                    // before the schema has been published entirely. so we want to wait.
                    //
                    // but how? we would need to return an awaitable something (Task) that
                    // would bubble up to SerializationService.ToData and then whoever is
                    // invoking ToData would need to check whether to await it before
                    // proceeding with sending the message?
                    //
                    // and then what about reading (ToObject)? it's sync by default but
                    // if the schema is n/a then we return a non-completed ValueTask that
                    // the caller would need to await? that just cannot work with lazy
                    // deserialization - have we made a decision?!
                }
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
            _schemas.Add(schema, true);
            return schema;
        }
    }
}
