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
using Hazelcast.Core;
using Hazelcast.Serialization;
using Hazelcast.Serialization.Compact;
using Moq;
using NUnit.Framework;

namespace Hazelcast.Tests.Serialization.Compact
{
    [TestFixture]
    public class CompactReaderTests
    {
        [Test]
        public void CanReadNonNullableAsNullable()
        {
            var orw = Mock.Of<IReadWriteObjectsFromIObjectDataInputOutput>();

            var schema = SchemaBuilder.For("type").WithField("field", FieldKind.Int8).Build();
            var output = new ObjectDataOutput(1024, orw, Endianness.LittleEndian);
            var writer = new CompactWriter(orw, output, schema);
            writer.WriteInt8("field", 42);
            writer.Complete();

            var buffer = output.ToByteArray();

            var input = new ObjectDataInput(buffer, orw, Endianness.LittleEndian);
            var reader = new CompactReader(orw, input, schema, typeof(object));

            Assert.That(reader.ReadInt8("field"), Is.EqualTo(42));
            Assert.That(reader.ReadNullableInt8("field"), Is.EqualTo(42));
        }

        [Test]
        public void CanReadNonNullableBooleanAsNullable()
        {
            var orw = Mock.Of<IReadWriteObjectsFromIObjectDataInputOutput>();

            var schema = SchemaBuilder.For("type").WithField("field", FieldKind.Boolean).Build();
            var output = new ObjectDataOutput(1024, orw, Endianness.LittleEndian);
            var writer = new CompactWriter(orw, output, schema);
            writer.WriteBoolean("field", true);
            writer.Complete();

            var buffer = output.ToByteArray();

            var input = new ObjectDataInput(buffer, orw, Endianness.LittleEndian);
            var reader = new CompactReader(orw, input, schema, typeof(object));

            Assert.That(reader.ReadBoolean("field"), Is.True);
            Assert.That(reader.ReadNullableBoolean("field"), Is.True);
        }

        [Test]
        public void CanReadNullableNotNullAsNonNullable()
        {
            var orw = Mock.Of<IReadWriteObjectsFromIObjectDataInputOutput>();

            var schema = SchemaBuilder.For("type").WithField("field", FieldKind.NullableInt8).Build();
            var output = new ObjectDataOutput(1024, orw, Endianness.LittleEndian);
            var writer = new CompactWriter(orw, output, schema);
            writer.WriteNullableInt8("field", 42);
            writer.Complete();

            var buffer = output.ToByteArray();

            var input = new ObjectDataInput(buffer, orw, Endianness.LittleEndian);
            var reader = new CompactReader(orw, input, schema, typeof(object));

            Assert.That(reader.ReadInt8("field"), Is.EqualTo(42));
            Assert.That(reader.ReadNullableInt8("field"), Is.EqualTo(42));
        }

        [Test]
        public void CanReadNullableNotNullBooleanAsNonNullable()
        {
            var orw = Mock.Of<IReadWriteObjectsFromIObjectDataInputOutput>();

            var schema = SchemaBuilder.For("type").WithField("field", FieldKind.NullableBoolean).Build();
            var output = new ObjectDataOutput(1024, orw, Endianness.LittleEndian);
            var writer = new CompactWriter(orw, output, schema);
            writer.WriteNullableBoolean("field", true);
            writer.Complete();

            var buffer = output.ToByteArray();

            var input = new ObjectDataInput(buffer, orw, Endianness.LittleEndian);
            var reader = new CompactReader(orw, input, schema, typeof(object));

            Assert.That(reader.ReadBoolean("field"), Is.True);
            Assert.That(reader.ReadNullableBoolean("field"), Is.True);
        }

        [Test]
        public void CannotReadNullableNullAsNonNullable()
        {
            var orw = Mock.Of<IReadWriteObjectsFromIObjectDataInputOutput>();

            var schema = SchemaBuilder.For("type").WithField("field", FieldKind.NullableInt8).Build();
            var output = new ObjectDataOutput(1024, orw, Endianness.LittleEndian);
            var writer = new CompactWriter(orw, output, schema);
            writer.WriteNullableInt8("field", null);
            writer.Complete();

            var buffer = output.ToByteArray();

            var input = new ObjectDataInput(buffer, orw, Endianness.LittleEndian);
            var reader = new CompactReader(orw, input, schema, typeof(object));

            Assert.Throws<SerializationException>(() => reader.ReadInt8("field"));
            Assert.That(reader.ReadNullableInt8("field"), Is.Null);
        }

