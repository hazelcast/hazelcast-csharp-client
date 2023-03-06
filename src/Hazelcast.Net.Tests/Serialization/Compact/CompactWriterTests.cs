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

using System;
using Hazelcast.Core;
using Hazelcast.Models;
using Hazelcast.Serialization;
using Hazelcast.Serialization.Compact;
using Moq;
using NUnit.Framework;

namespace Hazelcast.Tests.Serialization.Compact
{
    [TestFixture]
    public class CompactWriterTests
    {
        [Test]
        public void CannotWriteOnceCompleted()
        {
            var orw = Mock.Of<IReadWriteObjectsFromIObjectDataInputOutput>();
            var output = new ObjectDataOutput(0, orw, Endianness.BigEndian);
            var schema = new Schema();
            var writer = new CompactWriter(orw, output, schema);

            writer.Complete();
            Assert.Throws<InvalidOperationException>(() => writer.WriteInt8("name", 0));
        }

        [Test]
        public void OkToCompleteMultipleTimes()
        {
            var orw = Mock.Of<IReadWriteObjectsFromIObjectDataInputOutput>();
            var output = new ObjectDataOutput(0, orw, Endianness.BigEndian);
            var schema = new Schema();
            var writer = new CompactWriter(orw, output, schema);

            writer.Complete();
            writer.Complete();
            writer.Complete();
            Assert.Throws<InvalidOperationException>(() => writer.WriteInt8("name", 0));
        }

        [Test]
        public void TimeStampWithTimeZonePrecision()
        {
            var orw = Mock.Of<IReadWriteObjectsFromIObjectDataInputOutput>();
            var output = new ObjectDataOutput(0, orw, Endianness.BigEndian);
            var schema = SchemaBuilder.For("type").WithField("name", FieldKind.TimeStampWithTimeZone).Build();
            var writer = new CompactWriter(orw, output, schema);

            // sub-second precision for offset throws
            Assert.Throws<SerializationException>(() =>
                writer.WriteTimeStampWithTimeZone("name", new HOffsetDateTime(DateTime.Now, TimeSpan.FromMilliseconds(500))));
        }

        [Test]
        public void Exceptions()
        {
            var orw = Mock.Of<IReadWriteObjectsFromIObjectDataInputOutput>();
            var output = new ObjectDataOutput(0, orw, Endianness.BigEndian);
            var schema = SchemaBuilder.For("type").WithField("name", FieldKind.String).Build();
            var writer = new CompactWriter(orw, output, schema);

            Assert.Throws<SerializationException>(() => writer.WriteInt8("name", 0));
        }

        [Test]
        public void CanWriteArrayOfCompactOfSameType()
        {
            var orw = Mock.Of<IReadWriteObjectsFromIObjectDataInputOutput>();
            var output = new ObjectDataOutput(0, orw, Endianness.BigEndian);
            var schema = SchemaBuilder.For("array").WithField("array", FieldKind.ArrayOfCompact).Build();
            var writer = new CompactWriter(orw, output, schema);

            // only one type and the right one
            var value = new Thing[] { new(), new() };
            writer.WriteArrayOfCompact("array", value);
        }

        [Test]
        public void CannotWriteArrayOfCompactOfDifferentTypes()
        {
            var orw = Mock.Of<IReadWriteObjectsFromIObjectDataInputOutput>();
            var output = new ObjectDataOutput(0, orw, Endianness.BigEndian);
            var schema = SchemaBuilder.For("array").WithField("array", FieldKind.ArrayOfCompact).Build();
            var writer = new CompactWriter(orw, output, schema);

            // two different types
            var value = new Thing[] { new(), new ThingExtend() };
            Assert.Throws<SerializationException>(() => writer.WriteArrayOfCompact("array", value));
        }

        [Test]
        public void CannotWriteArrayOfCompactOfDerivedTypes()
        {
            var orw = Mock.Of<IReadWriteObjectsFromIObjectDataInputOutput>();
            var output = new ObjectDataOutput(0, orw, Endianness.BigEndian);
            var schema = SchemaBuilder.For("array").WithField("array", FieldKind.ArrayOfCompact).Build();
            var writer = new CompactWriter(orw, output, schema);

            // only one type but not the right one
            var value = new Thing[] { new ThingExtend() };
            Assert.Throws<SerializationException>(() => writer.WriteArrayOfCompact("array", value));
        }
    }
}
