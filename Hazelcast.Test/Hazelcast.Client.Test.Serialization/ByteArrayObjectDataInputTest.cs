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
    public class ByteArrayObjectDataInputTest
    {
        private static readonly byte[] InitData = {0, 1, 2, 3, 4, 5, 6, 7, 8, 9};
        private ByteArrayObjectDataInput _input;

        [TearDown]
        public virtual void After()
        {
            _input.Close();
        }

        [SetUp]
        public virtual void Before()
        {
            _input = new ByteArrayObjectDataInput(InitData, null, ByteOrder.BigEndian);
        }

        [Test]
        public virtual void TestAvailable()
        {
            Assert.AreEqual(_input.size - _input.pos, _input.Available());
        }

        public virtual void TestCheckAvailable()
        {
            _input.CheckAvailable(-1, InitData.Length);
        }

        public virtual void TestCheckAvailable_EOF()
        {
            _input.CheckAvailable(0, InitData.Length + 1);
        }

        [Test]
        public virtual void TestClose()
        {
            _input.Close();
            Assert.IsNull(_input.data);
            Assert.IsNull(_input.charBuffer);
        }

        [Test]
        public virtual void TestGetByteOrder()
        {
            var input = new ByteArrayObjectDataInput(InitData, null, ByteOrder.LittleEndian);
            Assert.AreEqual(ByteOrder.BigEndian, _input.GetByteOrder());
            Assert.AreEqual(ByteOrder.LittleEndian, input.GetByteOrder());
        }

        [Test]
        public virtual void TestInit()
        {
            _input.Init(InitData, 2);
            Assert.AreEqual(InitData, _input.data);
            Assert.AreEqual(InitData.Length, _input.size);
            Assert.AreEqual(2, _input.pos);
        }

        [Test]
        public virtual void TestMark()
        {
            _input.Position(1);
            _input.Mark(1);
            Assert.AreEqual(1, _input.mark);
        }

        [Test]
        public virtual void TestMarkSupported()
        {
            Assert.IsTrue(_input.MarkSupported());
        }

        [Test]
        public virtual void TestPosition()
        {
            Assert.AreEqual(0, _input.Position());
        }

        [Test]
        public virtual void TestPositionNewPos()
        {
            _input.Position(InitData.Length - 1);
            Assert.AreEqual(InitData.Length - 1, _input.Position());
        }

        public virtual void TestPositionNewPos_HighNewPos()
        {
            _input.Position(InitData.Length + 10);
        }

        [Test]
        public virtual void TestPositionNewPos_mark()
        {
            _input.Position(InitData.Length - 1);
            _input.Mark(0);
            var firstMarked = _input.mark;
            _input.Position(1);
            Assert.AreEqual(InitData.Length - 1, firstMarked);
            Assert.AreEqual(1, _input.Position());
            Assert.AreEqual(-1, _input.mark);
        }

        public virtual void TestPositionNewPos_negativeNewPos()
        {
            _input.Position(-1);
        }

        [Test]
        public virtual void TestRead()
        {
            var read = _input.Read();
            Assert.AreEqual(0, read);
        }

        [Test]
        public virtual void TestReadBoolean()
        {
            var read1 = _input.ReadBoolean();
            var read2 = _input.ReadBoolean();
            Assert.IsFalse(read1);
            Assert.IsTrue(read2);
        }

        public virtual void TestReadBoolean_EOF()
        {
            _input.pos = InitData.Length + 1;
            _input.ReadBoolean();
        }

        [Test]
        public virtual void TestReadBooleanArray()
        {
            byte[] bytes1 =
            {
                0, 0, 0, 0, 0, 0, 0, 1, 8, 9, unchecked((byte) (-1)), unchecked((byte) (-1)),
                unchecked((byte) (-1)), unchecked((byte) (-1))
            };
            _input.Init(bytes1, 0);
            _input.Position(10);
            var theNullArray = _input.ReadBooleanArray();
            _input.Position(0);
            var theZeroLenghtArray = _input.ReadBooleanArray();
            _input.Position(4);
            var booleanArray = _input.ReadBooleanArray();
            Assert.IsNull(theNullArray);
            Assert.AreEqual(new bool[0], theZeroLenghtArray);
            Assert.AreEqual(new[] {true}, booleanArray);
        }

        [Test]
        public virtual void TestReadBooleanPosition()
        {
            var read1 = _input.ReadBoolean(0);
            var read2 = _input.ReadBoolean(1);
            Assert.IsFalse(read1);
            Assert.IsTrue(read2);
        }

        public virtual void TestReadBooleanPosition_EOF()
        {
            _input.ReadBoolean(InitData.Length + 1);
        }

        [Test]
        public virtual void TestReadByte()
        {
            int read = _input.ReadByte();
            Assert.AreEqual(0, read);
        }

        public virtual void TestReadByte_EOF()
        {
            _input.pos = InitData.Length + 1;
            _input.ReadByte();
        }

        [Test]
        public virtual void TestReadByteArray()
        {
            byte[] bytes1 =
            {
                0, 0, 0, 0, 0, 0, 0, 1, 8, 9, unchecked((byte) (-1)), unchecked((byte) (-1)),
                unchecked((byte) (-1)), unchecked((byte) (-1))
            };
            _input.Init(bytes1, 0);
            _input.Position(10);
            var theNullArray = _input.ReadByteArray();
            _input.Position(0);
            var theZeroLenghtArray = _input.ReadByteArray();
            _input.Position(4);
            var bytes = _input.ReadByteArray();
            Assert.IsNull(theNullArray);
            Assert.AreEqual(new byte[0], theZeroLenghtArray);
            Assert.AreEqual(new byte[] {8}, bytes);
        }

        [Test]
        public virtual void TestReadBytePosition()
        {
            int read = _input.ReadByte(1);
            Assert.AreEqual(1, read);
        }

        public virtual void TestReadBytePosition_EOF()
        {
            _input.ReadByte(InitData.Length + 1);
        }

        [Test]
        public virtual void TestReadChar()
        {
            var c = _input.ReadChar();
            Assert.AreEqual(1, c);
        }

        [Test]
        public virtual void TestReadCharArray()
        {
            byte[] bytes1 =
            {
                0, 0, 0, 0, 0, 0, 0, 1, 0, 1, unchecked((byte) (-1)), unchecked((byte) (-1)),
                unchecked((byte) (-1)), unchecked((byte) (-1))
            };
            _input.Init(bytes1, 0);
            _input.Position(10);
            var theNullArray = _input.ReadCharArray();
            _input.Position(0);
            var theZeroLenghtArray = _input.ReadCharArray();
            _input.Position(4);
            var booleanArray = _input.ReadCharArray();
            Assert.IsNull(theNullArray);
            Assert.AreEqual(new char[0], theZeroLenghtArray);
            Assert.AreEqual(new[] {(char) 1}, booleanArray);
        }

        [Test]
        public virtual void TestReadCharPosition()
        {
            var c = _input.ReadChar(0);
            Assert.AreEqual(1, c);
        }

        [Test]
        public virtual void TestReadData()
        {
            byte[] bytes1 =
            {
                0, 0, 0, 0, 0, 0, 0, 8, unchecked((byte) (-1)), unchecked((byte) (-1)), unchecked((byte) (-1)),
                unchecked((byte) (-1)), 0, 0, 0, 0, 0, 1, unchecked((byte) (-1)), unchecked((byte) (-1)),
                unchecked((byte) (-1)), unchecked(
                    (byte) (-1))
            };
            _input.Init(bytes1, 0);
            _input.Position(bytes1.Length - 4);
            var nullData = _input.ReadData();
            _input.Position(0);
            var theZeroLenghtArray = _input.ReadData();
            _input.Position(4);
            var data = _input.ReadData();
            Assert.IsNull(nullData);
            Assert.AreEqual(0, theZeroLenghtArray.GetTypeId());
            Assert.AreEqual(new byte[0], theZeroLenghtArray.ToByteArray());
            Assert.AreEqual(
                new byte[]
                {
                    unchecked((byte) (-1)), unchecked((byte) (-1)), unchecked((byte) (-1)), unchecked((byte) (-1)), 0, 0,
                    0,
                    0
                }, data.ToByteArray());
        }

        [Test]
        public virtual void TestReadDouble()
        {
            var readDouble = _input.ReadDouble();
            var longB = Bits.ReadLongB(InitData, 0);
            var aDouble = BitConverter.Int64BitsToDouble(longB);
            Assert.AreEqual(aDouble, readDouble, 0);
        }

        [Test]
        public virtual void TestReadDoubleArray()
        {
            byte[] bytes1 =
            {
                0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, unchecked((byte) (-1)),
                unchecked((byte) (-1)), unchecked((byte) (-1)), unchecked((byte) (-1))
            };
            _input.Init(bytes1, 0);
            var theZeroLenghtArray = _input.ReadDoubleArray();
            Assert.AreEqual(new double[0], theZeroLenghtArray);
        }

        [Test]
        public virtual void TestReadDoubleByteOrder()
        {
            var readDouble = _input.ReadDouble(ByteOrder.LittleEndian);
            var longB = Bits.ReadLong(InitData, 0, false);
            var aDouble = BitConverter.Int64BitsToDouble(longB);
            Assert.AreEqual(aDouble, readDouble, 0);
        }

        [Test]
        public virtual void TestReadDoubleForPositionByteOrder()
        {
            var readDouble = _input.ReadDouble(2, ByteOrder.LittleEndian);
            var longB = Bits.ReadLong(InitData, 2, false);
            var aDouble = BitConverter.Int64BitsToDouble(longB);
            Assert.AreEqual(aDouble, readDouble, 0);
        }

        [Test]
        public virtual void TestReadDoublePosition()
        {
            var readDouble = _input.ReadDouble(2);
            var longB = Bits.ReadLongB(InitData, 2);
            var aDouble = BitConverter.Int64BitsToDouble(longB);
            Assert.AreEqual(aDouble, readDouble, 0);
        }

        [Test]
        public virtual void TestReadFloat()
        {
            double readFloat = _input.ReadFloat();
            var intB = Bits.ReadIntB(InitData, 0);
            double aFloat = BitConverter.ToSingle(BitConverter.GetBytes(intB), 0);
            Assert.AreEqual(aFloat, readFloat, 0);
        }

        [Test]
        public virtual void TestReadFloatArray()
        {
            byte[] bytes1 =
            {
                0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, unchecked((byte) (-1)),
                unchecked((byte) (-1)), unchecked((byte) (-1)), unchecked((byte) (-1))
            };
            _input.Init(bytes1, 0);
            var theZeroLenghtArray = _input.ReadFloatArray();
            Assert.AreEqual(new float[0], theZeroLenghtArray);
        }

        [Test]
        public virtual void TestReadFloatByteOrder()
        {
            double readFloat = _input.ReadFloat(ByteOrder.LittleEndian);
            var intB = Bits.ReadIntL(InitData, 0);
            double aFloat = BitConverter.ToSingle(BitConverter.GetBytes(intB), 0);
            Assert.AreEqual(aFloat, readFloat, 0);
        }

        [Test]
        public virtual void TestReadFloatForPositionByteOrder()
        {
            double readFloat = _input.ReadFloat(2, ByteOrder.LittleEndian);
            var intB = Bits.ReadIntL(InitData, 2);
            double aFloat = BitConverter.ToSingle(BitConverter.GetBytes(intB), 0);
            Assert.AreEqual(aFloat, readFloat, 0);
        }

        [Test]
        public virtual void TestReadFloatPosition()
        {
            double readFloat = _input.ReadFloat(2);
            var intB = Bits.ReadIntB(InitData, 2);
            double aFloat = BitConverter.ToSingle(BitConverter.GetBytes(intB), 0);
            Assert.AreEqual(aFloat, readFloat, 0);
        }

        [Test]
        public virtual void TestReadForBOffLen()
        {
            var read = _input.Read(InitData, 0, 5);
            Assert.AreEqual(5, read);
        }

        public virtual void TestReadForBOffLen_negativeLen()
        {
            _input.Read(InitData, 0, -11);
        }

        public virtual void TestReadForBOffLen_negativeOffset()
        {
            _input.Read(InitData, -10, 1);
        }

        public virtual void TestReadForBOffLen_null_array()
        {
            _input.Read(null, 0, 1);
        }

        [Test]
        public virtual void TestReadForBOffLen_pos_gt_size()
        {
            _input.pos = 100;
            var read = _input.Read(InitData, 0, 1);
            Assert.AreEqual(-1, read);
        }

        [Test]
        public virtual void TestReadFullyB()
        {
            var readFull = new byte[InitData.Length];
            _input.ReadFully(readFull);
            Assert.AreEqual(readFull, _input.data);
        }

        public virtual void TestReadFullyB_EOF()
        {
            _input.Position(InitData.Length);
            var readFull = new byte[InitData.Length];
            _input.ReadFully(readFull);
        }

        [Test]
        public virtual void TestReadFullyForBOffLen()
        {
            var readFull = new byte[10];
            _input.ReadFully(readFull, 0, 5);
            for (var i = 0; i < 5; i++)
            {
                Assert.AreEqual(readFull[i], _input.data[i]);
            }
        }

        public virtual void TestReadFullyForBOffLen_EOF()
        {
            _input.Position(InitData.Length);
            var readFull = new byte[InitData.Length];
            _input.ReadFully(readFull, 0, readFull.Length);
        }

        [Test]
        public virtual void TestReadInt()
        {
            var readInt = _input.ReadInt();
            var theInt = Bits.ReadIntB(InitData, 0);
            Assert.AreEqual(theInt, readInt);
        }

        [Test]
        public virtual void TestReadIntArray()
        {
            byte[] bytes1 =
            {
                0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, unchecked((byte) (-1)), unchecked((byte) (-1)),
                unchecked((byte) (-1)), unchecked((byte) (-1))
            };
            _input.Init(bytes1, 0);
            _input.Position(12);
            var theNullArray = _input.ReadIntArray();
            _input.Position(0);
            var theZeroLenghtArray = _input.ReadIntArray();
            _input.Position(4);
            var bytes = _input.ReadIntArray();
            Assert.IsNull(theNullArray);
            Assert.AreEqual(new int[0], theZeroLenghtArray);
            Assert.AreEqual(new[] {1}, bytes);
        }

        [Test]
        public virtual void TestReadIntByteOrder()
        {
            var readInt = _input.ReadInt(ByteOrder.LittleEndian);
            var theInt = Bits.ReadIntL(InitData, 0);
            Assert.AreEqual(theInt, readInt);
        }

        [Test]
        public virtual void TestReadIntForPositionByteOrder()
        {
            var readInt = _input.ReadInt(3, ByteOrder.LittleEndian);
            var theInt = Bits.ReadIntL(InitData, 3);
            Assert.AreEqual(theInt, readInt);
        }

        [Test]
        public virtual void TestReadIntPosition()
        {
            var readInt = _input.ReadInt(2);
            var theInt = Bits.ReadIntB(InitData, 2);
            Assert.AreEqual(theInt, readInt);
        }

        public virtual void TestReadLine()
        {
            _input.ReadLine();
        }

        [Test]
        public virtual void TestReadLong()
        {
            var readLong = _input.ReadLong();
            var longB = Bits.ReadLongB(InitData, 0);
            Assert.AreEqual(longB, readLong);
        }

        [Test]
        public virtual void TestReadLongArray()
        {
            byte[] bytes1 =
            {
                0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, unchecked((byte) (-1)),
                unchecked((byte) (-1)), unchecked((byte) (-1)), unchecked((byte) (-1))
            };
            _input.Init(bytes1, 0);
            _input.Position(bytes1.Length - 4);
            var theNullArray = _input.ReadLongArray();
            _input.Position(0);
            var theZeroLenghtArray = _input.ReadLongArray();
            _input.Position(4);
            var bytes = _input.ReadLongArray();
            Assert.IsNull(theNullArray);
            Assert.AreEqual(new long[0], theZeroLenghtArray);
            Assert.AreEqual(new long[] {1}, bytes);
        }

        [Test]
        public virtual void TestReadLongByteOrder()
        {
            var readLong = _input.ReadLong(ByteOrder.LittleEndian);
            var longB = Bits.ReadLongL(InitData, 0);
            Assert.AreEqual(longB, readLong);
        }

        [Test]
        public virtual void TestReadLongForPositionByteOrder()
        {
            var readLong = _input.ReadLong(2, ByteOrder.LittleEndian);
            var longB = Bits.ReadLongL(InitData, 2);
            Assert.AreEqual(longB, readLong);
        }

        [Test]
        public virtual void TestReadLongPosition()
        {
            var readLong = _input.ReadLong(2);
            var longB = Bits.ReadLongB(InitData, 2);
            Assert.AreEqual(longB, readLong);
        }

        [Test]
        public virtual void TestReadPosition()
        {
            var read = _input.Read(1);
            Assert.AreEqual(1, read);
        }

        [Test]
        public virtual void TestReadShort()
        {
            var read = _input.ReadShort();
            var val = Bits.ReadShortB(InitData, 0);
            Assert.AreEqual(val, read);
        }

        [Test]
        public virtual void TestReadShortArray()
        {
            byte[] bytes1 =
            {
                0, 0, 0, 0, 0, 0, 0, 1, 0, 1, unchecked((byte) (-1)), unchecked((byte) (-1)),
                unchecked((byte) (-1)), unchecked((byte) (-1))
            };
            _input.Init(bytes1, 0);
            _input.Position(10);
            var theNullArray = _input.ReadShortArray();
            _input.Position(0);
            var theZeroLenghtArray = _input.ReadShortArray();
            _input.Position(4);
            var booleanArray = _input.ReadShortArray();
            Assert.IsNull(theNullArray);
            Assert.AreEqual(new short[0], theZeroLenghtArray);
            Assert.AreEqual(new short[] {1}, booleanArray);
        }

        [Test]
        public virtual void TestReadShortByteOrder()
        {
            var read = _input.ReadShort(ByteOrder.LittleEndian);
            var val = Bits.ReadShortL(InitData, 0);
            Assert.AreEqual(val, read);
        }

        [Test]
        public virtual void TestReadShortForPositionByteOrder()
        {
            var read = _input.ReadShort(1, ByteOrder.LittleEndian);
            var val = Bits.ReadShortL(InitData, 1);
            Assert.AreEqual(val, read);
        }

        [Test]
        public virtual void TestReadShortPosition()
        {
            var read = _input.ReadShort(1);
            var val = Bits.ReadShortB(InitData, 1);
            Assert.AreEqual(val, read);
        }

        [Test]
        public virtual void TestReadUnsignedByte()
        {
            byte[] bytes1 =
            {
                0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, unchecked((byte) (-1)),
                unchecked((byte) (-1)), unchecked((byte) (-1)), unchecked((byte) (-1))
            };
            _input.Init(bytes1, bytes1.Length - 4);
            var unsigned = _input.ReadUnsignedByte();
            Assert.AreEqual(unchecked(0xFF), unsigned);
        }

        [Test]
        public virtual void TestReadUnsignedShort()
        {
            byte[] bytes1 =
            {
                0, 0, 0, 0, 0, 0, 0, 8, 0, 0, 0, 0, 0, 0, 0, 1, unchecked((byte) (-1)),
                unchecked((byte) (-1)), unchecked((byte) (-1)), unchecked((byte) (-1))
            };
            _input.Init(bytes1, bytes1.Length - 4);
            var unsigned = _input.ReadUnsignedShort();
            Assert.AreEqual(unchecked(0xFFFF), unsigned);
        }

        [Test]
        public virtual void TestReadUTF()
        {
        }

        [Test]
        public virtual void TestReadUTFArray()
        {
            byte[] bytes1 =
            {
                0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, unchecked((byte) (-1)),
                unchecked((byte) (-1)), unchecked((byte) (-1)), unchecked((byte) (-1))
            };
            _input.Init(bytes1, 0);
            var theZeroLenghtArray = _input.ReadUTFArray();
            Assert.AreEqual(new string[0], theZeroLenghtArray);
        }

        [Test]
        public virtual void TestReset()
        {
            _input.Position(1);
            _input.Mark(1);
            _input.Reset();
            Assert.AreEqual(1, _input.pos);
        }

        [Test]
        public virtual void TestSkip()
        {
            var s1 = _input.Skip(-1);
            var s2 = _input.Skip(int.MaxValue);
            var s3 = _input.Skip(1);
            Assert.AreEqual(0, s1);
            Assert.AreEqual(0, s2);
            Assert.AreEqual(1, s3);
        }

        [Test]
        public virtual void TestSkipBytes()
        {
            long s1 = _input.SkipBytes(-1);
            long s3 = _input.SkipBytes(1);
            Assert.AreEqual(0, s1);
            Assert.AreEqual(1, s3);
        }

        [Test]
        public virtual void TestToString()
        {
            Assert.IsNotNull(_input.ToString());
        }
    }
}