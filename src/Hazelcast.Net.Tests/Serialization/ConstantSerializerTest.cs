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
using System.Collections.Generic;
using System.Numerics;
using Hazelcast.Core;
using Hazelcast.Serialization;
using Hazelcast.Serialization.ConstantSerializers;
using Hazelcast.Testing;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Hazelcast.Tests.Serialization
{
    [TestFixture]
    public class ConstantSerializerTest
    {
        private static readonly Endianness[] Endiannesses =
        {
            Endianness.BigEndian,
            Endianness.LittleEndian
        };

        private SerializationService CreateSerializationService(Endianness endianness, bool enableClrSerialization)
        {
            var options = new SerializationOptions { EnableClrSerialization = enableClrSerialization };

            return new SerializationServiceBuilder(options, new NullLoggerFactory())
                .SetEndianness(endianness)
                .SetPortableVersion(1)
                .AddDefinitions(new ConstantSerializerDefinitions())
                .Build();
        }

        private void AssertSerialization<T>(T obj, Endianness order, bool enableClrSerialization = false)
        {
            var ss = CreateSerializationService(order, enableClrSerialization);
            var data = ss.ToData(obj);
            var deserialized = ss.ToObject<T>(data);
            Assert.AreEqual(obj, deserialized);

            // test first time deserialization
            var ss2 = CreateSerializationService(order, enableClrSerialization);
            Assert.AreEqual(obj, ss2.ToObject<T>(data));
        }

        [Test, TestCaseSource(nameof(Endiannesses))]
        public void TestBoolean(Endianness order)
        {
            AssertSerialization(TestUtils.RandomBool(), order);
        }

        [Test, TestCaseSource(nameof(Endiannesses))]
        public void TestBooleanArray(Endianness order)
        {
            AssertSerialization(TestUtils.RandomArray(TestUtils.RandomBool), order);
        }

        [Test, TestCaseSource(nameof(Endiannesses))]
        public void TestByte(Endianness order)
        {
            AssertSerialization(TestUtils.RandomByte(), order);
        }

        [Test, TestCaseSource(nameof(Endiannesses))]
        public void TestByteArray(Endianness order)
        {
            AssertSerialization(TestUtils.RandomArray(TestUtils.RandomByte), order);
        }

        [Test, TestCaseSource(nameof(Endiannesses))]
        public void TestChar(Endianness order)
        {
            AssertSerialization(TestUtils.RandomChar(), order);
        }

        [Test, TestCaseSource(nameof(Endiannesses))]
        public void TestCharArray(Endianness order)
        {
            AssertSerialization(TestUtils.RandomArray(TestUtils.RandomChar), order);
        }

        [Test, TestCaseSource(nameof(Endiannesses))]
        public void TestDouble(Endianness order)
        {
            AssertSerialization(TestUtils.RandomDouble(), order);
        }

        [Test, TestCaseSource(nameof(Endiannesses))]
        public void TestDoubleArray(Endianness order)
        {
            AssertSerialization(TestUtils.RandomArray(TestUtils.RandomDouble), order);
        }

        [Test, TestCaseSource(nameof(Endiannesses))]
        public void TestFloat(Endianness order)
        {
            AssertSerialization(TestUtils.RandomFloat(), order);
        }

        [Test, TestCaseSource(nameof(Endiannesses))]
        public void TestFloatArray(Endianness order)
        {
            AssertSerialization(TestUtils.RandomArray(TestUtils.RandomFloat), order);
        }

        [Test, TestCaseSource(nameof(Endiannesses))]
        public void TestInteger(Endianness order)
        {
            AssertSerialization(TestUtils.RandomInt(), order);
        }

        [Test, TestCaseSource(nameof(Endiannesses))]
        public void TestIntegerArray(Endianness order)
        {
            AssertSerialization(TestUtils.RandomArray(TestUtils.RandomInt), order);
        }

        [Test, TestCaseSource(nameof(Endiannesses))]
        public void TestLong(Endianness order)
        {
            AssertSerialization(TestUtils.RandomLong(), order);
        }

        [Test, TestCaseSource(nameof(Endiannesses))]
        public void TestLongArray(Endianness order)
        {
            AssertSerialization(TestUtils.RandomArray(TestUtils.RandomLong), order);
        }

        [Test, TestCaseSource(nameof(Endiannesses))]
        public void TestShort(Endianness order)
        {
            AssertSerialization(TestUtils.RandomShort(), order);
        }

        [Test, TestCaseSource(nameof(Endiannesses))]
        public void TestShortArray(Endianness order)
        {
            AssertSerialization(TestUtils.RandomArray(TestUtils.RandomShort), order);
        }

        [Test, TestCaseSource(nameof(Endiannesses))]
        public void TestString(Endianness order)
        {
            AssertSerialization(TestUtils.RandomString(), order);
        }

        [Test, TestCaseSource(nameof(Endiannesses))]
        public void TestStringArray(Endianness order)
        {
            AssertSerialization(TestUtils.RandomArray(TestUtils.RandomString), order);
        }

        [Test, TestCaseSource(nameof(Endiannesses))]
        public void TestNull(Endianness order)
        {
            AssertSerialization<object>(null, order);
        }

        [Test, TestCaseSource(nameof(Endiannesses))]
        public void TestDate(Endianness order)
        {
            var now = DateTime.UtcNow;

            //strip nanos as they will not be serialized
            var dateWithoutNanos = new DateTime(now.Ticks - now.Ticks % 10000);
            AssertSerialization(dateWithoutNanos, order);
        }

        [Test, TestCaseSource(nameof(Endiannesses))]
        public void TestList(Endianness order)
        {
            var list = new List<object> {"1", 2, 2.0};

            //strip nanos as they will not be serialized
            AssertSerialization(list, order);
        }

        [Test, TestCaseSource(nameof(Endiannesses))]
        public void TestLinkedList(Endianness order)
        {
            var list = new LinkedList<object>();

            list.AddLast("1");
            list.AddLast(2);
            list.AddLast(2.0);

            //strip nanos as they will not be serialized
            AssertSerialization(list, order);
        }

        [Test, TestCaseSource(nameof(Endiannesses))]
        public void TestSerializable(Endianness order)
        {
            var p = new SerializableClass { Age = TestUtils.RandomInt(), Name = TestUtils.RandomString() };

            AssertSerialization(p, order, true);
        }

        [Test, TestCaseSource(nameof(Endiannesses))]
        public void TestGenericListUsingSerializable(Endianness order)
        {
            var p = new List<string> { "a", "b", "c"};

            AssertSerialization(p, order, true);
        }

        [Test, TestCaseSource(nameof(Endiannesses))]
        public void TestBigInteger(Endianness order)
        {
            var bigInt = BigInteger.Parse("123456789012345678901234567890123456789012345678901234567890");
            AssertSerialization(bigInt, order);
        }

        [Test, TestCaseSource(nameof(Endiannesses))]
        public void TestJavaClass(Endianness order)
        {
            var javaClass = new JavaClass(TestUtils.RandomString());
            AssertSerialization(javaClass, order);
        }

        [Serializable]
        public class SerializableClass
        {
            public int Age { get; set; }
            public string Name { get; set; }

            protected bool Equals(SerializableClass other)
            {
                return Age == other.Age && string.Equals(Name, other.Name);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((SerializableClass)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (Age*397) ^ (Name != null ? Name.GetHashCode() : 0);
                }
            }
        }
    }
}
