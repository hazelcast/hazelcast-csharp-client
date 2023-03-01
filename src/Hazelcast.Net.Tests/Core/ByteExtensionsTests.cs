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
using System.Buffers;
using System.Linq;
using System.Text;
using Hazelcast.Core;
using Hazelcast.Models;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.Core
{
    [TestFixture]
    public class ByteExtensionsTests
    {
        // endianness is, by default, unspecified and then falls back to big endian

        [Test]
        public void WriteBits()
        {
            var bytes = new byte[8];

            bytes.WriteBits(2, 0b_0000_0000, 0b_0000_0000);
            AssertBytes(bytes, 0, 0, 0b_0000_0000, 0, 0, 0, 0, 0);

            bytes.WriteBits(2, 0b_1111_1111, 0b_0000_0000);
            AssertBytes(bytes, 0, 0, 0b_0000_0000, 0, 0, 0, 0, 0);

            bytes.WriteBits(2, 0b_1111_1111, 0b_1111_1111);
            AssertBytes(bytes, 0, 0, 0b_1111_1111, 0, 0, 0, 0, 0);

            bytes.WriteBits(2, 0b_0000_0000, 0b_0000_0000);
            AssertBytes(bytes, 0, 0, 0b_1111_1111, 0, 0, 0, 0, 0);

            bytes.WriteBits(2, 0b_0000_0000, 0b_1111_1111);
            AssertBytes(bytes, 0, 0, 0b_0000_0000, 0, 0, 0, 0, 0);

            bytes.WriteBits(4, 0b_0000_0110, 0b_0000_0010);
            AssertBytes(bytes, 0, 0, 0, 0, 0b_0000_0010, 0, 0, 0);

            bytes.WriteBits(4, 0b_0000_0100, 0b_0000_0110);
            AssertBytes(bytes, 0, 0, 0, 0, 0b_0000_0100, 0, 0, 0);
        }

        [Test]
        public void WriteByte()
        {
            var bytes = new byte[8];

            bytes.WriteByte(2, 42);
            AssertBytes(bytes, 0, 0, 42, 0, 0, 0, 0, 0);

            bytes.WriteByteL(2, 42);
            AssertBytes(bytes, 0, 0, 42, 0, 0, 0, 0, 0);
        }

        [Test]
        public void ReadByte()
        {
            var bytes = new byte[] { 0, 0, 42 };

            Assert.That(bytes.ReadByte(2), Is.EqualTo(42));
            Assert.That(bytes.ReadByteL(2), Is.EqualTo(42));
        }

        [Test]
        public void WriteBool()
        {
            var bytes = new byte[8];
            bytes.WriteBool(2, true);
            AssertBytes(bytes, 0, 0, 1, 0, 0, 0, 0, 0);

            bytes.WriteBool(2, false);
            AssertBytes(bytes, 0, 0, 0, 0, 0, 0, 0, 0);

            bytes.WriteBoolL(2, true);
            AssertBytes(bytes, 0, 0, 1, 0, 0, 0, 0, 0);

            bytes.WriteBoolL(2, false);
            AssertBytes(bytes, 0, 0, 0, 0, 0, 0, 0, 0);
        }

        [Test]
        public void ReadBool()
        {
            var bytes = new byte[] { 0, 0, 1 };

            Assert.That(bytes.ReadBool(2), Is.True);
            Assert.That(bytes.ReadBoolL(2), Is.True);

            bytes[2] = 0;
            Assert.That(bytes.ReadBool(2), Is.False);
            Assert.That(bytes.ReadBoolL(2), Is.False);
        }

        [Test]
        public void WriteChar()
        {
            var bytes = new byte[8];

            bytes.WriteChar(2, 'x', Endianness.BigEndian);
            AssertBytes(bytes, 0, 0, 0, 120, 0, 0, 0, 0);

            bytes.WriteChar(2, 'x', Endianness.LittleEndian);
            AssertBytes(bytes, 0, 0, 120, 0, 0, 0, 0, 0);

            bytes.WriteChar(2, '❤', Endianness.BigEndian);
            AssertBytes(bytes, 0, 0, 39, 100, 0, 0, 0, 0);

            bytes.WriteChar(2, '❤', Endianness.LittleEndian);
            AssertBytes(bytes, 0, 0, 100, 39, 0, 0, 0, 0);
        }

        [Test]
        public void ReadChar()
        {
            var bytes = new byte[] { 0, 0, 0, 120, 0 };

            Assert.That(bytes.ReadChar(2, Endianness.BigEndian), Is.EqualTo('x'));

            bytes = new byte[] { 0, 0, 120, 0 };
            Assert.That(bytes.ReadChar(2, Endianness.LittleEndian), Is.EqualTo('x'));

            bytes = new byte[] { 0, 0, 39, 100 };
            Assert.That(bytes.ReadChar(2, Endianness.BigEndian), Is.EqualTo('❤'));

            bytes = new byte[] { 0, 0, 100, 39 };
            Assert.That(bytes.ReadChar(2, Endianness.LittleEndian), Is.EqualTo('❤'));
        }

        [Test]
        public void WriteInt()
        {
            var bytes = new byte[8];

            bytes.WriteInt(2, 123456789, Endianness.BigEndian);
            AssertBytes(bytes, 0, 0, 7, 91, 205, 21, 0);

            bytes.WriteInt(2, 123456789, Endianness.LittleEndian);
            AssertBytes(bytes, 0, 0, 21, 205, 91, 7, 0);

            bytes.WriteIntL(2, 123456789);
            AssertBytes(bytes, 0, 0, 21, 205, 91, 7, 0);
        }

        [Test]
        public void ReadInt()
        {
            var bytes = new byte[] { 0, 0, 7, 91, 205, 21, 0 };

            Assert.That(bytes.ReadInt(2, Endianness.BigEndian), Is.EqualTo(123456789));

            bytes = new byte[] { 0, 0, 21, 205, 91, 7, 0 };
            Assert.That(bytes.ReadInt(2, Endianness.LittleEndian), Is.EqualTo(123456789));
            Assert.That(bytes.ReadIntL(2), Is.EqualTo(123456789));
        }

        [Test]
        public void ReadIntSequence()
        {
            var bytes = new byte[] { 0 };
            var sequence = new ReadOnlySequence<byte>(bytes);

            bytes = new byte[] { 7, 91, 205, 21, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            sequence = new ReadOnlySequence<byte>(bytes);
            Assert.That(BytesExtensions.ReadInt(ref sequence, Endianness.BigEndian), Is.EqualTo(123456789));
            Assert.That(sequence.Length, Is.EqualTo(12));

            bytes = new byte[] { 21, 205, 91, 7, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            sequence = new ReadOnlySequence<byte>(bytes);
            Assert.That(BytesExtensions.ReadInt(ref sequence, Endianness.LittleEndian), Is.EqualTo(123456789));
            Assert.That(sequence.Length, Is.EqualTo(12));

            var bytes1 = new byte[] { 7, 91 };
            var bytes2 = new byte[] { 205, 21, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            var firstSegment = new MemorySegment<byte>(bytes1);
            var lastSegment = firstSegment.Append(bytes2);

            sequence = new ReadOnlySequence<byte>(firstSegment, 0, lastSegment, lastSegment.Memory.Length);
            Assert.That(BytesExtensions.ReadInt(ref sequence, Endianness.BigEndian), Is.EqualTo(123456789));
            Assert.That(sequence.Length, Is.EqualTo(12));
        }

        [Test]
        public void ReadIntSpan()
        {
            var bytes = new byte[] { 0 };
            var span = new ReadOnlySpan<byte>(bytes);

            try
            {
                _ = span.ReadInt(Endianness.BigEndian);
                Assert.Fail("Expected an exception.");
            }
            catch (ArgumentException) { /* expected */ }

            bytes = new byte[] { 7, 91, 205, 21, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            span = new ReadOnlySpan<byte>(bytes);
            Assert.That(span.ReadInt(Endianness.BigEndian), Is.EqualTo(123456789));

            bytes = new byte[] { 21, 205, 91, 7, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            span = new ReadOnlySpan<byte>(bytes);
            Assert.That(span.ReadInt(Endianness.LittleEndian), Is.EqualTo(123456789));
        }

        [Test]
        public void WriteIntLEnum()
        {
            var bytes = new byte[8];

            bytes.WriteIntL(2, SomeEnum.Value1);
            AssertBytes(bytes, 0, 0, 21, 205, 91, 7, 0);
        }

        [Test]
        public void WriteShort()
        {
            var bytes = new byte[8];

            bytes.WriteShort(2, 12345, Endianness.BigEndian);
            AssertBytes(bytes, 0, 0, 48, 57, 0, 0, 0);

            bytes.WriteShort(2, 12345, Endianness.LittleEndian);
            AssertBytes(bytes, 0, 0, 57, 48, 0, 0, 0);
        }

        [Test]
        public void ReadShort()
        {
            var bytes = new byte[] { 0, 0, 48, 57, 0, 0, 0 };

            Assert.That(bytes.ReadShort(2, Endianness.BigEndian), Is.EqualTo(12345));

            bytes = new byte[] { 0, 0, 57, 48, 0, 0, 0 };
            Assert.That(bytes.ReadShort(2, Endianness.LittleEndian), Is.EqualTo(12345));
        }

        [Test]
        public void ReadShortSpan()
        {
            var bytes = new byte[] { 0 };
            var span = new ReadOnlySpan<byte>(bytes);

            try
            {
                _ = span.ReadShort(Endianness.BigEndian);
                Assert.Fail("Expected an exception.");
            }
            catch (ArgumentException) { /* expected */ }

            bytes = new byte[] { 48, 57, 0, 0, 0 };
            span = new ReadOnlySpan<byte>(bytes);
            Assert.That(span.ReadShort(Endianness.BigEndian), Is.EqualTo(12345));

            bytes = new byte[] { 57, 48, 0, 0, 0 };
            span = new ReadOnlySpan<byte>(bytes);
            Assert.That(span.ReadShort(Endianness.LittleEndian), Is.EqualTo(12345));
        }

        [Test]
        public void WriteUShort()
        {
            var bytes = new byte[8];

            bytes.WriteUShort(2, 12345, Endianness.BigEndian);
            AssertBytes(bytes, 0, 0, 48, 57, 0, 0, 0);

            bytes.WriteUShort(2, 12345, Endianness.LittleEndian);
            AssertBytes(bytes, 0, 0, 57, 48, 0, 0, 0);
        }

        [Test]
        public void ReadUShort()
        {
            var bytes = new byte[] { 0, 0, 48, 57, 0, 0, 0 };

            Assert.That(bytes.ReadUShort(2, Endianness.BigEndian), Is.EqualTo(12345));

            bytes = new byte[] { 0, 0, 57, 48, 0, 0, 0 };
            Assert.That(bytes.ReadUShort(2, Endianness.LittleEndian), Is.EqualTo(12345));
        }

        [Test]
        public void ReadUShortSequence()
        {
            var bytes = new byte[] { 0 };
            var sequence = new ReadOnlySequence<byte>(bytes);

            Assert.Throws<ArgumentException>(() => _ = BytesExtensions.ReadUShort(ref sequence, Endianness.BigEndian));

            bytes = new byte[] { 48, 57, 0, 0, 0 };
            sequence = new ReadOnlySequence<byte>(bytes);
            Assert.That(BytesExtensions.ReadUShort(ref sequence, Endianness.BigEndian), Is.EqualTo(12345));
            Assert.That(sequence.Length, Is.EqualTo(3));

            bytes = new byte[] { 57, 48, 0, 0, 0 };
            sequence = new ReadOnlySequence<byte>(bytes);
            Assert.That(BytesExtensions.ReadUShort(ref sequence, Endianness.LittleEndian), Is.EqualTo(12345));
            Assert.That(sequence.Length, Is.EqualTo(3));

            var bytes1 = new byte[] { 48 };
            var bytes2 = new byte[] { 57, 0, 0, 0 };
            var firstSegment = new MemorySegment<byte>(bytes1);
            var lastSegment = firstSegment.Append(bytes2);

            sequence = new ReadOnlySequence<byte>(firstSegment, 0, lastSegment, lastSegment.Memory.Length);
            Assert.That(BytesExtensions.ReadUShort(ref sequence, Endianness.BigEndian), Is.EqualTo(12345));
            Assert.That(sequence.Length, Is.EqualTo(3));
        }

        [Test]
        public void ReadUShortSpan()
        {
            var bytes = new byte[] { 0 };
            var span = new ReadOnlySpan<byte>(bytes);

            try
            {
                _ = span.ReadUShort(Endianness.BigEndian);
                Assert.Fail("Expected an exception.");
            }
            catch (ArgumentException) { /* expected */ }

            bytes = new byte[] { 48, 57, 0, 0, 0 };
            span = new ReadOnlySpan<byte>(bytes);
            Assert.That(span.ReadUShort(Endianness.BigEndian), Is.EqualTo(12345));

            bytes = new byte[] { 57, 48, 0, 0, 0 };
            span = new ReadOnlySpan<byte>(bytes);
            Assert.That(span.ReadUShort(Endianness.LittleEndian), Is.EqualTo(12345));
        }

        [Test]
        public void WriteLong()
        {
            var bytes = new byte[16];

            bytes.WriteLong(2, (long)int.MaxValue + 123456, Endianness.BigEndian);
            AssertBytes(bytes, 0, 0, 0, 0, 0, 0, 128, 1, 226, 63, 0, 0, 0, 0, 0, 0);

            bytes.WriteLong(2, (long)int.MaxValue + 123456, Endianness.LittleEndian);
            AssertBytes(bytes, 0, 0, 63, 226, 1, 128, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

            bytes.WriteLongL(2, (long)int.MaxValue + 123456);
            AssertBytes(bytes, 0, 0, 63, 226, 1, 128, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        }

        [Test]
        public void ReadLong()
        {
            var bytes = new byte[] { 0, 0, 0, 0, 0, 0, 128, 1, 226, 63, 0, 0, 0, 0, 0, 0 };
            Assert.That(bytes.ReadLong(2, Endianness.BigEndian), Is.EqualTo((long)int.MaxValue + 123456));

            bytes = new byte[] { 0, 0, 63, 226, 1, 128, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            Assert.That(bytes.ReadLong(2, Endianness.LittleEndian), Is.EqualTo((long)int.MaxValue + 123456));
            Assert.That(bytes.ReadLongL(2), Is.EqualTo((long)int.MaxValue + 123456));
        }

        [Test]
        public void WriteFloat()
        {
            var bytes = new byte[16];

            bytes.WriteFloat(2, (float)int.MaxValue + 123456, Endianness.BigEndian);
            AssertBytes(bytes, 0, 0, 79, 0, 1, 226, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

            bytes.WriteFloat(2, (float)int.MaxValue + 123456, Endianness.LittleEndian);
            AssertBytes(bytes, 0, 0, 226, 1, 0, 79, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        }

        [Test]
        public void ReadFloat()
        {
            var bytes = new byte[] { 0, 0, 79, 0, 1, 226, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            Assert.That(bytes.ReadFloat(2, Endianness.BigEndian), Is.EqualTo((float)int.MaxValue + 123456));

            bytes = new byte[] { 0, 0, 226, 1, 0, 79, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            Assert.That(bytes.ReadFloat(2, Endianness.LittleEndian), Is.EqualTo((float)int.MaxValue + 123456));
        }

        [Test]
        public void WriteDouble()
        {
            var bytes = new byte[16];
            bytes.WriteDouble(2, (double)int.MaxValue + 123456, Endianness.BigEndian);
            AssertBytes(bytes, 0, 0, 65, 224, 0, 60, 71, 224, 0, 0, 0, 0, 0, 0, 0, 0);

            bytes.WriteDouble(2, (double)int.MaxValue + 123456, Endianness.LittleEndian);
            AssertBytes(bytes, 0, 0, 0, 0, 224, 71, 60, 0, 224, 65, 0, 0, 0, 0, 0, 0);
        }

        [Test]
        public void ReadDouble()
        {
            var bytes = new byte[] { 0, 0, 65, 224, 0, 60, 71, 224, 0, 0, 0, 0, 0, 0, 0, 0 };
            Assert.That(bytes.ReadDouble(2, Endianness.BigEndian), Is.EqualTo((double)int.MaxValue + 123456));

            bytes = new byte[] { 0, 0, 0, 0, 224, 71, 60, 0, 224, 65, 0, 0, 0, 0, 0, 0 };
            Assert.That(bytes.ReadDouble(2, Endianness.LittleEndian), Is.EqualTo((double)int.MaxValue + 123456));
        }

        [Test]
        public void WriteEmptyGuid()
        {
            var bytes = new byte[24];
            for (var i = 0; i < bytes.Length; i++) bytes[i] = 0xff;
            var guid = Guid.Empty;
            bytes.WriteGuidL(2, guid);
            AssertBytes(bytes, 0xff, 0xff, 0x01, 0xff, 0xff);
        }

        [Test]
        public void ReadEmptyGuid()
        {
            var bytes = new byte[24];
            for (var i = 0; i < bytes.Length; i++) bytes[i] = 0xff;
            bytes[2] = 1;
            Assert.That(bytes.ReadGuidL(2), Is.EqualTo(Guid.Empty));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ReadWriteGuid(bool withEmptyFlag)
        {
            // we don't test the actual byte representation of the serialized guid here,
            // this is done in the GuidPolyglotTests as we need to ensure it conforms with Java

            var guid = Guid.NewGuid();
            var bytes = new byte[24];
            for (var i = 0; i < bytes.Length; i++) bytes[i] = 0xff;
            bytes.WriteGuidL(2, guid, withEmptyFlag);
            Assert.That(bytes.ReadGuidL(2, withEmptyFlag), Is.EqualTo(guid));
        }


        [Test]
        [TestCase(new byte[] { 0x00, 0xE5, 0x07, 0x00, 0x00, 0x07, 0x0B }, 1, "2021-07-11")]
        [TestCase(new byte[] { 0xFF, 0xE9, 0x03, 0x00, 0x00, 0x0C, 0x1F }, 1, "1001-12-31")]
        [TestCase(new byte[] { 0xFF, 0xC9, 0x9A, 0x3B, 0x01, 0x01 }, 0, "999999999-01-01")]
        [TestCase(new byte[] { 0x01, 0x36, 0x65, 0xC4, 0x01, 0x01 }, 0, "-999999999-01-01")]
        public void ReadLocalDate(byte[] bytes, int position, string expected)
        {
            Assert.AreEqual(HLocalDate.Parse(expected), bytes.ReadLocalDate(position));
        }

        [Test]
        [TestCase("2021-07-11", new byte[] { 0xE5, 0x07,0x00, 0x00, 0x07, 0x0B }, 1)]
        [TestCase("1001-12-31", new byte[] { 0xE9, 0x03,0x00, 0x00, 0x0C, 0x1F }, 1)]
        [TestCase("999999999-01-01", new byte[] { 0xFF, 0xC9, 0x9A, 0x3B, 0x01, 0x01 }, 0)]
        [TestCase("-999999999-01-01", new byte[] { 0x01, 0x36, 0x65, 0xC4, 0x01, 0x01 }, 0)]
        public void WriteLocalDate(string localDateStr, byte[] expected, int position)
        {
            var (localDate, bytes) = (HLocalDate.Parse(localDateStr), new byte[position + BytesExtensions.SizeOfLocalDate]);
            bytes.WriteLocalDate(position, localDate);
            CollectionAssert.AreEqual(expected, bytes.Skip(position));
        }

        [Test]
        [TestCase(new byte[] { 0x00, 0x0C, 0x11, 0x10, 0x00, 0x00, 0x00, 0x00 }, 1, "12:17:16")]
        [TestCase(new byte[] { 0xFF, 0x15, 0x3B, 0x38, 0x4E, 0x61, 0xBC, 0x00 }, 1, "21:59:56.012345678")]
        public void ReadLocalTime(byte[] bytes, int position, string expected)
        {
            Assert.AreEqual(HLocalTime.Parse(expected), bytes.ReadLocalTime(position));
        }

        [Test]
        [TestCase("12:17:16", new byte[] { 0x0C, 0x11, 0x10, 0x00, 0x00, 0x00, 0x00 }, 1)]
        [TestCase("21:59:56.012345678", new byte[] { 0x15, 0x3B, 0x38, 0x4E, 0x61, 0xBC, 0x00 }, 1)]
        public void WriteLocalTime(string localTimeStr, byte[] expected, int position)
        {
            var (localTime, bytes) = (HLocalTime.Parse(localTimeStr), new byte[position + BytesExtensions.SizeOfLocalTime]);
            bytes.WriteLocalTime(position, localTime);
            CollectionAssert.AreEqual(expected, bytes.Skip(position));
        }

        [Test]
        [TestCase(
            new byte[] { 0x00, 0xE5, 0x07, 0x00, 0x00, 0x07, 0x0B, 0x0C, 0x11, 0x10, 0x00, 0x00, 0x00, 0x00 },
            1, "2021-07-11T12:17:16"
        )]
        [TestCase(
            new byte[] { 0xFF, 0xE9, 0x03, 0x00, 0x00, 0x0C, 0x1F, 0x15, 0x3B, 0x38, 0x4E, 0x61, 0xBC, 0x00 },
            1, "1001-12-31T21:59:56.012345678"
        )]
        [TestCase(
            new byte[] { 0xFF, 0xC9, 0x9A, 0x3B, 0x01, 0x01, 0x17, 0x3B, 0x3B, 0xFF, 0xC9, 0x9A, 0x3B },
            0, "999999999-01-01T23:59:59.999999999"
        )]
        [TestCase(
            new byte[] { 0x01, 0x36, 0x65, 0xC4, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
            0, "-999999999-01-01T00:00:00"
        )]
        public void ReadLocalDateTime(byte[] bytes, int position, string expected)
        {
            Assert.AreEqual(HLocalDateTime.Parse(expected), bytes.ReadLocalDateTime(position));
        }

        [Test]
        [TestCase(
            "2021-07-11T12:17:16",
            new byte[] { 0xE5, 0x07, 0x00, 0x00, 0x07, 0x0B, 0x0C, 0x11, 0x10, 0x00, 0x00, 0x00, 0x00 }, 1
        )]
        [TestCase(
            "1001-12-31T21:59:56.012345678",
            new byte[] { 0xE9, 0x03, 0x00, 0x00, 0x0C, 0x1F, 0x15, 0x3B, 0x38, 0x4E, 0x61, 0xBC, 0x00 }, 1
        )]
        [TestCase(
            "999999999-01-01T23:59:59.999999999",
            new byte[] { 0xFF, 0xC9, 0x9A, 0x3B, 0x01, 0x01, 0x17, 0x3B, 0x3B, 0xFF, 0xC9, 0x9A, 0x3B }, 0
        )]
        [TestCase(
            "-999999999-01-01T00:00:00",
            new byte[] { 0x01, 0x36, 0x65, 0xC4, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, 0
        )]
        public void WriteLocalDateTime(string localDateTimeStr, byte[] expected, int position)
        {
            var (localDateTime, bytes) = (HLocalDateTime.Parse(localDateTimeStr), new byte[position + BytesExtensions.SizeOfLocalDateTime]);
            bytes.WriteLocalDateTime(position, localDateTime);
            CollectionAssert.AreEqual(expected, bytes.Skip(position));
        }

        [Test]
        [TestCase(
            new byte[] { 0x00, 0xE5, 0x07, 0x00, 0x00, 0x07, 0x0B, 0x0C, 0x11, 0x10, 0x00, 0x00, 0x00, 0x00, 0x30, 0x2A, 0x00, 0x00 },
            1, "2021-07-11T12:17:16+03:00"
        )]
        [TestCase(
            new byte[] { 0xFF, 0xE9, 0x03, 0x00, 0x00, 0x0C, 0x1F, 0x15, 0x3B, 0x38, 0x4E, 0x61, 0xBC, 0x00, 0x00, 0x00, 0x00, 0x00 },
            1, "1001-12-31T21:59:56.012345678Z"
        )]
        public void ReadOffsetDateTime(byte[] bytes, int position, string expected)
        {
            Assert.AreEqual(HOffsetDateTime.Parse(expected), bytes.ReadOffsetDateTime(position));
        }

        [Test]
        [TestCase(
            "2021-07-11T12:17:16+03:00",
            new byte[] { 0xE5, 0x07, 0x00, 0x00, 0x07, 0x0B, 0x0C, 0x11, 0x10, 0x00, 0x00, 0x00, 0x00, 0x30, 0x2A, 0x00, 0x00 }, 1
        )]
        [TestCase(
            "1001-12-31T21:59:56.012345678Z",
            new byte[] { 0xE9, 0x03, 0x00, 0x00, 0x0C, 0x1F, 0x15, 0x3B, 0x38, 0x4E, 0x61, 0xBC, 0x00, 0x00, 0x00, 0x00, 0x00 }, 1
        )]
        public void WriteOffsetDateTime(string offsetDateTimeStr, byte[] expected, int position)
        {
            var (offsetDateTime, bytes) = (HOffsetDateTime.Parse(offsetDateTimeStr), new byte[position + BytesExtensions.SizeOfOffsetDateTime]);
            bytes.WriteOffsetDateTime(position, offsetDateTime);
            CollectionAssert.AreEqual(expected, bytes.Skip(position));
        }

        [Test]
        public void ValidateUtf8()
        {
            // a string that contains 1, 2, 3 and 4 bytes chars
            var s = new string(new[] { Utf8Char.OneByte, Utf8Char.TwoBytes, Utf8Char.ThreeBytes }) + Utf8Char.FourBytes;

            // validate Encoding.UTF8 as a reference for Write/Read Utf8Char tests below
            var encodingBytes = Encoding.UTF8.GetBytes(s);
            AssertBytes(encodingBytes, 0x78, 0xc3, 0xa3, 0xe0, 0xa3, 0x9f, 0xf0, 0x9f, 0x98, 0x81);

            // and back
            Assert.That(Encoding.UTF8.GetString(encodingBytes), Is.EqualTo(s));
        }

        [Test]
        public void FillSpan()
        {
            var sequence = new ReadOnlySequence<byte>(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });
            var bytes = new byte[20];
            var span = new Span<byte>(bytes);

            try
            {
                sequence.Fill(span);
                Assert.Fail("Expected exception.");
            }
            catch (ArgumentOutOfRangeException) { /* expected */ }

            bytes = new byte[8];
            span = new Span<byte>(bytes);

            sequence.Fill(span);
            Assert.That(sequence.Length, Is.EqualTo(2));
            for (var i = 0; i < 8; i++) Assert.That(bytes[i], Is.EqualTo(i));

            var bytes1 = new byte[] { 0, 1, 2, 3, 4 };
            var bytes2 = new byte[] { 5, 6, 7, 8, 9 };
            var firstSegment = new MemorySegment<byte>(bytes1);
            var lastSegment = firstSegment.Append(bytes2);

            sequence = new ReadOnlySequence<byte>(firstSegment, 0, lastSegment, lastSegment.Memory.Length);

            sequence.Fill(span);
            Assert.That(sequence.Length, Is.EqualTo(2));
            for (var i = 0; i < 8; i++) Assert.That(bytes[i], Is.EqualTo(i));
        }

        [Test]
        public void FillSequence()
        {
            var sequence = new ReadOnlySequence<byte>(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });
            var bytes = new byte[20];
            var span = new Span<byte>(bytes);

            try
            {
                span.Fill(ref sequence);
                Assert.Fail("Expected exception.");
            }
            catch (ArgumentOutOfRangeException) { /* expected */ }

            bytes = new byte[8];
            span = new Span<byte>(bytes);

            span.Fill(ref sequence);
            Assert.That(sequence.Length, Is.EqualTo(2));
            for (var i = 0; i < 8; i++) Assert.That(bytes[i], Is.EqualTo(i));

            var bytes1 = new byte[] { 0, 1, 2, 3, 4 };
            var bytes2 = new byte[] { 5, 6, 7, 8, 9 };
            var firstSegment = new MemorySegment<byte>(bytes1);
            var lastSegment = firstSegment.Append(bytes2);

            sequence = new ReadOnlySequence<byte>(firstSegment, 0, lastSegment, lastSegment.Memory.Length);

            span.Fill(ref sequence);
            Assert.That(sequence.Length, Is.EqualTo(2));
            for (var i = 0; i < 8; i++) Assert.That(bytes[i], Is.EqualTo(i));
        }

        private static void AssertBytes(byte[] bytes, params byte[] values)
        {
            var equals = true;
            for (var i = 0; i < bytes.Length && i < values.Length; i++)
                equals &= bytes[i] == values[i];

            if (equals) return;

            var svalues = values.Select(x => x.ToString("x2"));
            var sbytes = bytes.Select(x => x.ToString("x2"));

            Assert.Fail($"Expected ({string.Join(" ", svalues)}) " +
                        $"but got ({string.Join(" ", sbytes)}).");
        }

        private enum SomeEnum
        {
            Value1 = 123456789,
            Value2 = 987654321
        }

        private class MemorySegment<T> : ReadOnlySequenceSegment<T>
        {
            public MemorySegment(ReadOnlyMemory<T> memory)
            {
                Memory = memory;
            }

            public MemorySegment<T> Append(ReadOnlyMemory<T> memory)
            {
                var segment = new MemorySegment<T>(memory)
                {
                    RunningIndex = RunningIndex + Memory.Length
                };

                Next = segment;

                return segment;
            }
        }
    }
}