        [Test]
        public void CannotReadNullableNullBooleanAsNonNullable()
        {
            var orw = Mock.Of<IReadWriteObjectsFromIObjectDataInputOutput>();

            var schema = SchemaBuilder.For("type").WithField("field", FieldKind.NullableBoolean).Build();
            var output = new ObjectDataOutput(1024, orw, Endianness.LittleEndian);
            var writer = new CompactWriter(orw, output, schema);
            writer.WriteNullableBoolean("field", null);
            writer.Complete();

            var buffer = output.ToByteArray();

            var input = new ObjectDataInput(buffer, orw, Endianness.LittleEndian);
            var reader = new CompactReader(orw, input, schema, typeof(object));

            Assert.Throws<SerializationException>(() => reader.ReadBoolean("field"));
            Assert.That(reader.ReadNullableBoolean("field"), Is.Null);
        }

        [Test]
        public void CanReadNonNullableArrayOfBooleanAsNullable()
        {
            var orw = Mock.Of<IReadWriteObjectsFromIObjectDataInputOutput>();

            var schema = SchemaBuilder.For("type").WithField("field", FieldKind.ArrayOfBoolean).Build();
            var output = new ObjectDataOutput(1024, orw, Endianness.LittleEndian);
            var writer = new CompactWriter(orw, output, schema);
            writer.WriteArrayOfBoolean("field", new bool[] { true, false, true, false });
            writer.Complete();

            var buffer = output.ToByteArray();

            var input = new ObjectDataInput(buffer, orw, Endianness.LittleEndian);
            var reader = new CompactReader(orw, input, schema, typeof(object));

            var value0 = reader.ReadArrayOfBoolean("field");
            Assert.That(value0, Is.Not.Null);
            var value1 = reader.ReadArrayOfNullableBoolean("field");
            Assert.That(value1, Is.Not.Null);

            Assert.That(value0.Length, Is.EqualTo(value1.Length));
            for (var i = 0; i < value0.Length; i++) Assert.That(value0[i], Is.EqualTo(value1[i]));
        }

        [Test]
        public void CanReadNullableArrayOfBooleanNotNullAsNonNullable()
        {
            var orw = Mock.Of<IReadWriteObjectsFromIObjectDataInputOutput>();

            var schema = SchemaBuilder.For("type").WithField("field", FieldKind.ArrayOfNullableBoolean).Build();
            var output = new ObjectDataOutput(1024, orw, Endianness.LittleEndian);
            var writer = new CompactWriter(orw, output, schema);
            writer.WriteArrayOfNullableBoolean("field", new bool?[] { true, false, true, false });
            writer.Complete();

            var buffer = output.ToByteArray();

            var input = new ObjectDataInput(buffer, orw, Endianness.LittleEndian);
            var reader = new CompactReader(orw, input, schema, typeof(object));

            var value0 = reader.ReadArrayOfBoolean("field");
            Assert.That(value0, Is.Not.Null);
            var value1 = reader.ReadArrayOfNullableBoolean("field");
            Assert.That(value1, Is.Not.Null);

            Assert.That(value0.Length, Is.EqualTo(value1.Length));
            for (var i = 0; i < value0.Length; i++) Assert.That(value0[i], Is.EqualTo(value1[i]));
        }

        [Test]
        public void CannotReadNullableArrayOfBooleanNullAsNonNullable()
        {
            var orw = Mock.Of<IReadWriteObjectsFromIObjectDataInputOutput>();

            var schema = SchemaBuilder.For("type").WithField("field", FieldKind.ArrayOfNullableBoolean).Build();
            var output = new ObjectDataOutput(1024, orw, Endianness.LittleEndian);
            var writer = new CompactWriter(orw, output, schema);
            writer.WriteArrayOfNullableBoolean("field", new bool?[] { true, false, null, true });
            writer.Complete();

            var buffer = output.ToByteArray();

            var input = new ObjectDataInput(buffer, orw, Endianness.LittleEndian);
            var reader = new CompactReader(orw, input, schema, typeof(object));

            Assert.Throws<SerializationException>(() => reader.ReadArrayOfBoolean("field"));
            var value1 = reader.ReadArrayOfNullableBoolean("field");
        }

