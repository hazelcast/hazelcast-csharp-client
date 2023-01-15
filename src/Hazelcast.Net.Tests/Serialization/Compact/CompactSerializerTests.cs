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
using Hazelcast.Exceptions;
using Hazelcast.Messaging;
using Hazelcast.Serialization;
using Hazelcast.Serialization.Compact;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;

namespace Hazelcast.Tests.Serialization.Compact
{
    [TestFixture]
    public class CompactSerializerTests
    {
        [TestCase(Endianness.BigEndian, false)]
        [TestCase(Endianness.LittleEndian, false)]
        [TestCase(Endianness.BigEndian, true)]
        [TestCase(Endianness.LittleEndian, true)]
        public void SerializationServiceCanToDataThenToObject(Endianness endianness, bool withSchemas)
        {
            // define a schema for the Thing class - at that point, schemas must be defined
            // explicitly via code, we don't support any other mean of configuring schemas
            var thingSchema = SchemaBuilder
                .For("thing")
                .WithField("name", FieldKind.String)
                .WithField("value", FieldKind.Int32)
                .Build();

            Console.WriteLine("Schema:");
            Console.WriteLine(thingSchema.Id);
            Console.WriteLine();

            var options = new SerializationOptions
            {
                Endianness = endianness
            };

            // register a schema and a serializer for the Thing class - at that point, serializers
            // and schemas must be registered explicitly via code, we don't support anything else.
            options.Compact.AddSerializer(new ThingCompactSerializer());
            options.Compact.SetSchema<Thing>(thingSchema, false);

            // create the entire serialization service
            var messaging = Mock.Of<IClusterMessaging>();
            var service = HazelcastClientFactory.CreateSerializationService(options, messaging, new NullLoggerFactory());

            // and then, create a Thing instance, serialize it to data, and de-serialize it back to object

            var thing = new Thing
            {
                Name = "thingName",
                Value = 42
            };

            Console.WriteLine("ToData:");
            var data = service.ToData(thing, withSchemas);

            // dump bytes
            var bytes = data.ToByteArray();
            Console.WriteLine(bytes.Dump());
            Console.WriteLine();

            // validate a few basic things
            // note: type-id of root object is *always* big-endian, whereas schema-id depends on options
            var type = withSchemas ? SerializationConstants.ConstantTypeCompactWithSchema : SerializationConstants.ConstantTypeCompact;
            Assert.That(bytes.ReadInt(4, Endianness.BigEndian), Is.EqualTo(type));
            Assert.That(bytes.ReadLong(8, endianness), Is.EqualTo(thingSchema.Id));

            // schema id: -6514273721777083925
            //
            //   00 00 00 00                    :: <hash>
            //   ff ff ff c9                    :: -55
            //   a5 98 a6 90 6b d0 a9 eb        :: <schema-id>
            // <start>
            //   00 00 00 11                    :: 17 <data-length>
            // <data-start>
            //   00 00 00 2a                    :: 42
            //   00 00 00 09                    :: 'thingName'.Length
            //   74 68 69 6e 67 4e 61 6d 65     :: 'thingName'
            // <offsets>
            //   04                             :: 'thingName' offset

            Console.WriteLine("ToObject:");
            var thing2 = service.ToObject<Thing>(data);

            // validate that the object was properly de-serialized
            Assert.That(thing2.Name, Is.EqualTo(thing.Name));
            Assert.That(thing2.Value, Is.EqualTo(thing2.Value));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void SerializationServiceCanToDataThenToObjectNested(Endianness endianness)
        {
            // define schemas
            var thingSchema = SchemaBuilder
                .For("thing")
                .WithField("name", FieldKind.String)
                .WithField("value", FieldKind.Int32)
                .Build();

            var thingNestSchema = SchemaBuilder
                .For("thing-nest")
                .WithField("name", FieldKind.String)
                .WithField("thing", FieldKind.Compact)
                .Build();

            var options = new SerializationOptions
            {
                Endianness = endianness
            };

            options.Compact.AddSerializer(new ThingCompactSerializer());
            options.Compact.SetSchema<Thing>(thingSchema, false);
            options.Compact.AddSerializer(new ThingNestCompactSerializer());
            options.Compact.SetSchema<ThingNest>(thingNestSchema, false);

            var messaging = Mock.Of<IClusterMessaging>();
            var service = HazelcastClientFactory.CreateSerializationService(options, messaging, new NullLoggerFactory());

            var nest = new ThingNest
            {
                Name = "nestName",
                Thing = new Thing
                {
                    Name = "thingName",
                    Value = 42
                }
            };

            Console.WriteLine("ToData:");
            var data = service.ToData(nest);

            var bytes = data.ToByteArray();
            Console.WriteLine(bytes.Dump());
            Console.WriteLine();

            Assert.That(bytes.ReadInt(4, Endianness.BigEndian), Is.EqualTo(SerializationConstants.ConstantTypeCompact));
            Assert.That(bytes.ReadLong(8, endianness), Is.EqualTo(thingNestSchema.Id));

            Console.WriteLine("ToObject:");
            var nest2 = service.ToObject<ThingNest>(data);

            Assert.That(nest2.Name, Is.EqualTo(nest.Name));
            Assert.That(nest2.Thing, Is.Not.Null);
            Assert.That(nest2.Thing.Name, Is.EqualTo(nest.Thing.Name));
            Assert.That(nest2.Thing.Value, Is.EqualTo(nest.Thing.Value));
        }

        [Test]
        public void Adapter_Exceptions()
        {
            Assert.Throws<ArgumentNullException>(() => CompactSerializerAdapter.Create(null));
            Assert.Throws<ArgumentException>(() => CompactSerializerAdapter.Create(Mock.Of<ICompactSerializer>()));
        }

        [Test]
        public void SerializerDefaultTypeName()
        {
            var serializer = new DefaultNameThingSerializer();
            Assert.That(serializer.TypeName, Is.EqualTo(CompactOptions.GetDefaultTypeName<Thing>()));
        }

        [Test]
        public void Extensions_Exceptions()
        {
            Assert.Throws<ArgumentNullException>(() => ((ICompactSerializer)null).GetSerializedType());
            Assert.Throws<ArgumentException>(() => Mock.Of<ICompactSerializer>().GetSerializedType());

            Assert.Throws<ArgumentNullException>(() => ((ICompactSerializer)null).IsICompactSerializerOfTSerialized(out _));
            Assert.Throws<ArgumentNullException>(() => ((Type)null).IsICompactSerializerOfTSerialized(out _));
        }

        private class DefaultNameThingSerializer : CompactSerializerBase<Thing>
        {
            public override Thing Read(ICompactReader reader) => throw new NotSupportedException();

            public override void Write(ICompactWriter writer, Thing value) => throw new NotSupportedException();
        }

        private class Thing
        {
            public string Name { get; set; }

            public int Value { get; set; }
        }

        private class ThingCompactSerializer : CompactSerializerBase<Thing>
        {
            public override string TypeName => "thing";

            public override Thing Read(ICompactReader reader)
            {
                return new Thing
                {
                    Name = reader.ReadString("name"),
                    Value = reader.ReadInt32("value")
                };
            }

            public override void Write(ICompactWriter writer, Thing obj)
            {
                writer.WriteString("name", obj.Name);
                writer.WriteInt32("value", obj.Value);
            }
        }

        private class ThingNest
        {
            public string Name { get; set; }

            public Thing Thing { get; set; }
        }

        private class ThingNestCompactSerializer : CompactSerializerBase<ThingNest>
        {
            public override string TypeName => "thing-nest";

            public override ThingNest Read(ICompactReader reader)
            {
                return new ThingNest
                {
                    Name = reader.ReadString("name"),
                    Thing = reader.ReadCompact<Thing>("thing")
                };
            }

            public override void Write(ICompactWriter writer, ThingNest obj)
            {
                writer.WriteString("name", obj.Name);
                writer.WriteCompact("thing", obj.Thing);
            }
        }
    }
}
