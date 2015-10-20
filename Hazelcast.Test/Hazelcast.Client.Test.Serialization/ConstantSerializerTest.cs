/*
* Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

﻿using Hazelcast.IO.Serialization;
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

        private void AssertSerialization<T>(T obj, ByteOrder order)
        {
            var ss = CreateSerializationService(order);
            var data = ss.ToData(obj);
            Assert.AreEqual(obj, ss.ToObject<T>(data));
        }
    }
}
