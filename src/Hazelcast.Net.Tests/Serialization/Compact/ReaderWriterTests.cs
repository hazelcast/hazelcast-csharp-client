// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Core;
using Hazelcast.Serialization;
using Hazelcast.Serialization.Compact;
using Moq;
using NUnit.Framework;

namespace Hazelcast.Tests.Serialization.Compact
{
    [TestFixture]
    public class ReaderWriterTests
    {
        private static byte[] Write(Schema schema, Endianness endianness, Action<ICompactWriter> write)
        {
            // should not be invoked, a dummy mock is all we need
            var objectsReaderWriter = Mock.Of<IReadWriteObjectsFromIObjectDataInputOutput>();

            const int initialBufferSize = 1024;

            var output = new ObjectDataOutput(initialBufferSize, objectsReaderWriter, endianness);
            var writer = new CompactWriter(objectsReaderWriter, output, schema);
            write(writer);
            return output.ToByteArray();
        }

        private static void Read(Schema schema, Endianness endianness, byte[] bytes, Action<ICompactReader> read)
        {
            // should not be invoked, a dummy mock is all we need
            var objectsReaderWriter = Mock.Of<IReadWriteObjectsFromIObjectDataInputOutput>();
            
            var input = new ObjectDataInput(bytes, objectsReaderWriter, endianness);
            var reader = new CompactReader(objectsReaderWriter, input, schema, typeof(object));
            read(reader);
        }
        
        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void Int32(Endianness endianness)
        {
            var schema = SchemaBuilder.For("whatever").WithField("value", FieldKind.Int32).Build();

            var value0 = 1 + RandomProvider.Next();
            var value1 = 0;
            var bytes = Write(schema, endianness, w => w.WriteInt32("value", value0));
            Read(schema, endianness, bytes, r => { value1 = r.ReadInt32("value"); });
            Assert.That(value1, Is.EqualTo(value0));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void Int64(Endianness endianness)
        {
            var schema = SchemaBuilder.For("whatever").WithField("value", FieldKind.Int64).Build();

            var value0 = 1 + ((long) RandomProvider.Next() << 32) + RandomProvider.Next();
            var value1 = 0L;
            var bytes = Write(schema, endianness, w => w.WriteInt64("value", value0));
            Read(schema, endianness, bytes, r => { value1 = r.ReadInt64("value"); });
            Assert.That(value1, Is.EqualTo(value0));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void Boolean(Endianness endianness)
        {
            var schema = SchemaBuilder.For("whatever").WithField("value", FieldKind.Boolean).Build();

            var v = false;
            var bytes = Write(schema, endianness, w => w.WriteBoolean("value", true));
            Read(schema, endianness, bytes, r => { v = r.ReadBoolean("value"); });
            Assert.That(v);
            bytes = Write(schema, endianness, w => w.WriteBoolean("value", false));
            Read(schema, endianness, bytes, r => { v = r.ReadBoolean("value"); });
            Assert.That(!v);
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void Booleans(Endianness endianness)
        {
            var schema = SchemaBuilder.For("whatever")
                .WithField("valueA", FieldKind.Boolean)
                .WithField("valueB", FieldKind.Boolean)
                .Build();

            var vA = false;
            var vB = false;
            var bytes = Write(schema, endianness, w =>
            {
                w.WriteBoolean("valueA", true);
                w.WriteBoolean("valueB", false);
            });
            Read(schema, endianness, bytes, r =>
            {
                vA = r.ReadBoolean("valueA");
                vB = r.ReadBoolean("valueB");
            });
            Assert.That(vA);
            Assert.That(!vB);
        }
    }
}