        [Test]
        public void CanReadNonNullableArrayOfInt8AsNullable()
        {
            var orw = Mock.Of<IReadWriteObjectsFromIObjectDataInputOutput>();

            var schema = SchemaBuilder.For("type").WithField("field", FieldKind.ArrayOfInt8).Build();
            var output = new ObjectDataOutput(1024, orw, Endianness.LittleEndian);
            var writer = new CompactWriter(orw, output, schema);
            writer.WriteArrayOfInt8("field", new sbyte[] { 1, 2, 3, 4 });
            writer.Complete();

            var buffer = output.ToByteArray();

            var input = new ObjectDataInput(buffer, orw, Endianness.LittleEndian);
            var reader = new CompactReader(orw, input, schema, typeof(object));

            var value0 = reader.ReadArrayOfInt8("field");
            Assert.That(value0, Is.Not.Null);
            var value1 = reader.ReadArrayOfNullableInt8("field");
            Assert.That(value1, Is.Not.Null);

            Assert.That(value0.Length, Is.EqualTo(value1.Length));
            for (var i = 0; i < value0.Length; i++) Assert.That(value0[i], Is.EqualTo(value1[i]));
        }

        [Test]
        public void CanReadNullableArrayOfInt8NotNullAsNonNullable()
        {
            var orw = Mock.Of<IReadWriteObjectsFromIObjectDataInputOutput>();

            var schema = SchemaBuilder.For("type").WithField("field", FieldKind.ArrayOfNullableInt8).Build();
            var output = new ObjectDataOutput(1024, orw, Endianness.LittleEndian);
            var writer = new CompactWriter(orw, output, schema);
            writer.WriteArrayOfNullableInt8("field", new sbyte?[] { 1, 2, 3, 4 });
            writer.Complete();

            var buffer = output.ToByteArray();

            var input = new ObjectDataInput(buffer, orw, Endianness.LittleEndian);
            var reader = new CompactReader(orw, input, schema, typeof(object));

            var value0 = reader.ReadArrayOfInt8("field");
            Assert.That(value0, Is.Not.Null);
            var value1 = reader.ReadArrayOfNullableInt8("field");
            Assert.That(value1, Is.Not.Null);

            Assert.That(value0.Length, Is.EqualTo(value1.Length));
            for (var i = 0; i < value0.Length; i++) Assert.That(value0[i], Is.EqualTo(value1[i]));
        }

        [Test]
        public void CannotReadNullableArrayOfInt8NullAsNonNullable()
        {
            var orw = Mock.Of<IReadWriteObjectsFromIObjectDataInputOutput>();

            var schema = SchemaBuilder.For("type").WithField("field", FieldKind.ArrayOfNullableInt8).Build();
            var output = new ObjectDataOutput(1024, orw, Endianness.LittleEndian);
            var writer = new CompactWriter(orw, output, schema);
            writer.WriteArrayOfNullableInt8("field", new sbyte?[] { 1, 2, null, 4 });
            writer.Complete();

            var buffer = output.ToByteArray();

            var input = new ObjectDataInput(buffer, orw, Endianness.LittleEndian);
            var reader = new CompactReader(orw, input, schema, typeof(object));

            Assert.Throws<SerializationException>(() => reader.ReadArrayOfInt8("field"));
            var value1 = reader.ReadArrayOfNullableInt8("field");
        }

