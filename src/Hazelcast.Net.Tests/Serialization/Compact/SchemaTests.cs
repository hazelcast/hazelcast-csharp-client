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
using System.Linq;
using Hazelcast.Core;
using Hazelcast.Serialization;
using Hazelcast.Serialization.Compact;
using Hazelcast.Serialization.ConstantSerializers;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Hazelcast.Tests.Serialization.Compact
{
    [TestFixture]
    public class SchemaTests
    {
        private static Schema GetSchema() => GetSchema("typename", "fieldname");

        private static Schema GetSchema(string typename, string fieldname) => new Schema(typename, new[]
        {
            new SchemaField(fieldname, FieldKind.String)
        });

        [Test]
        public void Exceptions()
        {
            Assert.Throws<ArgumentException>(() => new Schema(null, Array.Empty<SchemaField>()));
            Assert.Throws<ArgumentException>(() => new Schema("", Array.Empty<SchemaField>()));
            Assert.Throws<ArgumentException>(() => new Schema("    ", Array.Empty<SchemaField>()));

            Assert.Throws<ArgumentNullException>(() => new Schema("typename", null));

            Assert.Throws<ArgumentOutOfRangeException>(() => new CompactSerializationHook().CreateFactory().Create(666));

            Assert.Throws<ArgumentException>(() => new Schema("thing", new[]
            {
                new SchemaField("field1", FieldKind.Boolean),
                new SchemaField("field1", FieldKind.Boolean) // duplicate name
            }));

            Assert.Throws<ArgumentException>(() => new Schema("thing", new[]
            {
                new SchemaField("field1", FieldKind.Boolean),
                null // null field
            }));

            Assert.Throws<ArgumentNullException>(() => new Schema().ReadData(null));
            Assert.Throws<ArgumentNullException>(() => new Schema().WriteData(null));
        }

        [Test]
        public void FieldsAndNames()
        {
            var schema = new Schema("thing", new[]
            {
                new SchemaField("field1", FieldKind.Boolean),
                new SchemaField("field2", FieldKind.Boolean)
            });

            var fields = schema.Fields;
            var fieldNames = schema.FieldNames;

            Assert.That(fields.Count, Is.EqualTo(2));
            Assert.That(fieldNames.Count, Is.EqualTo(2));

            for (var i = 0; i < fields.Count; i++)
                Assert.That(fields[i].FieldName, Is.EqualTo(fieldNames[i]));
        }

        [Test]
        public void CanSerializeSchemaAsIdentifiedData()
        {
            // note: passing null as serialization service is ok as long as we don't have to
            // handle objects, 'cos ObjectDataInput/Output delegate them to the serialization
            // service

            var schema0 = GetSchema();
            var output = new ObjectDataOutput(1024, null, Endianness.LittleEndian);

            Assert.That(schema0.FactoryId, Is.EqualTo(CompactSerializationHook.Constants.FactoryId));
            Assert.That(schema0.ClassId, Is.EqualTo(CompactSerializationHook.Constants.ClassIds.Schema));

            var schema1 = new Schema();

            schema0.WriteData(output);
            var bytes = output.ToByteArray();
            var input = new ObjectDataInput(bytes, null, Endianness.LittleEndian);
            schema1.ReadData(input);

            Assert.That(schema1.Id, Is.EqualTo(schema0.Id));
        }

        [Test]
        public void SerializationServiceCanSerializeSchema()
        {
            var serializationService = new SerializationServiceBuilder(new NullLoggerFactory())
                .AddHook<CompactSerializationHook>()
                .AddDefinitions(new ConstantSerializerDefinitions())
                .Build();

            var schema0 = GetSchema();
            var data = serializationService.ToData(schema0);
            var schema1 = serializationService.ToObject<Schema>(data);

            Assert.That(schema1.Id, Is.EqualTo(schema0.Id));
        }

        [Test]
        public void Equality()
        {
            var schema1 = SchemaBuilder.For("thing").Build();

#pragma warning disable CS1718 // Comparison made to same variable - on purpose
            // ReSharper disable once EqualExpressionComparison - on purpose
            Assert.That(schema1 == schema1);
#pragma warning restore CS1718 // Comparison made to same variable

            Assert.That(schema1.Equals(schema1));
            Assert.That(schema1, Is.EqualTo(schema1));

            var schema2 = SchemaBuilder.For("thing").Build();

            Assert.That(schema1 == schema2);
            Assert.That(schema1.Equals(schema2));
            Assert.That(schema1, Is.EqualTo(schema2));
            Assert.That(schema1.GetHashCode(), Is.EqualTo(schema2.GetHashCode()));

            var schema3 = SchemaBuilder.For("different").Build();

            Assert.That(schema1 != schema3);
            Assert.That(!schema1.Equals(schema3));
            Assert.That(schema1, Is.Not.EqualTo(schema3));
            Assert.That(schema1.GetHashCode(), Is.Not.EqualTo(schema3.GetHashCode()));

            Assert.That(Equals(schema1, schema2));
            Assert.That(!Equals(schema1, schema3));
        }
    }
}
