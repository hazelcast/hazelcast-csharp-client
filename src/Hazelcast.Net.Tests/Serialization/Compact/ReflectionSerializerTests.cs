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
using System.Linq;
using System.Reflection;
using Hazelcast.Core;
using Hazelcast.Serialization;
using Hazelcast.Serialization.Compact;
using Moq;
using NUnit.Framework;

namespace Hazelcast.Tests.Serialization.Compact
{
    [TestFixture]
    public class ReflectionSerializerTests
    {
        // this test ensures that all FieldKind values are tested in GenerateSchema
        //
        [Test]
        public void ValidateGenerateSchemaSource()
        {
            // get all enum values
            var kindValues = new HashSet<FieldKind>(Enum.GetValues(typeof (FieldKind)).Cast<FieldKind>());

            // remove those that don't make sense here or are not supported by compact serialization
            kindValues.Remove(FieldKind.NotAvailable);

            // remove those that are tested
            foreach (var (_, kind) in GenerateSchemaSource) kindValues.Remove(kind);

            // nothing should remain
            Assert.That(kindValues.Count, Is.Zero, $"Untested values: {string.Join(", ", kindValues)}");
        }

        private static (Type, FieldKind)[] GenerateSchemaSource => ReflectionDataSource.TypeKindMap;

        // this test ensures that ReflectionSerializer + SchemaBuilderWriter generate the correct schema
        //
        [TestCaseSource(nameof(GenerateSchemaSource))]
        public void GenerateSchema((Type PropertyType, FieldKind ExpectedFieldKind) testCase)
        {
            var type = ReflectionHelper.CreateObjectType(testCase.PropertyType);
            var obj = Activator.CreateInstance(type);
            if (obj == null) throw new Exception("panic: null object.");
            var serializer = new ReflectionSerializer();
            var sw = new SchemaBuilderWriter("thing");
            serializer.Write(sw, obj);
            var schema = sw.Build();

            Assert.That(schema.TypeName, Is.EqualTo("thing"));
            Assert.That(schema.Fields.Count, Is.EqualTo(1));
            Assert.That(schema.Fields[0].FieldName, Is.EqualTo("Value0"));
            Assert.That(schema.Fields[0].Kind, Is.EqualTo(testCase.ExpectedFieldKind));
        }

        // this test ensures that SchemaBuilderWriter throws the right exceptions.
        [Test]
        public void SchemaBuilderWriterExceptions()
        {
            var sw = new SchemaBuilderWriter("thing");
            sw.WriteBoolean("foo", false);
            Assert.Throws<SerializationException>(() => sw.WriteBoolean("foo", false));
            Assert.Throws<SerializationException>(() => sw.WriteInt8("foo", 0));
            sw.WriteBoolean("FOO", false); // is case-sensitive
        }

        private static (Type, object?)[] SerializeSource => ReflectionDataSource.TypeValueList;

        // this ensures that ReflectionSerializer + SchemaBuilderWriter can write then read primitive types
        //
        [TestCaseSource(nameof(SerializeSource))]
        public void SerializeOne((Type PropertyType, object? PropertyValue) testCase)
        {
            Console.Write("Value: ");
            if (testCase.PropertyValue is Array arrayValue)
            {
                Console.Write("[");
                var first = true;
                foreach (var element in arrayValue)
                {
                    if (first) first = false; else Console.Write(",");
                    Console.Write(element?.ToString() ?? "<null>");
                }
                Console.Write("]");
            }
            else
            {
                Console.Write(testCase.PropertyValue?.ToString() ?? "<null>");
            }
            Console.WriteLine();

            var type = ReflectionHelper.CreateObjectType(testCase.PropertyType);
            var obj = Activator.CreateInstance(type);
            if (obj == null) throw new Exception("panic: null obj");
            ReflectionHelper.SetPropertyValue(obj, "Value0", testCase.PropertyValue);

            var serializer = new ReflectionSerializer();
            var sw = new SchemaBuilderWriter("thing");
            serializer.Write(sw, obj);
            var schema = sw.Build();

            var orw = new ObjectReaderWriter(serializer);
            
            var output = new ObjectDataOutput(1024, orw, Endianness.LittleEndian);
            var writer = new CompactWriter(orw, output, schema);
            serializer.Write(writer, obj);
            writer.Complete();

            var buffer = output.ToByteArray();
            Console.WriteLine(buffer.Dump());

            var input = new ObjectDataInput(buffer, orw, Endianness.LittleEndian);
            var reader = new CompactReader(orw, input, schema, obj.GetType());

            var obj2 = serializer.Read(reader);
            Assert.That(obj2, Is.Not.Null);
            Assert.That(obj2.GetType(), Is.EqualTo(type));

            AssertPropertyValue(testCase.PropertyValue, ReflectionHelper.GetPropertyValue(obj2, "Value0"));
        }