        [Test]
        public void CanReadNonNullableArrayOfInt16AsNullable()
        {
            var orw = Mock.Of<IReadWriteObjectsFromIObjectDataInputOutput>();

            var schema = SchemaBuilder.For("type").WithField("field", FieldKind.ArrayOfInt16).Build();
            var output = new ObjectDataOutput(1024, orw, Endianness.LittleEndian);
            var writer = new CompactWriter(orw, output, schema);
            writer.WriteArrayOfInt16("field", new short[] { 1, 2, 3, 4 });
            writer.Complete();

            var buffer = output.ToByteArray();

            var input = new ObjectDataInput(buffer, orw, Endianness.LittleEndian);
            var reader = new CompactReader(orw, input, schema, typeof(object));

            var value0 = reader.ReadArrayOfInt16("field");
            Assert.That(value0, Is.Not.Null);
            var value1 = reader.ReadArrayOfNullableInt16("field");
            Assert.That(value1, Is.Not.Null);

            Assert.That(value0.Length, Is.EqualTo(value1.Length));
            for (var i = 0; i < value0.Length; i++) Assert.That(value0[i], Is.EqualTo(value1[i]));
        }

        [Test]
        public void CanReadNullableArrayOfInt16NotNullAsNonNullable()
        {
            var orw = Mock.Of<IReadWriteObjectsFromIObjectDataInputOutput>();

            var schema = SchemaBuilder.For("type").WithField("field", FieldKind.ArrayOfNullableInt16).Build();
            var output = new ObjectDataOutput(1024, orw, Endianness.LittleEndian);
            var writer = new CompactWriter(orw, output, schema);
            writer.WriteArrayOfNullableInt16("field", new short?[] { 1, 2, 3, 4 });
            writer.Complete();

            var buffer = output.ToByteArray();

            var input = new ObjectDataInput(buffer, orw, Endianness.LittleEndian);
            var reader = new CompactReader(orw, input, schema, typeof(object));

            var value0 = reader.ReadArrayOfInt16("field");
            Assert.That(value0, Is.Not.Null);
            var value1 = reader.ReadArrayOfNullableInt16("field");
            Assert.That(value1, Is.Not.Null);

            Assert.That(value0.Length, Is.EqualTo(value1.Length));
            for (var i = 0; i < value0.Length; i++) Assert.That(value0[i], Is.EqualTo(value1[i]));
        }

        [Test]
        public void CannotReadNullableArrayOfInt16NullAsNonNullable()
        {
            var orw = Mock.Of<IReadWriteObjectsFromIObjectDataInputOutput>();

            var schema = SchemaBuilder.For("type").WithField("field", FieldKind.ArrayOfNullableInt16).Build();
            var output = new ObjectDataOutput(1024, orw, Endianness.LittleEndian);
            var writer = new CompactWriter(orw, output, schema);
            writer.WriteArrayOfNullableInt16("field", new short?[] { 1, 2, null, 4 });
            writer.Complete();

            var buffer = output.ToByteArray();

            var input = new ObjectDataInput(buffer, orw, Endianness.LittleEndian);
            var reader = new CompactReader(orw, input, schema, typeof(object));

            Assert.Throws<SerializationException>(() => reader.ReadArrayOfInt16("field"));
            var value1 = reader.ReadArrayOfNullableInt16("field");
        }

        [Test]
        public void CanReadNonNullableArrayOfInt32AsNullable()
        {
            var orw = Mock.Of<IReadWriteObjectsFromIObjectDataInputOutput>();

            var schema = SchemaBuilder.For("type").WithField("field", FieldKind.ArrayOfInt32).Build();
            var output = new ObjectDataOutput(1024, orw, Endianness.LittleEndian);
            var writer = new CompactWriter(orw, output, schema);
            writer.WriteArrayOfInt32("field", new[] { 1, 2, 3, 4 });
            writer.Complete();

            var buffer = output.ToByteArray();

            var input = new ObjectDataInput(buffer, orw, Endianness.LittleEndian);
            var reader = new CompactReader(orw, input, schema, typeof(object));

            var value0 = reader.ReadArrayOfInt32("field");
            Assert.That(value0, Is.Not.Null);
            var value1 = reader.ReadArrayOfNullableInt32("field");
            Assert.That(value1, Is.Not.Null);

            Assert.That(value0.Length, Is.EqualTo(value1.Length));
            for (var i = 0; i < value0.Length; i++) Assert.That(value0[i], Is.EqualTo(value1[i]));
        }

