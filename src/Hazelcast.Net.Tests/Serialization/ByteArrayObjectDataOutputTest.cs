// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using NUnit.Framework;

namespace Hazelcast.Tests.Serialization
{
    public class ByteArrayObjectDataOutputTest
    {
        private static readonly byte[] TestData = {1, 2, 3};
        private ByteArrayObjectDataOutput _output;

        [TearDown]
        public virtual void After()
        {
            _output.Dispose();
        }

        [SetUp]
        public virtual void Before()
        {
            _output = new ByteArrayObjectDataOutput(10, null, Endianness.BigEndian);
        }

        [Test]
        public virtual void TestAvailable()
        {
            var available = _output.Buffer.Length;
            Assert.AreEqual(10, available);
        }

        [Test]
        public virtual void TestClear()
        {
            _output.Clear();
            Assert.AreEqual(0, _output.Position);
            Assert.AreEqual(10, _output.Buffer.Length);
        }

        [Test]
        public virtual void TestClear_bufferLen_lt_initX8()
        {
            _output.Validate(0, 10*10);
            _output.Clear();
            Assert.AreEqual(10*8, _output.Buffer.Length);
        }

        [Test]
        public virtual void TestClear_bufferNull()
        {
            _output.Clear();
            Assert.That(_output.Buffer, Is.Not.Null);
        }

        [Test]
        public virtual void TestClose()
        {
            _output.Dispose();
            Assert.AreEqual(0, _output.Position);
        }

        [Test]
        public virtual void TestEnsureAvailable()
        {
            _output = new ByteArrayObjectDataOutput(0, null, Endianness.BigEndian);
            _output.Validate(0, 5);
            Assert.AreEqual(10, _output.Buffer.Length);
        }

        // but ... we don't reset buffer to null anymore?
        /*
        [Test]
        public virtual void TestEnsureAvailable_smallLen()
        {
            _output = new ByteArrayObjectDataOutput(10, null, Endianness.BigEndian);
            _output.Buffer = null;
            _output.Validate(0, 1);
            Assert.AreEqual(10, _output.Buffer.Length);
        }
        */

        [Test]
        public virtual void TestToByteArray()
        {
            var arrayWhenPosZero = _output.ToByteArray();
            Assert.AreEqual(new byte[0], arrayWhenPosZero);
        }

        [Test]
        public virtual void TestToString()
        {
            Assert.IsNotNull(_output.ToString());
        }

        [Test]
        public virtual void TestWriteBooleanForPositionV()
        {
            _output.Write(0, true);
            _output.Write(1, false);
            Assert.AreEqual(1, _output.Buffer[0]);
            Assert.AreEqual(0, _output.Buffer[1]);
        }

        [Test]
        public virtual void TestWriteByteForPositionV()
        {
            _output.Write(0, (byte) 10);
            Assert.AreEqual(10, _output.Buffer[0]);
        }

        [Test]
        public virtual void TestWriteDoubleForPositionV()
        {
            var v = 1.1d;
            _output.Write(1, v);
            var theLong = BitConverter.DoubleToInt64Bits(v);
            var readLongB = BytesExtensions.ReadLong(_output.Buffer, 1, Endianness.BigEndian);
            Assert.AreEqual(theLong, readLongB);
        }

        [Test]
        public virtual void TestWriteDoubleForPositionVEndianness()
        {
            var v = 1.1d;
            _output.Write(1, v, Endianness.LittleEndian);
            var theLong = BitConverter.DoubleToInt64Bits(v);
            var readLongB = BytesExtensions.ReadLongL(_output.Buffer, 1);
            Assert.AreEqual(theLong, readLongB);
        }

        [Test]
        public virtual void TestWriteDoubleForVEndianness()
        {
            var v = 1.1d;
            _output.Write(v, Endianness.LittleEndian);
            var theLong = BitConverter.DoubleToInt64Bits(v);
            var readLongB = BytesExtensions.ReadLongL(_output.Buffer, 0);
            Assert.AreEqual(theLong, readLongB);
        }