        private void AssertPropertyValue(object? value, object? value2)
        {
            switch (value)
            {
                case null:
                    Assert.That(value2, Is.Null);
                    break;

                case ReflectionDataSource.SomeClass o:
                    {
                        Assert.That(value2, Is.Not.Null);
                        Assert.That(value2, Is.InstanceOf<ReflectionDataSource.SomeClass>());
                        var o2 = (ReflectionDataSource.SomeClass)value2!;
                        Assert.That(o.Value, Is.EqualTo(o2.Value));
                        break;
                    }

                case ReflectionDataSource.SomeClass2 o:
                    {
                        Assert.That(value2, Is.Not.Null);
                        Assert.That(value2, Is.InstanceOf<ReflectionDataSource.SomeClass2>());
                        var o2 = (ReflectionDataSource.SomeClass2)value2!;
                        if (o.Value == null)
                        {
                            Assert.That(o2.Value, Is.Null);
                        }
                        else
                        {
                            Assert.That(o2.Value, Is.Not.Null);
                            Assert.That(o2.Value!.Value, Is.EqualTo(o.Value.Value));
                        }
                        break;
                    }

                case ReflectionDataSource.SomeStruct o:
                    {
                        Assert.That(value2, Is.Not.Null);
                        Assert.That(value2, Is.InstanceOf<ReflectionDataSource.SomeStruct>());
                        var o2 = (ReflectionDataSource.SomeStruct)value2!;
                        Assert.That(o.Value, Is.EqualTo(o2.Value));
                        break;
                    }

                case Array array when value2 is Array array2:
                    {
                        Assert.That(array2.Rank, Is.EqualTo(array.Rank));
                        Assert.That(array2.Length, Is.EqualTo(array.Length));
                        for (var i = 0; i < array.Length; i++)
                            AssertPropertyValue(array.GetValue(i), array2.GetValue(i));
                        break;
                    }

                case Array:
                    Assert.That(value2 is Array); // fail
                    break;

                default:
                    Assert.That(value2, Is.EqualTo(value));
                    break;
            }
        }

        [Test]
        public void SerializeBooleans()
        {
            var propertyTypes = new Type[32];
            for (var i = 0; i < propertyTypes.Length; i++) propertyTypes[i] = typeof (bool);
            var type = ReflectionHelper.CreateObjectType(propertyTypes);
            var obj = Activator.CreateInstance(type);
            if (obj == null) throw new Exception("panic: null obj");
            var properties = new PropertyInfo[propertyTypes.Length];
            for (var i = 0; i < propertyTypes.Length; i++)
            {
                var property = type.GetProperty($"Value{i}");
                Assert.That(property, Is.Not.Null);
                properties[i] = property!;
                try
                {
                    property!.SetValue(obj, i % 3 == 0);
                }
                catch
                {
                    Assert.Fail($"Failed to assign value to property.");
                }
            }

            var serializer = new ReflectionSerializer();
            var sw = new SchemaBuilderWriter("thing");
            serializer.Write(sw, obj);
            var schema = sw.Build();

            var orw = new ObjectReaderWriter(serializer);

            var output = new ObjectDataOutput(1024, orw, Endianness.LittleEndian);
            var writer = new CompactWriter(orw, output, schema);
            serializer.Write(writer, obj);
            writer.Complete();

            var buffer = output.ToByteArray();
            Console.WriteLine(buffer.Dump());

            var input = new ObjectDataInput(buffer, orw, Endianness.LittleEndian);
            var reader = new CompactReader(orw, input, schema, obj.GetType());

            var obj2 = serializer.Read(reader);
            Assert.That(obj2, Is.Not.Null);
            Assert.That(obj2.GetType(), Is.EqualTo(type));

            for (var i = 0; i < propertyTypes.Length; i++)
            {
                Assert.That(properties[i].GetValue(obj2), Is.EqualTo(properties[i].GetValue(obj)));
            }
        }

        [Test]
        public void SerializeMany()
        {
            const int repeat = 100;
            const int count = 10; // 10 properties

            var random = new Random();

            for (var i = 0; i < repeat; i++)
                SerializeMultiple(count, random, SerializeSource);

            // push to short offsets
            SerializeMultiple(byte.MaxValue * 2, random, SerializeSource);
            // actually *not* possible - see SerializeALot
            //SerializeMultiple(short.MaxValue * 2, random, TypeValueList);
        }

