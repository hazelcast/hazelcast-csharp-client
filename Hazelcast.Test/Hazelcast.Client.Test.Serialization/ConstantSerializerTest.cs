// Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
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
using System.Runtime.Serialization;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;
using NUnit.Framework;

namespace Hazelcast.Client.Test.Serialization
{
    [TestFixture]
    public class ConstantSerializerTest
    {
        private static readonly ByteOrder[] ByteOrders =
        {
            ByteOrder.BigEndian, ByteOrder.LittleEndian,
            ByteOrder.NativeOrder()
        };

        private ISerializationService CreateSerializationService(ByteOrder order)
        {
            return new SerializationServiceBuilder().
                SetByteOrder(order).SetUseNativeByteOrder(false).SetPortableVersion(1).Build();
        }

        private void AssertSerialization<T>(T obj, ByteOrder order)
        {
            var ss = CreateSerializationService(order);
            var data = ss.ToData(obj);
            var deserialized = ss.ToObject<T>(data);
            Assert.AreEqual(obj, deserialized);

            // test first time deserialization
            var ss2 = CreateSerializationService(order);
            Assert.AreEqual(obj, ss2.ToObject<T>(data));
        }

        [Test, TestCaseSource("ByteOrders")]
        public void TestBoolean(ByteOrder order)
        {
            AssertSerialization(TestSupport.RandomBool(), order);
        }

        [Test, TestCaseSource("ByteOrders")]
        public void TestBooleanArray(ByteOrder order)
        {
            AssertSerialization(TestSupport.RandomArray(TestSupport.RandomBool), order);
        }

        [Test, TestCaseSource("ByteOrders")]
        public void TestByte(ByteOrder order)
        {
            AssertSerialization(TestSupport.RandomByte(), order);
        }

        [Test, TestCaseSource("ByteOrders")]
        public void TestByteArray(ByteOrder order)
        {
            AssertSerialization(TestSupport.RandomArray(TestSupport.RandomByte), order);
        }

        [Test, TestCaseSource("ByteOrders")]
        public void TestChar(ByteOrder order)
        {
            AssertSerialization(TestSupport.RandomChar(), order);
        }

        [Test, TestCaseSource("ByteOrders")]
        public void TestCharArray(ByteOrder order)
        {
            AssertSerialization(TestSupport.RandomArray(TestSupport.RandomChar), order);
        }

        [Test, TestCaseSource("ByteOrders")]
        public void TestDouble(ByteOrder order)
        {
            AssertSerialization(TestSupport.RandomDouble(), order);
        }

        [Test, TestCaseSource("ByteOrders")]
        public void TestDoubleArray(ByteOrder order)
        {
            AssertSerialization(TestSupport.RandomArray(TestSupport.RandomDouble), order);
        }

        [Test, TestCaseSource("ByteOrders")]
        public void TestFloat(ByteOrder order)
        {
            AssertSerialization(TestSupport.RandomFloat(), order);
        }

        [Test, TestCaseSource("ByteOrders")]
        public void TestFloatArray(ByteOrder order)
        {
            AssertSerialization(TestSupport.RandomArray(TestSupport.RandomFloat), order);
        }

        [Test, TestCaseSource("ByteOrders")]
        public void TestInteger(ByteOrder order)
        {
            AssertSerialization(TestSupport.RandomInt(), order);
        }

        [Test, TestCaseSource("ByteOrders")]
        public void TestIntegerArray(ByteOrder order)
        {
            AssertSerialization(TestSupport.RandomArray(TestSupport.RandomInt), order);
        }

        [Test, TestCaseSource("ByteOrders")]
        public void TestLong(ByteOrder order)
        {
            AssertSerialization(TestSupport.RandomLong(), order);
        }

        [Test, TestCaseSource("ByteOrders")]
        public void TestLongArray(ByteOrder order)
        {
            AssertSerialization(TestSupport.RandomArray(TestSupport.RandomLong), order);
        }

        [Test, TestCaseSource("ByteOrders")]
        public void TestShort(ByteOrder order)
        {
            AssertSerialization(TestSupport.RandomShort(), order);
        }

        [Test, TestCaseSource("ByteOrders")]
        public void TestShortArray(ByteOrder order)
        {
            AssertSerialization(TestSupport.RandomArray(TestSupport.RandomShort), order);
        }

        [Test, TestCaseSource("ByteOrders")]
        public void TestString(ByteOrder order)
        {
            AssertSerialization(TestSupport.RandomString(), order);
        }

        [Test, TestCaseSource("ByteOrders")]
        public void TestStringArray(ByteOrder order)
        {
            AssertSerialization(TestSupport.RandomArray(TestSupport.RandomString), order);
        }

        [Test, TestCaseSource("ByteOrders")]
        public void TestNull(ByteOrder order)
        {
            AssertSerialization<object>(null, order);
        }

        [Test, TestCaseSource("ByteOrders")]
        public void TestDate(ByteOrder order)
        {
            var now = DateTime.UtcNow;

            //strip nanos as they will not be serialized
            var dateWithoutNanos = new DateTime(now.Ticks - now.Ticks % 10000);
            AssertSerialization(dateWithoutNanos, order);
        }

        [Test, TestCaseSource("ByteOrders")]
        public void TestList(ByteOrder order)
        {
            var list = new List<object> {"1", 2, 2.0};

            //strip nanos as they will not be serialized
            AssertSerialization(list, order);
        }

        [Test, TestCaseSource("ByteOrders")]
        public void TestLinkedList(ByteOrder order)
        {
            var list = new LinkedList<object>();

            list.AddLast("1");
            list.AddLast(2);
            list.AddLast(2.0);

            //strip nanos as they will not be serialized
            AssertSerialization(list, order);
        }

        [Test, TestCaseSource("ByteOrders")]
        public void TestSerializable(ByteOrder order)
        {
            var p = new SerializableClass {Age = TestSupport.RandomInt(), Name = TestSupport.RandomString()};

            AssertSerialization(p, order);
        }

        [Test, TestCaseSource("ByteOrders")]
        public void TestGenericListUsingSerializable(ByteOrder order)
        {
            var p = new List<string> { "a", "b", "c"};

            AssertSerialization(p, order);
        }

        [Test, TestCaseSource("ByteOrders")]
        public void TestBigInteger(ByteOrder order)
        {
            var bigInt = BigInteger.Parse("123456789012345678901234567890123456789012345678901234567890");
            AssertSerialization(bigInt, order);
        }

        [Test, TestCaseSource("ByteOrders")]
        public void TestJavaEnum(ByteOrder order)
        {
            var javaEnum = new JavaEnum(TestSupport.RandomString(), TestSupport.RandomString());
            AssertSerialization(javaEnum, order);
        }

        [Test, TestCaseSource("ByteOrders")]
        public void TestJavaClass(ByteOrder order)
        {
            var javaEnum = new JavaClass(TestSupport.RandomString());
            AssertSerialization(javaEnum, order);
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