        [Test]
        public void CanReadNullableArrayOfInt32NotNullAsNonNullable()
        {
            var orw = Mock.Of<IReadWriteObjectsFromIObjectDataInputOutput>();

            var schema = SchemaBuilder.For("type").WithField("field", FieldKind.ArrayOfNullableInt32).Build();
            var output = new ObjectDataOutput(1024, orw, Endianness.LittleEndian);
            var writer = new CompactWriter(orw, output, schema);
            writer.WriteArrayOfNullableInt32("field", new int?[] { 1, 2, 3, 4 });
            writer.Complete();

            var buffer = output.ToByteArray();

            var input = new ObjectDataInput(buffer, orw, Endianness.LittleEndian);
            var reader = new CompactReader(orw, input, schema, typeof(object));

            var value0 = reader.ReadArrayOfInt32("field");
            Assert.That(value0, Is.Not.Null);
            var value1 = reader.ReadArrayOfNullableInt32("field");
            Assert.That(value1, Is.Not.Null);

            Assert.That(value0.Length, Is.EqualTo(value1.Length));
            for (var i = 0; i < value0.Length; i++) Assert.That(value0[i], Is.EqualTo(value1[i]));
        }

        [Test]
        public void CannotReadNullableArrayOfInt32NullAsNonNullable()
        {
            var orw = Mock.Of<IReadWriteObjectsFromIObjectDataInputOutput>();

            var schema = SchemaBuilder.For("type").WithField("field", FieldKind.ArrayOfNullableInt32).Build();
            var output = new ObjectDataOutput(1024, orw, Endianness.LittleEndian);
            var writer = new CompactWriter(orw, output, schema);
            writer.WriteArrayOfNullableInt32("field", new int?[] { 1, 2, null, 4 });
            writer.Complete();

            var buffer = output.ToByteArray();

            var input = new ObjectDataInput(buffer, orw, Endianness.LittleEndian);
            var reader = new CompactReader(orw, input, schema, typeof(object));

            Assert.Throws<SerializationException>(() => reader.ReadArrayOfInt32("field"));
            var value1 = reader.ReadArrayOfNullableInt32("field");
        }

        [Test]
        public void CanReadNonNullableArrayOfInt64AsNullable()
        {
            var orw = Mock.Of<IReadWriteObjectsFromIObjectDataInputOutput>();

            var schema = SchemaBuilder.For("type").WithField("field", FieldKind.ArrayOfInt64).Build();
            var output = new ObjectDataOutput(1024, orw, Endianness.LittleEndian);
            var writer = new CompactWriter(orw, output, schema);
            writer.WriteArrayOfInt64("field", new long[] { 1, 2, 3, 4 });
            writer.Complete();

            var buffer = output.ToByteArray();

            var input = new ObjectDataInput(buffer, orw, Endianness.LittleEndian);
            var reader = new CompactReader(orw, input, schema, typeof(object));

            var value0 = reader.ReadArrayOfInt64("field");
            Assert.That(value0, Is.Not.Null);
            var value1 = reader.ReadArrayOfNullableInt64("field");
            Assert.That(value1, Is.Not.Null);

            Assert.That(value0.Length, Is.EqualTo(value1.Length));
            for (var i = 0; i < value0.Length; i++) Assert.That(value0[i], Is.EqualTo(value1[i]));
        }

        [Test]
        public void CanReadNullableArrayOfInt64NotNullAsNonNullable()
        {
            var orw = Mock.Of<IReadWriteObjectsFromIObjectDataInputOutput>();

            var schema = SchemaBuilder.For("type").WithField("field", FieldKind.ArrayOfNullableInt64).Build();
            var output = new ObjectDataOutput(1024, orw, Endianness.LittleEndian);
            var writer = new CompactWriter(orw, output, schema);
            writer.WriteArrayOfNullableInt64("field", new long?[] { 1, 2, 3, 4 });
            writer.Complete();

            var buffer = output.ToByteArray();

            var input = new ObjectDataInput(buffer, orw, Endianness.LittleEndian);
            var reader = new CompactReader(orw, input, schema, typeof(object));

            var value0 = reader.ReadArrayOfInt64("field");
            Assert.That(value0, Is.Not.Null);
            var value1 = reader.ReadArrayOfNullableInt64("field");
            Assert.That(value1, Is.Not.Null);

            Assert.That(value0.Length, Is.EqualTo(value1.Length));
            for (var i = 0; i < value0.Length; i++) Assert.That(value0[i], Is.EqualTo(value1[i]));
        }