        [Test]
        public void SerializeALot()
        {
            const int repeat = 100;
            var random = new Random();

            // this is going to trigger the offset size thing
            // make sure to only use reference types
            var serializeSource = SerializeSource.Where(x => x.Item1.IsNullable()).ToArray();

            for (var i = 0; i < repeat; i++)
            {
                // byte offsets
                SerializeMultiple(100, random, serializeSource);
                SerializeMultiple(byte.MaxValue - 1, random, serializeSource);
                // push to short offsets
                SerializeMultiple(byte.MaxValue, random, serializeSource);
                SerializeMultiple(byte.MaxValue * 2, random, serializeSource);
            }

            // we cannot push to int offsets this way, as .NET will not accept
            // creating a type with enough properties, so we'll have to do it
            // differently (with large data volumes)
        }

        [Test]
        public void SerializeLargeVolume()
        {
            // each string is 4 bytes (length) + 1 byte per char (we're using [A-Z0-9] chars)
            // note that string.Length is an Int32 and a string cannot get bigger than that

            var random = new Random();

            // byte offsets (data length < byte.MaxValue)
            SerializeVolume(50, random);
            SerializeVolume(byte.MaxValue - 1, random);

            // short offsets (data length < ushort.MaxValue)
            SerializeVolume(byte.MaxValue, random);
            SerializeVolume(byte.MaxValue + 1, random);
            SerializeVolume(byte.MaxValue * 2, random);
            SerializeVolume(ushort.MaxValue - 1, random);

            // int offsets
            SerializeVolume(ushort.MaxValue, random);
            SerializeVolume(ushort.MaxValue + 1, random);
            SerializeVolume(ushort.MaxValue * 2, random);
        }

        private void SerializeVolume(long datalength, Random random)
        {
            const int count = 10;
            var sizeOther = (datalength - count * 4) / (count - 1);
            var sizeFirst = datalength - count * 4 - (count - 1) * sizeOther;

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            var propertyTypes = new Type[count];
            for (var i = 0; i < count; i++) propertyTypes[i] = typeof(string);

            var type = ReflectionHelper.CreateObjectType(propertyTypes);
            var obj = Activator.CreateInstance(type);
            if (obj == null) throw new Exception("panic: null obj");
            var properties = new PropertyInfo[count];
            for (var i = 0; i < count; i++)
            {
                var property = type.GetProperty($"Value{i}");
                Assert.That(property, Is.Not.Null);
                properties[i] = property!;
                try
                {
                    var size = i == 0 ? sizeFirst : sizeOther;
                    var text = new char[size];
                    for (var j = 0; j < size; j++) text[j] = chars[random.Next(chars.Length)];
                    property!.SetValue(obj, new string(text));
                }
                catch
                {
                    Assert.Fail($"Failed to assign value to property.");
                }
            }

            var serializer = new ReflectionSerializer();
            var sw = new SchemaBuilderWriter("thing");
            serializer.Write(sw, obj);
            var schema = sw.Build();

            var orw = new ObjectReaderWriter(serializer);

            var output = new ObjectDataOutput(1024, orw, Endianness.LittleEndian);
            var writer = new CompactWriter(orw, output, schema);
            serializer.Write(writer, obj);
            writer.Complete();

            var buffer = output.ToByteArray();

            var input = new ObjectDataInput(buffer, orw, Endianness.LittleEndian);
            var reader = new CompactReader(orw, input, schema, obj.GetType());

            var obj2 = serializer.Read(reader);
            Assert.That(obj2, Is.Not.Null);
            Assert.That(obj2.GetType(), Is.EqualTo(type));

            for (var i = 0; i < count; i++)
                Assert.That(properties[i].GetValue(obj2), Is.EqualTo(properties[i].GetValue(obj)));
        }

