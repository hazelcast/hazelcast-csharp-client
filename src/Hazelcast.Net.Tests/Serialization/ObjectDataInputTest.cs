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
    public class ObjectDataInputTest
    {
        private static readonly byte[] InitData = {0, 1, 2, 3, 4, 5, 6, 7, 8, 9};
        private ObjectDataInput _input;

        [TearDown]
        public virtual void After()
        {
            _input.Dispose();
        }

        [SetUp]
        public virtual void Before()
        {
            _input = new ObjectDataInput(InitData, null, Endianness.BigEndian);
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
            _input.Dispose();
            Assert.IsNull(_input.Buffer);
        }

        [Test]
        public virtual void TestPosition()
        {
            Assert.AreEqual(0, _input.Position);
        }

        [Test]
        public virtual void TestPositionNewPos()
        {
            _input.Position = InitData.Length - 1;
            Assert.AreEqual(InitData.Length - 1, _input.Position);
        }

        public virtual void TestPositionNewPos_HighNewPos()
        {
            _input.Position = InitData.Length + 10;
        }

        public virtual void TestPositionNewPos_negativeNewPos()
        {
            _input.Position = -1;
        }

        [Test]
        public virtual void TestReadBool()
        {
            var read1 = _input.ReadBoolean();
            var read2 = _input.ReadBoolean();
            Assert.IsFalse(read1);
            Assert.IsTrue(read2);
        }

        public virtual void TestReadBool_EOF()
        {
            _input.Position = InitData.Length + 1;
            _input.ReadBoolean();
        }

        [Test]
        public virtual void TestReadBoolArray()
        {
            byte[] bytes1 =
            {
                0, 0, 0, 0, 0, 0, 0, 1, 8, 9, unchecked((byte) (-1)), unchecked((byte) (-1)),
                unchecked((byte) (-1)), unchecked((byte) (-1))
            };
            _input.Initialize(bytes1, 0);
            _input.Position = 10;
            var theNullArray = _input.ReadBooleanArray();
            _input.Position = 0;
            var theZeroLenghtArray = _input.ReadBooleanArray();
            _input.Position = 4;
            var booleanArray = _input.ReadBooleanArray();
            Assert.IsNull(theNullArray);
            Assert.AreEqual(new bool[0], theZeroLenghtArray);
            Assert.AreEqual(new[] {true}, booleanArray);
        }

        [Test]
        public virtual void TestReadBoolPosition()
        {
            var read1 = _input.ReadBoolean(0);
            var read2 = _input.ReadBoolean(1);
            Assert.IsFalse(read1);
            Assert.IsTrue(read2);
        }

        public virtual void TestReadBoolPosition_EOF()
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
            _input.Position = InitData.Length + 1;
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
            _input.Initialize(bytes1, 0);
            _input.Position = 10;
            var theNullArray = _input.ReadByteArray();
            _input.Position = 0;
            var theZeroLenghtArray = _input.ReadByteArray();
            _input.Position = 4;
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
            _input.Initialize(bytes1, 0);
            _input.Position = 10;
            var theNullArray = _input.ReadCharArray();
            _input.Position = 0;
            var theZeroLenghtArray = _input.ReadCharArray();
            _input.Position = 4;
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
        public virtual void TestReadDouble()
        {
            var readDouble = _input.ReadDouble();
            var longB = BytesExtensions.ReadLong(InitData, 0, Endianness.BigEndian);
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
            _input.Initialize(bytes1, 0);
            var theZeroLenghtArray = _input.ReadDoubleArray();
            Assert.AreEqual(new double[0], theZeroLenghtArray);
        }
        
        [Test]
        public virtual void TestReadDoublePosition()
        {
            var readDouble = _input.ReadDouble(2);
            var longB = BytesExtensions.ReadLong(InitData, 2, Endianness.BigEndian);
            var aDouble = BitConverter.Int64BitsToDouble(longB);
            Assert.AreEqual(aDouble, readDouble, 0);
        }

        [Test]
        public virtual void TestReadFloat()
        {
            // expected 9.2557164867118492E-41 which is what the original code reads
            // now we read 66051.0d? -- but ReadInt *does* return 66051 on original code, how can that become? meh?

            // so our reading of the int is ok, our conversion to aFloat is ok
            // but our reading (and writing?) of a float is not?

            double readFloat = _input.ReadFloat();
            var intB = BytesExtensions.ReadInt(InitData, 0, Endianness.BigEndian);
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
            _input.Initialize(bytes1, 0);
            var theZeroLenghtArray = _input.ReadFloatArray();
            Assert.AreEqual(new float[0], theZeroLenghtArray);
        }

        [Test]
        public virtual void TestReadFloatPosition()
        {
            double readFloat = _input.ReadFloat(2);
            var intB = BytesExtensions.ReadInt(InitData, 2, Endianness.BigEndian);
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
            var read = _input.Read(InitData, 0, 10);
            Assert.AreEqual(10, read);

            read = _input.Read(InitData, 0, 1);

            Assert.AreEqual(-1, read);
        }

        [Test]
        public virtual void TestReadBytesForBOffLen()
        {
            var readFull = new byte[10];
            _input.Read(readFull, 0, 5);
            for (var i = 0; i < 5; i++)
            {
                Assert.AreEqual(readFull[i], _input.Buffer[i]);
            }
        }

        [Test]
        public virtual void TestReadInt()
        {
            var readInt = _input.ReadInt();
            var theInt = BytesExtensions.ReadInt(InitData, 0, Endianness.BigEndian);
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
            _input.Initialize(bytes1, 0);
            _input.Position = 12;
            var theNullArray = _input.ReadIntArray();
            _input.Position = 0;
            var theZeroLenghtArray = _input.ReadIntArray();
            _input.Position = 4;
            var bytes = _input.ReadIntArray();
            Assert.IsNull(theNullArray);
            Assert.AreEqual(new int[0], theZeroLenghtArray);
            Assert.AreEqual(new[] {1}, bytes);
        }
        
        [Test]
        public virtual void TestReadIntPosition()
        {
            var readInt = _input.ReadInt(2);
            var theInt = BytesExtensions.ReadInt(InitData, 2, Endianness.BigEndian);
            Assert.AreEqual(theInt, readInt);
        }

        [Test]
        public virtual void TestReadLong()
        {
            var readLong = _input.ReadLong();
            var longB = BytesExtensions.ReadLong(InitData, 0, Endianness.BigEndian);
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
            _input.Initialize(bytes1, 0);
            _input.Position = bytes1.Length - 4;
            var theNullArray = _input.ReadLongArray();
            _input.Position = 0;
            var theZeroLenghtArray = _input.ReadLongArray();
            _input.Position = 4;
            var bytes = _input.ReadLongArray();
            Assert.IsNull(theNullArray);
            Assert.AreEqual(new long[0], theZeroLenghtArray);
            Assert.AreEqual(new long[] {1}, bytes);
        }

        [Test]
        public virtual void TestReadLongPosition()
        {
            var readLong = _input.ReadLong(2);
            var longB = BytesExtensions.ReadLong(InitData, 2, Endianness.BigEndian);
            Assert.AreEqual(longB, readLong);
        }

        [Test]
        public virtual void TestReadShort()
        {
            var read = _input.ReadShort();
            var val = BytesExtensions.ReadShort(InitData, 0, Endianness.BigEndian);
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
            _input.Initialize(bytes1, 0);
            _input.Position = 10;
            var theNullArray = _input.ReadShortArray();
            _input.Position = 0;
            var theZeroLenghtArray = _input.ReadShortArray();
            _input.Position = 4;
            var booleanArray = _input.ReadShortArray();
            Assert.IsNull(theNullArray);
            Assert.AreEqual(new short[0], theZeroLenghtArray);
            Assert.AreEqual(new short[] {1}, booleanArray);
        }

        [Test]
        public virtual void TestReadShortPosition()
        {
            var read = _input.ReadShort(1);
            var val = BytesExtensions.ReadShort(InitData, 1, Endianness.BigEndian);
            Assert.AreEqual(val, read);
        }

        [Test]
        public virtual void TestReadUnsignedByte()
        {
            // csharp bytes are always unsigned? see sbyte?

            byte[] bytes1 =
            {
                0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, unchecked((byte) (-1)),
                unchecked((byte) (-1)), unchecked((byte) (-1)), unchecked((byte) (-1))
            };
            _input.Initialize(bytes1, bytes1.Length - 4);
            var unsigned = _input.ReadByte();
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
            _input.Initialize(bytes1, bytes1.Length - 4);
            var unsigned = _input.ReadUshort();
            Assert.AreEqual(unchecked(0xFFFF), unsigned);
        }

        public void TestReadUTF()
        {
            //EXTENDED TEST ELSEWHERE: StringSerializationTest
        }

        [Test]
        public virtual void TestReadUTFArray()
        {
            byte[] bytes1 =
            {
                0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, unchecked((byte) (-1)),
                unchecked((byte) (-1)), unchecked((byte) (-1)), unchecked((byte) (-1))
            };
            _input.Initialize(bytes1, 0);
            var theZeroLenghtArray = _input.ReadUTF();
            Assert.AreEqual(new string[0], theZeroLenghtArray);
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
