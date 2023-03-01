// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
    public class ObjectDataOutputTest
    {
        private static readonly byte[] TestData = {1, 2, 3};
        private ObjectDataOutput _output;

        [TearDown]
        public virtual void After()
        {
            _output.Dispose();
        }

        [SetUp]
        public virtual void Before()
        {
            _output = new ObjectDataOutput(10, null, Endianness.BigEndian);
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
            _output.EnsureAvailable(10*10);
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
            _output.Buffer = null;
            _output.EnsureAvailable(5);
            Assert.AreEqual(10, _output.Buffer.Length);
        }

        [Test]
        public virtual void TestEnsureAvailable_smallLen()
        {
            _output.Buffer = null;
            _output.EnsureAvailable(1);
            Assert.AreEqual(10, _output.Buffer.Length);
        }

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
        public virtual void TestWriteFloatV()
        {
            var v = 1.1f;
            _output.WriteFloat(v);
            var expected = BitConverter.ToInt32(BitConverter.GetBytes(v), 0);
            var actual = BytesExtensions.ReadInt(_output.Buffer, 0, Endianness.BigEndian);
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public virtual void TestWriteForBOffLen()
        {
            var zeroBytes = new byte[20];
            _output.Write(zeroBytes, 0, 20);
            var bytes = new byte[20];
            Array.Copy(_output.Buffer, 0, bytes, 0, 20);
            Assert.AreEqual(zeroBytes, bytes);
            Assert.AreEqual(20, _output.Position);
        }

        [Test]
        public virtual void TestWriteForNullByteArray()
        {
            Assert.Throws<ArgumentNullException>(() => { _output.Write((byte[]) null, 0, 1); });
            Assert.Throws<ArgumentNullException>(() => { _output.Write((sbyte[])null, 0, 1); });
        }

        [Test]
        public virtual void TestWriteForBOffLen_negativeLen()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => { _output.Write(TestData, 0, -3); });
        }

        [Test]
        public virtual void TestWriteForBOffLen_negativeOff()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => { _output.Write(TestData, -1, 3); });
        }

        [Test]
        public virtual void TestWriteForBOffLen_OffLenHigherThenSize()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => { _output.Write(TestData, 0, -3); });
        }

        [Test]
        public virtual void TestWriteIntForPositionV()
        {
            var expected = 100;
            _output.WriteInt(1, expected);
            var actual = BytesExtensions.ReadInt(_output.Buffer, 1, Endianness.BigEndian);
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public virtual void TestWriteIntBigEndian()
        {
            var expected = 100;
            _output.WriteIntBigEndian(expected);
            var actual = BytesExtensions.ReadInt(_output.Buffer, 0);
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public virtual void TestWriteIntV()
        {
            var expected = 100;
            _output.WriteInt(expected);
            var actual = BytesExtensions.ReadInt(_output.Buffer, 0, Endianness.BigEndian);
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public virtual void TestWriteLongV()
        {
            long expected = 100;
            _output.WriteLong(expected);
            var actual = BytesExtensions.ReadLong(_output.Buffer, 0, Endianness.BigEndian);
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public virtual void TestWriteShortV()
        {
            short expected = 100;
            _output.WriteShort(expected);
            var actual = BytesExtensions.ReadShort(_output.Buffer, 0, Endianness.BigEndian);
            Assert.AreEqual(actual, expected);
        }
    }
}
