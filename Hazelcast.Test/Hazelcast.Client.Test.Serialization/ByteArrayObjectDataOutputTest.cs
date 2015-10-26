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

using System;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;
using NUnit.Framework;

namespace Hazelcast.Client.Test.Serialization
{
    public class ByteArrayObjectDataOutputTest
    {
        private static readonly byte[] TestData = {1, 2, 3};
        private ByteArrayObjectDataOutput _output;

        [TearDown]
        public virtual void After()
        {
            _output.Close();
        }

        [SetUp]
        public virtual void Before()
        {
            _output = new ByteArrayObjectDataOutput(10, null, ByteOrder.BigEndian);
        }

        [Test]
        public virtual void TestAvailable()
        {
            var available = _output.Available();
            _output.buffer = null;
            var availableWhenBufferNull = _output.Available();
            Assert.AreEqual(10, available);
            Assert.AreEqual(0, availableWhenBufferNull);
        }

        [Test]
        public virtual void TestClear()
        {
            _output.Clear();
            Assert.AreEqual(0, _output.Position());
            Assert.AreEqual(10, _output.Available());
        }

        [Test]
        public virtual void TestClear_bufferLen_lt_initX8()
        {
            _output.EnsureAvailable(10*10);
            _output.Clear();
            Assert.AreEqual(10*8, _output.Available());
        }

        [Test]
        public virtual void TestClear_bufferNull()
        {
            _output.buffer = null;
            _output.Clear();
            Assert.IsNull(_output.buffer);
        }

        [Test]
        public virtual void TestClose()
        {
            _output.Close();
            Assert.AreEqual(0, _output.Position());
            Assert.IsNull(_output.buffer);
        }

        [Test]
        public virtual void TestEnsureAvailable()
        {
            _output.buffer = null;
            _output.EnsureAvailable(5);
            Assert.AreEqual(10, _output.buffer.Length);
        }

        [Test]
        public virtual void TestEnsureAvailable_smallLen()
        {
            _output.buffer = null;
            _output.EnsureAvailable(1);
            Assert.AreEqual(10, _output.buffer.Length);
        }

        [Test]
        public virtual void TestGetByteOrder()
        {
            var outLE = new ByteArrayObjectDataOutput(10, null, ByteOrder.LittleEndian);
            var outBE = new ByteArrayObjectDataOutput(10, null, ByteOrder.BigEndian);
            Assert.AreEqual(ByteOrder.LittleEndian, outLE.GetByteOrder());
            Assert.AreEqual(ByteOrder.BigEndian, outBE.GetByteOrder());
        }

        [Test]
        public virtual void TestPosition()
        {
            _output.pos = 21;
            Assert.AreEqual(21, _output.Position());
        }

        [Test]
        public virtual void TestPositionNewPos()
        {
            _output.Position(1);
            Assert.AreEqual(1, _output.pos);
        }

        public virtual void TestPositionNewPos_highPos()
        {
            _output.Position(_output.buffer.Length + 1);
        }

        public virtual void TestPositionNewPos_negativePos()
        {
            _output.Position(-1);
        }

        [Test]
        public virtual void TestToByteArray()
        {
            var arrayWhenPosZero = _output.ToByteArray();
            _output.buffer = null;
            var arrayWhenBufferNull = _output.ToByteArray();
            Assert.AreEqual(new byte[0], arrayWhenPosZero);
            Assert.AreEqual(new byte[0], arrayWhenBufferNull);
        }

        [Test]
        public virtual void TestToString()
        {
            Assert.IsNotNull(_output.ToString());
        }

        [Test]
        public virtual void TestWriteBooleanForPositionV()
        {
            _output.WriteBoolean(0, true);
            _output.WriteBoolean(1, false);
            Assert.AreEqual(1, _output.buffer[0]);
            Assert.AreEqual(0, _output.buffer[1]);
        }

        [Test]
        public virtual void TestWriteByteForPositionV()
        {
            _output.WriteByte(0, 10);
            Assert.AreEqual(10, _output.buffer[0]);
        }

        [Test]
        public virtual void TestWriteDoubleForPositionV()
        {
            var v = 1.1d;
            _output.WriteDouble(1, v);
            long theLong = BitConverter.DoubleToInt64Bits(v);
            var readLongB = Bits.ReadLongB(_output.buffer, 1);
            Assert.AreEqual(theLong, readLongB);
        }

        [Test]
        public virtual void TestWriteDoubleForPositionVByteOrder()
        {
            var v = 1.1d;
            _output.WriteDouble(1, v, ByteOrder.LittleEndian);
            long theLong = BitConverter.DoubleToInt64Bits(v);
            var readLongB = Bits.ReadLongL(_output.buffer, 1);
            Assert.AreEqual(theLong, readLongB);
        }

