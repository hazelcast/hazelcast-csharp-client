// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Serialization;
using Hazelcast.Serialization.Compact;
using Hazelcast.Testing;
using NSubstitute;
using NUnit.Framework;

namespace Hazelcast.Tests.Serialization.Compact
{
    [TestFixture]
    public class CompactSerializationSerializerTests
    {
        [Test]
        public void SerializerConstants()
        {
            var serializer = new CompactSerializationSerializer(new CompactOptions(), Substitute.For<ISchemas>(), Endianness.BigEndian);
            var adapter = new CompactSerializationSerializerAdapter(serializer, false);

            Assert.That(adapter.Serializer, Is.SameAs(serializer));
            Assert.That(serializer.TypeId, Is.EqualTo(SerializationConstants.ConstantTypeCompact));
            Assert.That(adapter.TypeId, Is.EqualTo(SerializationConstants.ConstantTypeCompact));
        }

        [Test]
        public void TryRead_Exceptions()
        {
            var serializer = new CompactSerializationSerializer(new CompactOptions(), Substitute.For<ISchemas>(), Endianness.BigEndian);

            Assert.Throws<ArgumentException>(() => serializer.TryRead(null!, typeof(object), false, out _, out _));

            var bogusObjectDataInput = Substitute.For<IObjectDataInput>();
            Assert.Throws<ArgumentException>(() => serializer.TryRead(bogusObjectDataInput, typeof(object), false, out _, out _));

            var objectDataInput = new ObjectDataInput(Array.Empty<byte>(), Substitute.For<IReadObjectsFromObjectDataInput>(), Endianness.LittleEndian);
            Assert.Throws<ArgumentNullException>(() => serializer.TryRead(objectDataInput, null, false, out _, out _));
        }

        [Test]
        public async Task EnsureSchemas1()
        {
            var schemas = Substitute.For<ISchemas>();
            var schema = SchemaBuilder.For("thing").Build();

            // getOrFetch will always return null
            schemas
                .GetOrFetchAsync(Arg.Any<long>())
                .Returns(new ValueTask<Schema?>((Schema?)null));

            var serializer = new CompactSerializationSerializer(new CompactOptions(), schemas, Endianness.BigEndian);

            var orw = Substitute.For<IReadWriteObjectsFromIObjectDataInputOutput>();
            var output = new ObjectDataOutput(1024, orw, Endianness.BigEndian);
            output.WriteLong(schema.Id);

            // will fail to fetch the schema
            await AssertEx.ThrowsAsync<UnknownCompactSchemaException>(async () => await serializer.EnsureSchemas(output.ToByteArray(), 0));
        }

        [Test]
        public async Task EnsureSchemas2()
        {
            var schemas = Substitute.For<ISchemas>();
            var schema = SchemaBuilder.For("thing").Build();

            // getOrFetch will return the schema
            schemas
                .GetOrFetchAsync(Arg.Any<long>())
                .Returns(call => new ValueTask<Schema?>(call.Arg<long>() == schema.Id ? schema : null));

            var serializer = new CompactSerializationSerializer(new CompactOptions(), schemas, Endianness.BigEndian);

            var orw = Substitute.For<IReadWriteObjectsFromIObjectDataInputOutput>();
            var output = new ObjectDataOutput(1024, orw, Endianness.BigEndian);
            output.WriteLong(schema.Id);

            // will fetch the schema, which has no ref fields = no nested schemas
            await serializer.EnsureSchemas(output.ToByteArray(), 0);
        }

        [Test]
        public async Task EnsureNestedSchemas1()
        {
            var schemas = Substitute.For<ISchemas>();
            var schema = SchemaBuilder
                .For("thing")
                .WithField("nested0", FieldKind.Compact)
                .WithField("nested1", FieldKind.Compact)
                .WithField("nested2", FieldKind.ArrayOfCompact)
                .Build();
            var schema1 = SchemaBuilder.For("thing1").Build();

            // getOrFetch will return the schemas
            schemas
                .GetOrFetchAsync(Arg.Any<long>())
                .Returns(call => new ValueTask<Schema?>(
                    call.Arg<long>() == schema.Id ? schema :
                    call.Arg<long>() == schema1.Id ? schema1 :
                    null));

            var serializer = new CompactSerializationSerializer(new CompactOptions(), schemas, Endianness.BigEndian);

            var orw = Substitute.For<IReadWriteObjectsFromIObjectDataInputOutput>();
            var output = new ObjectDataOutput(1024, orw, Endianness.BigEndian);

            output.WriteLong(schema.Id);
            output.WriteInt(2 * BytesExtensions.SizeOfLong + 2 * BytesExtensions.SizeOfInt + 2 * BytesExtensions.SizeOfByte); // data length
            // start of data

            // for an object: schema id, no fields
            output.WriteLong(schema1.Id);

            // for an array: length, count, items
            output.WriteInt(BytesExtensions.SizeOfLong); // data length
            output.WriteInt(2); // 2 items
            output.WriteLong(schema1.Id); // 2nd item
            // end of array data

            output.WriteByte(255); // offset of item (null)
            output.WriteByte(0); // offset of second item
            // end of data

            // offsets
            output.WriteByte(255); // offset of first field (null)
            output.WriteByte(0); // offset of second field (compact)
            output.WriteByte(BytesExtensions.SizeOfLong); // offset of third field (array)

            // will fetch the schema, which has ref fields and nested schemas
            await serializer.EnsureSchemas(output.ToByteArray(), 0);
        }