        [Test]
        public void CannotReadNullableArrayOfInt64NullAsNonNullable()
        {
            var orw = Mock.Of<IReadWriteObjectsFromIObjectDataInputOutput>();

            var schema = SchemaBuilder.For("type").WithField("field", FieldKind.ArrayOfNullableInt64).Build();
            var output = new ObjectDataOutput(1024, orw, Endianness.LittleEndian);
            var writer = new CompactWriter(orw, output, schema);
            writer.WriteArrayOfNullableInt64("field", new long?[] { 1, 2, null, 4 });
            writer.Complete();

            var buffer = output.ToByteArray();

            var input = new ObjectDataInput(buffer, orw, Endianness.LittleEndian);
            var reader = new CompactReader(orw, input, schema, typeof(object));

            Assert.Throws<SerializationException>(() => reader.ReadArrayOfInt64("field"));
            var value1 = reader.ReadArrayOfNullableInt64("field");
        }

        [Test]
        public void CanReadNonNullableArrayOfFloat32AsNullable()
        {
            var orw = Mock.Of<IReadWriteObjectsFromIObjectDataInputOutput>();

            var schema = SchemaBuilder.For("type").WithField("field", FieldKind.ArrayOfFloat32).Build();
            var output = new ObjectDataOutput(1024, orw, Endianness.LittleEndian);
            var writer = new CompactWriter(orw, output, schema);
            writer.WriteArrayOfFloat32("field", new float[] { 1, 2, 3, 4 });
            writer.Complete();

            var buffer = output.ToByteArray();

            var input = new ObjectDataInput(buffer, orw, Endianness.LittleEndian);
            var reader = new CompactReader(orw, input, schema, typeof(object));

            var value0 = reader.ReadArrayOfFloat32("field");
            Assert.That(value0, Is.Not.Null);
            var value1 = reader.ReadArrayOfNullableFloat32("field");
            Assert.That(value1, Is.Not.Null);

            Assert.That(value0.Length, Is.EqualTo(value1.Length));
            for (var i = 0; i < value0.Length; i++) Assert.That(value0[i], Is.EqualTo(value1[i]));
        }

        [Test]
        public void CanReadNullableArrayOfFloat32NotNullAsNonNullable()
        {
            var orw = Mock.Of<IReadWriteObjectsFromIObjectDataInputOutput>();

            var schema = SchemaBuilder.For("type").WithField("field", FieldKind.ArrayOfNullableFloat32).Build();
            var output = new ObjectDataOutput(1024, orw, Endianness.LittleEndian);
            var writer = new CompactWriter(orw, output, schema);
            writer.WriteArrayOfNullableFloat32("field", new float?[] { 1, 2, 3, 4 });
            writer.Complete();

            var buffer = output.ToByteArray();

            var input = new ObjectDataInput(buffer, orw, Endianness.LittleEndian);
            var reader = new CompactReader(orw, input, schema, typeof(object));

            var value0 = reader.ReadArrayOfFloat32("field");
            Assert.That(value0, Is.Not.Null);
            var value1 = reader.ReadArrayOfNullableFloat32("field");
            Assert.That(value1, Is.Not.Null);

            Assert.That(value0.Length, Is.EqualTo(value1.Length));
            for (var i = 0; i < value0.Length; i++) Assert.That(value0[i], Is.EqualTo(value1[i]));
        }