        private void SerializeMultiple(int count, Random random, (Type, object?)[] serializeSource)
        {
            var sources = new (Type, object?)[count];
            for (var i = 0; i < count; i++)
                sources[i] = serializeSource[random.Next(serializeSource.Length)];
            var propertyTypes = sources.Select(x => x.Item1).ToArray();
            var type = ReflectionHelper.CreateObjectType(propertyTypes);
            var obj = Activator.CreateInstance(type);
            if (obj == null) throw new Exception("panic: null obj");
            var properties = new PropertyInfo[count];
            for (var i = 0; i < count; i++)
            {
                var property = type.GetProperty($"Value{i}");
                Assert.That(property, Is.Not.Null);
                properties[i] = property!;
                try
                {
                    property!.SetValue(obj, sources[i].Item2);
                }
                catch
                {
                    Assert.Fail($"Failed to assign value to property.");
                }
            }

            var serializer = new ReflectionSerializer();
            var sw = new SchemaBuilderWriter("thing");
            serializer.Write(sw, obj);
            var schema = sw.Build();

            var orw = new ObjectReaderWriter(serializer);

            var output = new ObjectDataOutput(1024, orw, Endianness.LittleEndian);
            var writer = new CompactWriter(orw, output, schema);
            serializer.Write(writer, obj);
            writer.Complete();

            var buffer = output.ToByteArray();

            var input = new ObjectDataInput(buffer, orw, Endianness.LittleEndian);
            var reader = new CompactReader(orw, input, schema, obj.GetType());

            var obj2 = serializer.Read(reader);
            Assert.That(obj2, Is.Not.Null);
            Assert.That(obj2.GetType(), Is.EqualTo(type));

            for (var i = 0; i < count; i++)
            {
                AssertPropertyValue(properties[i].GetValue(obj2), properties[i].GetValue(obj));
            }
        }

        // this test ensures that ReflectionSerializer throws the right exceptions.
        [Test]
        public void ReflectionSerializerExceptions()
        {
            var serializer = new ReflectionSerializer();

            Assert.Throws<NotSupportedException>(() =>
            {
                _ = serializer.TypeName;
            });

            Assert.Throws<ArgumentNullException>(() => serializer.Write(null!, null!));

            var notCompactReader = Mock.Of<ICompactReader>();
            Assert.Throws<ArgumentException>(() => serializer.Read(notCompactReader));

            var orw = Mock.Of<IReadWriteObjectsFromIObjectDataInputOutput>();
            var compactReader = new CompactReader(
                orw,
                new ObjectDataInput(Array.Empty<byte>(), orw, Endianness.BigEndian),
                SchemaBuilder.For("thing").Build(),
                typeof(ActivatorKiller)); // Activator.CreateInstance throws on that type (no public ctor)

            Assert.Throws<SerializationException>(() => serializer.Read(compactReader));

            compactReader = new CompactReader(
                orw,
                new ObjectDataInput(Array.Empty<byte>(), orw, Endianness.BigEndian),
                SchemaBuilder.For("thing").Build(),
                typeof(int?)); // Activator.CreateInstance returns null on that type

            Assert.Throws<SerializationException>(() => serializer.Read(compactReader));

            var bytes = new byte[BytesExtensions.SizeOfInt];
            for (var i = 0; i < bytes.Length; i++) bytes[i] = 0;
            compactReader = new CompactReader(
                orw,
                new ObjectDataInput(bytes, orw, Endianness.BigEndian),
                SchemaBuilder.For("thing").WithField("values", FieldKind.Compact).Build(),
                typeof(ClassWithInterfaceProperty)); // interfaces are not supported

            Assert.Throws<SerializationException>(() => serializer.Read(compactReader));

            compactReader = new CompactReader(
                orw,
                new ObjectDataInput(bytes, orw, Endianness.BigEndian),
                SchemaBuilder.For("thing").WithField("values", FieldKind.ArrayOfCompact).Build(),
                typeof(ClassWithInterfaceArrayProperty)); // arrays of interfaces are not supported

            Assert.Throws<SerializationException>(() => serializer.Read(compactReader));

            Assert.That(ConvertEx.UnboxNonNull<int>(33), Is.EqualTo(33));
            Assert.Throws<InvalidOperationException>(() =>
            {
                // defensive coding, that should never happen in our code
                _ = ConvertEx.UnboxNonNull<int>(null);
            });

            Assert.That(ConvertEx.ValueNonNull<int>(33), Is.EqualTo(33));
            Assert.Throws<InvalidOperationException>(() =>
            {
                // defensive coding, that should never happen in our code
                _ = ConvertEx.ValueNonNull<int>(null);
            });
        }

        [Test]
        public void SerializeNestedClass()
        {
            var obj = new ReflectionDataSource.SomeExtend { Value = 33, Other = 44 };

            Assert.That(obj.GetType(), Is.EqualTo(typeof(ReflectionDataSource.SomeExtend)));
            var properties = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            Assert.That(properties.Count, Is.EqualTo(2));
            foreach (var property in properties) Console.WriteLine($"PROPERTY: {property.DeclaringType}.{property.Name}");

            var serializer = new ReflectionSerializer();
            var sw = new SchemaBuilderWriter("thing");
            serializer.Write(sw, obj);
            var schema = sw.Build();

            var orw = new ObjectReaderWriter(serializer);

            var output = new ObjectDataOutput(1024, orw, Endianness.LittleEndian);
            var writer = new CompactWriter(orw, output, schema);
            serializer.Write(writer, obj);
            writer.Complete();

            var buffer = output.ToByteArray();

            var input = new ObjectDataInput(buffer, orw, Endianness.LittleEndian);
            var reader = new CompactReader(orw, input, schema, obj.GetType());

            var obj2 = serializer.Read(reader);
            Assert.That(obj2, Is.Not.Null);
            Assert.That(obj2.GetType(), Is.EqualTo(typeof(ReflectionDataSource.SomeExtend)));
            var obj2e = (ReflectionDataSource.SomeExtend)obj2;
            Assert.That(obj2e.Value, Is.EqualTo(obj.Value));
            Assert.That(obj2e.Other, Is.EqualTo(obj.Other));

        }