        [TestCase(32)]
        [TestCase(byte.MaxValue - 1)]
        [TestCase(byte.MaxValue)]
        [TestCase(ushort.MaxValue - 1)]
        [TestCase(ushort.MaxValue)]
        public async Task EnsureNestedSchemas2(int datalength)
        {
            var schemas = Substitute.For<ISchemas>();
            var schema = SchemaBuilder
                .For("thing")
                .WithField("nested0", FieldKind.Compact)
                .Build();
            var schema1 = SchemaBuilder.For("thing1").Build();

            // getOrFetch will return the schemas
            schemas
                .GetOrFetchAsync(Arg.Any<long>())
                .Returns(call => new ValueTask<Schema?>(
                    call.Arg<long>() == schema.Id ? schema :
                    call.Arg<long>() == schema1.Id ? schema1 :
                    null));

            var serializer = new CompactSerializationSerializer(new CompactOptions(), schemas, Endianness.BigEndian);

            var orw = Substitute.For<IReadWriteObjectsFromIObjectDataInputOutput>();
            var output = new ObjectDataOutput(1024, orw, Endianness.BigEndian);

            output.WriteLong(schema.Id);
            output.WriteInt(datalength); // data length
            // start of data

            // for an object: schema id, no fields
            output.WriteLong(schema1.Id);
            for (var i = BytesExtensions.SizeOfLong; i < datalength; i++)
                output.WriteByte(0); // fill with zeroes
            // end of data

            // offsets
            if (datalength < byte.MaxValue) output.WriteByte(0);
            else if (datalength < ushort.MaxValue) output.WriteShort(0);
            else output.WriteInt(0);

            // will fetch the schema, which has ref fields and nested schemas
            await serializer.EnsureSchemas(output.ToByteArray(), 0);
        }

        [Test]
        public void ReadUnknownSchema()
        {
            var schemas = Substitute.For<ISchemas>();

            var serializer = new CompactSerializationSerializer(new CompactOptions(), schemas, Endianness.BigEndian);

            var orw = Substitute.For<IReadWriteObjectsFromIObjectDataInputOutput>();
            var output = new ObjectDataOutput(1024, orw, Endianness.BigEndian);

            output.WriteLong(666L);

            var input = new ObjectDataInput(output.ToByteArray(), orw, Endianness.BigEndian);

            Assert.Throws<UnknownCompactSchemaException>(() => serializer.Read(input));
        }

        [Test]
        public void ReadImpossibleType()
        {
            var schemas = Substitute.For<ISchemas>();
            var schema = SchemaBuilder
                .For(CompactOptions.GetDefaultTypeName<ImpossibleType>())
                .WithField("value", FieldKind.Int32)
                .Build();

            // getOrFetch will return the schemas
            schemas
                .TryGet(Arg.Any<long>(), out Arg.Any<Schema>())
                .Returns(call =>
                {
                    if (call.ArgAt<long>(0) != schema.Id) return false;
                    call[1] = schema;
                    return true;

                });

            var serializer = new CompactSerializationSerializer(new CompactOptions(), schemas, Endianness.BigEndian);

            var orw = Substitute.For<IReadWriteObjectsFromIObjectDataInputOutput>();
            var output = new ObjectDataOutput(1024, orw, Endianness.BigEndian);

            output.WriteLong(schema.Id);
            output.WriteInt(BytesExtensions.SizeOfInt); // data length
            output.WriteInt(42); // value

            var input = new ObjectDataInput(output.ToByteArray(), orw, Endianness.BigEndian);

            // fails because the type cannot be constructed, and therefore we cannot create a
            // proper compact serialization registration producing an ImpossibleType = we don't
            // know how to deserialize.
            Assert.Throws<SerializationException>(() => serializer.Read<ImpossibleType>(input));

            // on the other hand, if we don't specify the type, we indicate that we accept any
            // object, and therefore since we fail to create a proper registration we fall back
            // to generic record.
            input.Position = 0;
            var obj = serializer.Read(input);
            Assert.That(obj, Is.Not.Null);
            Assert.That(obj, Is.InstanceOf<IGenericRecord>());
        }

        public class ImpossibleType
        {
            private ImpossibleType() { }
        }
    }
}