        [Test]
        public void CannotReadNullableArrayOfFloat32NullAsNonNullable()
        {
            var orw = Mock.Of<IReadWriteObjectsFromIObjectDataInputOutput>();

            var schema = SchemaBuilder.For("type").WithField("field", FieldKind.ArrayOfNullableFloat32).Build();
            var output = new ObjectDataOutput(1024, orw, Endianness.LittleEndian);
            var writer = new CompactWriter(orw, output, schema);
            writer.WriteArrayOfNullableFloat32("field", new float?[] { 1, 2, null, 4 });
            writer.Complete();

            var buffer = output.ToByteArray();

            var input = new ObjectDataInput(buffer, orw, Endianness.LittleEndian);
            var reader = new CompactReader(orw, input, schema, typeof(object));

            Assert.Throws<SerializationException>(() => reader.ReadArrayOfFloat32("field"));
            var value1 = reader.ReadArrayOfNullableFloat32("field");
        }

        [Test]
        public void CanReadNonNullableArrayOfFloat64AsNullable()
        {
            var orw = Mock.Of<IReadWriteObjectsFromIObjectDataInputOutput>();

            var schema = SchemaBuilder.For("type").WithField("field", FieldKind.ArrayOfFloat64).Build();
            var output = new ObjectDataOutput(1024, orw, Endianness.LittleEndian);
            var writer = new CompactWriter(orw, output, schema);
            writer.WriteArrayOfFloat64("field", new double[] { 1, 2, 3, 4 });
            writer.Complete();

            var buffer = output.ToByteArray();

            var input = new ObjectDataInput(buffer, orw, Endianness.LittleEndian);
            var reader = new CompactReader(orw, input, schema, typeof(object));

            var value0 = reader.ReadArrayOfFloat64("field");
            Assert.That(value0, Is.Not.Null);
            var value1 = reader.ReadArrayOfNullableFloat64("field");
            Assert.That(value1, Is.Not.Null);

            Assert.That(value0.Length, Is.EqualTo(value1.Length));
            for (var i = 0; i < value0.Length; i++) Assert.That(value0[i], Is.EqualTo(value1[i]));
        }

        [Test]
        public void CanReadNullableArrayOfFloat64NotNullAsNonNullable()
        {
            var orw = Mock.Of<IReadWriteObjectsFromIObjectDataInputOutput>();

            var schema = SchemaBuilder.For("type").WithField("field", FieldKind.ArrayOfNullableFloat64).Build();
            var output = new ObjectDataOutput(1024, orw, Endianness.LittleEndian);
            var writer = new CompactWriter(orw, output, schema);
            writer.WriteArrayOfNullableFloat64("field", new double?[] { 1, 2, 3, 4 });
            writer.Complete();

            var buffer = output.ToByteArray();

            var input = new ObjectDataInput(buffer, orw, Endianness.LittleEndian);
            var reader = new CompactReader(orw, input, schema, typeof(object));

            var value0 = reader.ReadArrayOfFloat64("field");
            Assert.That(value0, Is.Not.Null);
            var value1 = reader.ReadArrayOfNullableFloat64("field");
            Assert.That(value1, Is.Not.Null);

            Assert.That(value0.Length, Is.EqualTo(value1.Length));
            for (var i = 0; i < value0.Length; i++) Assert.That(value0[i], Is.EqualTo(value1[i]));
        }

        [Test]
        public void CannotReadNullableArrayOfFloat64NullAsNonNullable()
        {
            var orw = Mock.Of<IReadWriteObjectsFromIObjectDataInputOutput>();

            var schema = SchemaBuilder.For("type").WithField("field", FieldKind.ArrayOfNullableFloat64).Build();
            var output = new ObjectDataOutput(1024, orw, Endianness.LittleEndian);
            var writer = new CompactWriter(orw, output, schema);
            writer.WriteArrayOfNullableFloat64("field", new double?[] { 1, 2, null, 4 });
            writer.Complete();

            var buffer = output.ToByteArray();

            var input = new ObjectDataInput(buffer, orw, Endianness.LittleEndian);
            var reader = new CompactReader(orw, input, schema, typeof(object));

            Assert.Throws<SerializationException>(() => reader.ReadArrayOfFloat64("field"));
            var value1 = reader.ReadArrayOfNullableFloat64("field");
        }