        [Test]
        public void CannotSerializeNotSupportedTypes()
        {
            var obj = new List<int>();
            Console.WriteLine(obj.GetType().FullName);

            var serializer = new ReflectionSerializer();
            var sw = new SchemaBuilderWriter("thing");
            var e = Assert.Throws<SerializationException>(() => serializer.Write(sw, obj))!;
            Console.WriteLine(e.Message);
        }

        [Test]
        public void CannotSerializeAnonymousTypes()
        {
            var obj = new { a = 2 };
            Console.WriteLine(obj.GetType().FullName);

            var serializer = new ReflectionSerializer();
            var sw = new SchemaBuilderWriter("thing");
            var e = Assert.Throws<SerializationException>(() => serializer.Write(sw, obj))!;
            Console.WriteLine(e.Message);
        }

        private class ActivatorKiller
        {
            private ActivatorKiller()
            { }
        }

        private class ClassWithInterfaceProperty
        {
            public IList<int> Values { get; set; } = new List<int>();
        }

        private class ClassWithInterfaceArrayProperty
        {
            public IList<int>[] Values { get; set; } = Array.Empty<IList<int>>();
        }

        internal class ObjectReaderWriter : IReadObjectsFromObjectDataInput, IWriteObjectsToObjectDataOutput
        {
            private readonly ReflectionSerializer _serializer;
            private readonly Schema _someClassSchema;
            private readonly Schema _someClass2Schema;
            private readonly Schema _someStructSchema;
            private readonly Schema _someStruct2Schema;
            private readonly Schema _someStruct2NSchema;

            public ObjectReaderWriter(ReflectionSerializer serializer)
            {
                Schema BuildSchema(string name, object o)
                {
                    var nsw = new SchemaBuilderWriter(name);
                    serializer.Write(nsw, o);
                    return nsw.Build();
                }

                _serializer = serializer;
                _someClassSchema = BuildSchema("some-class", new ReflectionDataSource.SomeClass());
                _someClass2Schema = BuildSchema("some-class-2", new ReflectionDataSource.SomeClass2());
                _someStructSchema = BuildSchema("some-struct", new ReflectionDataSource.SomeStruct());
                _someStruct2Schema = BuildSchema("some-struct-2", new ReflectionDataSource.SomeStruct2());
                _someStruct2NSchema = BuildSchema("some-struct-2N", new ReflectionDataSource.SomeStruct2N());
            }

            public void Write(IObjectDataOutput output, object obj)
            {
                var schema = obj switch
                {
                    ReflectionDataSource.SomeClass => _someClassSchema,
                    ReflectionDataSource.SomeClass2 => _someClass2Schema,
                    ReflectionDataSource.SomeStruct => _someStructSchema,
                    ReflectionDataSource.SomeStruct2 => _someStruct2Schema,
                    ReflectionDataSource.SomeStruct2N => _someStruct2NSchema,
                    _ => throw new NotSupportedException($"Don't know how to write {obj.GetType()}.")
                };
                var w = new CompactWriter(this, (ObjectDataOutput) output, schema);
                _serializer.Write(w, obj);
                w.Complete();
            }

            public object Read(IObjectDataInput input, Type type)
            {
                var schema =
                    type == typeof(ReflectionDataSource.SomeClass) ? _someClassSchema :
                    type == typeof(ReflectionDataSource.SomeClass2) ? _someClass2Schema :
                    type == typeof(ReflectionDataSource.SomeStruct) ? _someStructSchema :
                    type == typeof(ReflectionDataSource.SomeStruct2) ? _someStruct2Schema :
                    type == typeof(ReflectionDataSource.SomeStruct2N) ? _someStruct2NSchema :
                    throw new NotSupportedException($"Don't know how to read {type}.");
                var r = new CompactReader(this, (ObjectDataInput)input, schema, type);
                return _serializer.Read(r);
            }

            public T Read<T>(IObjectDataInput input) => (T) Read(input, typeof (T));
        }
    }
}