        [Test]
        public virtual void TestWriteDoubleForVByteOrder()
        {
            var v = 1.1d;
            _output.WriteDouble(v, ByteOrder.LittleEndian);
            long theLong = BitConverter.DoubleToInt64Bits(v);
            var readLongB = Bits.ReadLongL(_output.buffer, 0);
            Assert.AreEqual(theLong, readLongB);
        }

        [Test]
        public virtual void TestWriteFloatForPositionV()
        {
            var v = 1.1f;
            _output.WriteFloat(1, v);
            int expected = BitConverter.ToInt32(BitConverter.GetBytes(v), 0);
            var actual = Bits.ReadIntB(_output.buffer, 1);
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public virtual void TestWriteFloatForPositionVByteOrder()
        {
            var v = 1.1f;
            _output.WriteFloat(1, v, ByteOrder.LittleEndian);
            int expected = BitConverter.ToInt32(BitConverter.GetBytes(v), 0);
            var actual = Bits.ReadIntL(_output.buffer, 1);
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public virtual void TestWriteFloatForVByteOrder()
        {
            var v = 1.1f;
            _output.WriteFloat(v, ByteOrder.LittleEndian);
            int expected = BitConverter.ToInt32(BitConverter.GetBytes(v), 0);
            var actual = Bits.ReadIntL(_output.buffer, 0);
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public virtual void TestWriteFloatV()
        {
            var v = 1.1f;
            _output.WriteFloat(v);
            int expected = BitConverter.ToInt32(BitConverter.GetBytes(v), 0);
            var actual = Bits.ReadIntB(_output.buffer, 0);
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public virtual void TestWriteForBOffLen()
        {
            var zeroBytes = new byte[20];
            _output.Write(zeroBytes, 0, 20);
            byte[] bytes = new byte[20];
            Array.Copy(_output.buffer, 0, bytes, 0, 20);
            Assert.AreEqual(zeroBytes, bytes);
            Assert.AreEqual(20, _output.pos);
        }

        public virtual void TestWriteForBOffLen_negativeLen()
        {
            _output.Write(TestData, 0, -3);
        }

        public virtual void TestWriteForBOffLen_negativeOff()
        {
            _output.Write(TestData, -1, 3);
        }

        public virtual void TestWriteForBOffLen_OffLenHigherThenSize()
        {
            _output.Write(TestData, 0, -3);
        }

        [Test]
        public virtual void TestWriteForPositionB()
        {
            _output.Write(1, 5);
            Assert.AreEqual(5, _output.buffer[1]);
        }

        [Test]
        public virtual void TestWriteIntForPositionV()
        {
            var expected = 100;
            _output.WriteInt(1, expected);
            var actual = Bits.ReadIntB(_output.buffer, 1);
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public virtual void TestWriteIntForPositionVByteOrder()
        {
            var expected = 100;
            _output.WriteInt(2, expected, ByteOrder.LittleEndian);
            var actual = Bits.ReadIntL(_output.buffer, 2);
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public virtual void TestWriteIntForVByteOrder()
        {
            var expected = 100;
            _output.WriteInt(expected, ByteOrder.LittleEndian);
            var actual = Bits.ReadIntL(_output.buffer, 0);
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public virtual void TestWriteIntV()
        {
            var expected = 100;
            _output.WriteInt(expected);
            var actual = Bits.ReadIntB(_output.buffer, 0);
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public virtual void TestWriteLongForPositionV()
        {
            long expected = 100;
            _output.WriteLong(2, expected);
            var actual = Bits.ReadLongB(_output.buffer, 2);
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public virtual void TestWriteLongForPositionVByteOrder()
        {
            long expected = 100;
            _output.WriteLong(2, expected, ByteOrder.LittleEndian);
            var actual = Bits.ReadLongL(_output.buffer, 2);
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public virtual void TestWriteLongForVByteOrder()
        {
            long expected = 100;
            _output.WriteLong(2, expected, ByteOrder.LittleEndian);
            var actual = Bits.ReadLongL(_output.buffer, 2);
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public virtual void TestWriteLongV()
        {
            long expected = 100;
            _output.WriteLong(expected);
            var actual = Bits.ReadLongB(_output.buffer, 0);
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public virtual void TestWriteShortForPositionV()
        {
            short expected = 100;
            _output.WriteShort(2, expected);
            var actual = Bits.ReadShortB(_output.buffer, 2);
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public virtual void TestWriteShortForPositionVByteOrder()
        {
            short expected = 100;
            _output.WriteShort(2, expected, ByteOrder.LittleEndian);
            var actual = Bits.ReadShortL(_output.buffer, 2);
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public virtual void TestWriteShortForVByteOrder()
        {
            short expected = 100;
            _output.WriteShort(2, expected, ByteOrder.LittleEndian);
            var actual = Bits.ReadShortL(_output.buffer, 2);
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public virtual void TestWriteShortV()
        {
            short expected = 100;
            _output.WriteShort(expected);
            var actual = Bits.ReadShortB(_output.buffer, 0);
            Assert.AreEqual(actual, expected);
        }
    }
}