        [Test]
        public virtual void TestWriteFloatForPositionV()
        {
            var v = 1.1f;
            _output.Write(1, v);
            var expected = BitConverter.ToInt32(BitConverter.GetBytes(v), 0);
            var actual = BytesExtensions.ReadInt(_output.Buffer, 1, Endianness.BigEndian);
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public virtual void TestWriteFloatForPositionVEndianness()
        {
            var v = 1.1f;
            _output.Write(1, v, Endianness.LittleEndian);
            var expected = BitConverter.ToInt32(BitConverter.GetBytes(v), 0);
            var actual = BytesExtensions.ReadIntL(_output.Buffer, 1);
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public virtual void TestWriteFloatForVEndianness()
        {
            var v = 1.1f;
            _output.Write(v, Endianness.LittleEndian);
            var expected = BitConverter.ToInt32(BitConverter.GetBytes(v), 0);
            var actual = BytesExtensions.ReadIntL(_output.Buffer, 0);
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public virtual void TestWriteFloatV()
        {
            var v = 1.1f;
            _output.Write(v);
            var expected = BitConverter.ToInt32(BitConverter.GetBytes(v), 0);
            var actual = BytesExtensions.ReadInt(_output.Buffer, 0, Endianness.BigEndian);
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public virtual void TestWriteForBOffLen()
        {
            var zeroBytes = new byte[20];
            _output.WriteBytes(zeroBytes, 0, 20);
            var bytes = new byte[20];
            Array.Copy(_output.Buffer, 0, bytes, 0, 20);
            Assert.AreEqual(zeroBytes, bytes);
            Assert.AreEqual(20, _output.Position);
        }

        public virtual void TestWriteForBOffLen_negativeLen()
        {
            _output.WriteBytes(TestData, 0, -3);
        }

        public virtual void TestWriteForBOffLen_negativeOff()
        {
            _output.WriteBytes(TestData, -1, 3);
        }

        public virtual void TestWriteForBOffLen_OffLenHigherThenSize()
        {
            _output.WriteBytes(TestData, 0, -3);
        }

        [Test]
        public virtual void TestWriteForPositionB()
        {
            _output.Write(1, (byte) 5);
            Assert.AreEqual(5, _output.Buffer[1]);
        }

        [Test]
        public virtual void TestWriteIntForPositionV()
        {
            var expected = 100;
            _output.Write(1, expected);
            var actual = BytesExtensions.ReadInt(_output.Buffer, 1, Endianness.BigEndian);
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public virtual void TestWriteIntForPositionVEndianness()
        {
            var expected = 100;
            _output.Write(2, expected, Endianness.LittleEndian);
            var actual = BytesExtensions.ReadIntL(_output.Buffer, 2);
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public virtual void TestWriteIntForVEndianness()
        {
            var expected = 100;
            _output.Write(expected, Endianness.LittleEndian);
            var actual = BytesExtensions.ReadIntL(_output.Buffer, 0);
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public virtual void TestWriteIntV()
        {
            var expected = 100;
            _output.Write(expected);
            var actual = BytesExtensions.ReadInt(_output.Buffer, 0, Endianness.BigEndian);
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public virtual void TestWriteLongForPositionV()
        {
            long expected = 100;
            _output.Write(2, expected);
            var actual = BytesExtensions.ReadLong(_output.Buffer, 2, Endianness.BigEndian);
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public virtual void TestWriteLongForPositionVEndianness()
        {
            long expected = 100;
            _output.Write(2, expected, Endianness.LittleEndian);
            var actual = BytesExtensions.ReadLongL(_output.Buffer, 2);
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public virtual void TestWriteLongForVEndianness()
        {
            long expected = 100;
            _output.Write(2, expected, Endianness.LittleEndian);
            var actual = BytesExtensions.ReadLongL(_output.Buffer, 2);
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public virtual void TestWriteLongV()
        {
            long expected = 100;
            _output.Write(expected);
            var actual = BytesExtensions.ReadLong(_output.Buffer, 0, Endianness.BigEndian);
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public virtual void TestWriteShortForPositionV()
        {
            short expected = 100;
            _output.Write(2, expected);
            var actual = BytesExtensions.ReadShort(_output.Buffer, 2, Endianness.BigEndian);
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public virtual void TestWriteShortForPositionVEndianness()
        {
            short expected = 100;
            _output.Write(2, expected, Endianness.LittleEndian);
            var actual = BytesExtensions.ReadShort(_output.Buffer, 2, Endianness.LittleEndian);
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public virtual void TestWriteShortForVEndianness()
        {
            short expected = 100;
            _output.Write(2, expected, Endianness.LittleEndian);
            var actual = BytesExtensions.ReadShort(_output.Buffer, 2, Endianness.LittleEndian);
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public virtual void TestWriteShortV()
        {
            short expected = 100;
            _output.Write(expected);
            var actual = BytesExtensions.ReadShort(_output.Buffer, 0, Endianness.BigEndian);
            Assert.AreEqual(actual, expected);
        }
    }
}