        [Test]
        public void CanReadNullArrays()
        {
            var orw = Mock.Of<IReadWriteObjectsFromIObjectDataInputOutput>();

            var schema = SchemaBuilder.For("type")
                .WithField("field0", FieldKind.ArrayOfBoolean)
                .WithField("field1", FieldKind.ArrayOfNullableBoolean)
                .WithField("field2", FieldKind.ArrayOfInt8)
                .WithField("field3", FieldKind.ArrayOfNullableInt8)
                .Build();
            var output = new ObjectDataOutput(1024, orw, Endianness.LittleEndian);
            var writer = new CompactWriter(orw, output, schema);
            writer.WriteArrayOfBoolean("field0", null);
            writer.WriteArrayOfNullableBoolean("field1", null);
            writer.WriteArrayOfInt8("field2", null);
            writer.WriteArrayOfNullableInt8("field3", null);
            writer.Complete();

            var buffer = output.ToByteArray();

            var input = new ObjectDataInput(buffer, orw, Endianness.LittleEndian);
            var reader = new CompactReader(orw, input, schema, typeof(object));

            Assert.That(reader.ReadArrayOfBoolean("field0"), Is.Null);
            Assert.That(reader.ReadArrayOfNullableBoolean("field0"), Is.Null);
            Assert.That(reader.ReadArrayOfBoolean("field1"), Is.Null);
            Assert.That(reader.ReadArrayOfNullableBoolean("field1"), Is.Null);

            Assert.That(reader.ReadArrayOfInt8("field2"), Is.Null);
            Assert.That(reader.ReadArrayOfNullableInt8("field2"), Is.Null);
            Assert.That(reader.ReadArrayOfInt8("field3"), Is.Null);
            Assert.That(reader.ReadArrayOfNullableInt8("field3"), Is.Null);
        }

        [Test]
        public void Exceptions()
        {
            var orw = Mock.Of<IReadWriteObjectsFromIObjectDataInputOutput>();

            var schema = SchemaBuilder.For("type").WithField("field", FieldKind.String).Build();
            var output = new ObjectDataOutput(1024, orw, Endianness.LittleEndian);
            var writer = new CompactWriter(orw, output, schema);
            writer.WriteString("field", "value");
            writer.Complete();

            var buffer = output.ToByteArray();

            var input = new ObjectDataInput(buffer, orw, Endianness.LittleEndian);
            var reader = new CompactReader(orw, input, schema, typeof(object));

            Assert.Throws<SerializationException>(() => reader.ReadBoolean("field"));
            Assert.Throws<SerializationException>(() => reader.ReadArrayOfBoolean("field"));
            Assert.Throws<SerializationException>(() => reader.ReadArrayOfNullableBoolean("field"));
            Assert.Throws<SerializationException>(() => reader.ReadInt8("field"));
            Assert.Throws<SerializationException>(() => reader.ReadArrayOfInt8("field"));
            Assert.Throws<SerializationException>(() => reader.ReadArrayOfNullableInt8("field"));

            Assert.Throws<SerializationException>(() => reader.ReadString("duh"));
            Assert.Throws<SerializationException>(() => reader.ReadInt8("duh"));
        }

        [Test]
        public void FieldNameAndKind()
        {
            var orw = Mock.Of<IReadWriteObjectsFromIObjectDataInputOutput>();

            var schema = SchemaBuilder.For("type").WithField("field", FieldKind.String).Build();
            var output = new ObjectDataOutput(1024, orw, Endianness.LittleEndian);
            var writer = new CompactWriter(orw, output, schema);

            Assert.That(writer.ValidateFieldNameInvariant("field", out var validated), Is.True);
            Assert.That(validated, Is.EqualTo("field"));

            Assert.That(writer.ValidateFieldNameInvariant("fIeLd", out validated), Is.True);
            Assert.That(validated, Is.EqualTo("field"));

            Assert.That(writer.ValidateFieldNameInvariant("duh", out validated), Is.False);
            Assert.That(validated, Is.Null);

            var reader = new CompactReader(orw, new ObjectDataInput(new byte[16], orw, Endianness.LittleEndian), schema, typeof(object));
            Assert.That(reader.GetFieldKind("field"), Is.EqualTo(FieldKind.String));
            Assert.That(reader.GetFieldKind("no-field"), Is.EqualTo(FieldKind.NotAvailable));
        }
    }
}
