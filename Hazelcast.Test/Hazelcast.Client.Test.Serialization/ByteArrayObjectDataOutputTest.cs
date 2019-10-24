// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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

        [SetUp]
        public void Setup()
        {
            _output = new ByteArrayObjectDataOutput(10, null, ByteOrder.BigEndian);
        }

        [TearDown]
        public void TearDown()
        {
            _output.Close();
        }

        [Test]
        public void Available()
        {
            var available = _output.Available();
            _output.Buffer = null;
            var availableWhenBufferNull = _output.Available();
            Assert.AreEqual(10, available);
            Assert.AreEqual(0, availableWhenBufferNull);
        }

        [Test]
        public void Clear()
        {
            _output.Clear();
            Assert.AreEqual(0, _output.Position());
            Assert.AreEqual(10, _output.Available());
        }

        [Test]
        public void Clear_bufferLen_lt_initX8()
        {
            _output.EnsureAvailable(10*10);
            _output.Clear();
            Assert.AreEqual(10*8, _output.Available());
        }

        [Test]
        public void Clear_bufferNull()
        {
            _output.Buffer = null;
            _output.Clear();
            Assert.IsNull(_output.Buffer);
        }

        [Test]
        public void Close()
        {
            _output.Close();
            Assert.AreEqual(0, _output.Position());
            Assert.IsNull(_output.Buffer);
        }

        [Test]
        public void EnsureAvailable()
        {
            _output.Buffer = null;
            _output.EnsureAvailable(5);
            Assert.AreEqual(10, _output.Buffer.Length);
        }

        [Test]
        public void EnsureAvailable_smallLen()
        {
            _output.Buffer = null;
            _output.EnsureAvailable(1);
            Assert.AreEqual(10, _output.Buffer.Length);
        }

        [Test]
        public void GetByteOrder()
        {
            var outLE = new ByteArrayObjectDataOutput(10, null, ByteOrder.LittleEndian);
            var outBE = new ByteArrayObjectDataOutput(10, null, ByteOrder.BigEndian);
            Assert.AreEqual(ByteOrder.LittleEndian, outLE.GetByteOrder());
            Assert.AreEqual(ByteOrder.BigEndian, outBE.GetByteOrder());
        }

        [Test]
        public void Position()
        {
            _output.Pos = 21;
            Assert.AreEqual(21, _output.Position());
        }

        [Test]
        public void PositionNewPos()
        {
            _output.Position(1);
            Assert.AreEqual(1, _output.Pos);
        }

        public void PositionNewPos_highPos()
        {
            _output.Position(_output.Buffer.Length + 1);
        }

        public void PositionNewPos_negativePos()
        {
            _output.Position(-1);
        }

        [Test]
        public void ToByteArray()
        {
            var arrayWhenPosZero = _output.ToByteArray();
            _output.Buffer = null;
            var arrayWhenBufferNull = _output.ToByteArray();
            Assert.AreEqual(new byte[0], arrayWhenPosZero);
            Assert.AreEqual(new byte[0], arrayWhenBufferNull);
        }

        [Test]
        public void ToString()
        {
            Assert.IsNotNull(_output.ToString());
        }

        [Test]
        public void WriteBooleanForPositionV()
        {
            _output.WriteBoolean(0, true);
            _output.WriteBoolean(1, false);
            Assert.AreEqual(1, _output.Buffer[0]);
            Assert.AreEqual(0, _output.Buffer[1]);
        }

        [Test]
        public void WriteByteForPositionV()
        {
            _output.WriteByte(0, 10);
            Assert.AreEqual(10, _output.Buffer[0]);
        }

        [Test]
        public void WriteDoubleForPositionV()
        {
            var v = 1.1d;
            _output.WriteDouble(1, v);
            var theLong = BitConverter.DoubleToInt64Bits(v);
            var readLongB = Bits.ReadLongB(_output.Buffer, 1);
            Assert.AreEqual(theLong, readLongB);
        }

        [Test]
        public void WriteDoubleForPositionVByteOrder()
        {
            var v = 1.1d;
            _output.WriteDouble(1, v, ByteOrder.LittleEndian);
            var theLong = BitConverter.DoubleToInt64Bits(v);
            var readLongB = Bits.ReadLongL(_output.Buffer, 1);
            Assert.AreEqual(theLong, readLongB);
        }

        [Test]
        public void WriteDoubleForVByteOrder()
        {
            var v = 1.1d;
            _output.WriteDouble(v, ByteOrder.LittleEndian);
            var theLong = BitConverter.DoubleToInt64Bits(v);
            var readLongB = Bits.ReadLongL(_output.Buffer, 0);
            Assert.AreEqual(theLong, readLongB);
        }

        [Test]
        public void WriteFloatForPositionV()
        {
            var v = 1.1f;
            _output.WriteFloat(1, v);
            var expected = BitConverter.ToInt32(BitConverter.GetBytes(v), 0);
            var actual = Bits.ReadIntB(_output.Buffer, 1);
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public void WriteFloatForPositionVByteOrder()
        {
            var v = 1.1f;
            _output.WriteFloat(1, v, ByteOrder.LittleEndian);
            var expected = BitConverter.ToInt32(BitConverter.GetBytes(v), 0);
            var actual = Bits.ReadIntL(_output.Buffer, 1);
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public void WriteFloatForVByteOrder()
        {
            var v = 1.1f;
            _output.WriteFloat(v, ByteOrder.LittleEndian);
            var expected = BitConverter.ToInt32(BitConverter.GetBytes(v), 0);
            var actual = Bits.ReadIntL(_output.Buffer, 0);
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public void WriteFloatV()
        {
            var v = 1.1f;
            _output.WriteFloat(v);
            var expected = BitConverter.ToInt32(BitConverter.GetBytes(v), 0);
            var actual = Bits.ReadIntB(_output.Buffer, 0);
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public void WriteForBOffLen()
        {
            var zeroBytes = new byte[20];
            _output.Write(zeroBytes, 0, 20);
            var bytes = new byte[20];
            Array.Copy(_output.Buffer, 0, bytes, 0, 20);
            Assert.AreEqual(zeroBytes, bytes);
            Assert.AreEqual(20, _output.Pos);
        }

        public void WriteForBOffLen_negativeLen()
        {
            _output.Write(TestData, 0, -3);
        }

        public void WriteForBOffLen_negativeOff()
        {
            _output.Write(TestData, -1, 3);
        }

        public void WriteForBOffLen_OffLenHigherThenSize()
        {
            _output.Write(TestData, 0, -3);
        }

        [Test]
        public void WriteForPositionB()
        {
            _output.Write(1, 5);
            Assert.AreEqual(5, _output.Buffer[1]);
        }

        [Test]
        public void WriteIntForPositionV()
        {
            var expected = 100;
            _output.WriteInt(1, expected);
            var actual = Bits.ReadIntB(_output.Buffer, 1);
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public void WriteIntForPositionVByteOrder()
        {
            var expected = 100;
            _output.WriteInt(2, expected, ByteOrder.LittleEndian);
            var actual = Bits.ReadIntL(_output.Buffer, 2);
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public void WriteIntForVByteOrder()
        {
            var expected = 100;
            _output.WriteInt(expected, ByteOrder.LittleEndian);
            var actual = Bits.ReadIntL(_output.Buffer, 0);
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public void WriteIntV()
        {
            var expected = 100;
            _output.WriteInt(expected);
            var actual = Bits.ReadIntB(_output.Buffer, 0);
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public void WriteLongForPositionV()
        {
            long expected = 100;
            _output.WriteLong(2, expected);
            var actual = Bits.ReadLongB(_output.Buffer, 2);
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public void WriteLongForPositionVByteOrder()
        {
            long expected = 100;
            _output.WriteLong(2, expected, ByteOrder.LittleEndian);
            var actual = Bits.ReadLongL(_output.Buffer, 2);
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public void WriteLongForVByteOrder()
        {
            long expected = 100;
            _output.WriteLong(2, expected, ByteOrder.LittleEndian);
            var actual = Bits.ReadLongL(_output.Buffer, 2);
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public void WriteLongV()
        {
            long expected = 100;
            _output.WriteLong(expected);
            var actual = Bits.ReadLongB(_output.Buffer, 0);
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public void WriteShortForPositionV()
        {
            short expected = 100;
            _output.WriteShort(2, expected);
            var actual = Bits.ReadShortB(_output.Buffer, 2);
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public void WriteShortForPositionVByteOrder()
        {
            short expected = 100;
            _output.WriteShort(2, expected, ByteOrder.LittleEndian);
            var actual = Bits.ReadShortL(_output.Buffer, 2);
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public void WriteShortForVByteOrder()
        {
            short expected = 100;
            _output.WriteShort(2, expected, ByteOrder.LittleEndian);
            var actual = Bits.ReadShortL(_output.Buffer, 2);
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public void WriteShortV()
        {
            short expected = 100;
            _output.WriteShort(expected);
            var actual = Bits.ReadShortB(_output.Buffer, 0);
            Assert.AreEqual(actual, expected);
        }
    }
}