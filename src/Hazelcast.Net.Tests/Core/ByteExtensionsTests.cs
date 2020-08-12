using System;
using System.Buffers;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Tests.Core
{
    [TestFixture]
    public class ByteExtensionsTests
    {
        // endianness is, by default, unspecified and then falls back to big endian

        [Test]
        public void WriteByte()
        {
            var bytes = new byte[8];

            Assert.Throws<ArgumentNullException>(() => ((byte[])null).WriteByte(2, 42));
            Assert.Throws<ArgumentOutOfRangeException>(() => bytes.WriteByte(-1, 42));
            Assert.Throws<ArgumentOutOfRangeException>(() => bytes.WriteByte(8, 42));

            bytes.WriteByte(2, 42);
            AssertBytes(bytes, 0, 0, 42, 0, 0, 0, 0, 0);

            bytes.WriteByteL(2, 42);
            AssertBytes(bytes, 0, 0, 42, 0, 0, 0, 0, 0);
        }

        [Test]
        public void ReadByte()
        {
            var bytes = new byte[] { 0, 0, 42 };

            Assert.Throws<ArgumentNullException>(() => _ = ((byte[])null).ReadByte(2));
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = bytes.ReadByte(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = bytes.ReadByte(8));

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

            Assert.Throws<ArgumentNullException>(() => _ = ((byte[])null).ReadBool(2));
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = bytes.ReadBool(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = bytes.ReadBool(8));

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

            Assert.Throws<ArgumentNullException>(() => ((byte[]) null).WriteChar(2, 'x'));
            Assert.Throws<ArgumentOutOfRangeException>(() => bytes.WriteChar(-1, 'x'));
            Assert.Throws<ArgumentOutOfRangeException>(() => bytes.WriteChar(7, 'x'));

            bytes.WriteChar(2, 'x');
            AssertBytes(bytes, 0, 0, 0, 120, 0, 0, 0, 0);

            bytes.WriteChar(2, 'x', Endianness.LittleEndian);
            AssertBytes(bytes, 0, 0, 120, 0, 0, 0, 0, 0);

            bytes.WriteChar(2, '❤');
            AssertBytes(bytes, 0, 0, 39, 100, 0, 0, 0, 0);

            bytes.WriteChar(2, '❤', Endianness.LittleEndian);
            AssertBytes(bytes, 0, 0, 100, 39, 0, 0, 0, 0);
        }

        [Test]
        public void ReadChar()
        {
            var bytes = new byte[] { 0, 0, 0, 120, 0 };

            Assert.Throws<ArgumentNullException>(() => _ = ((byte[])null).ReadChar(2));
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = bytes.ReadChar(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = bytes.ReadChar(7));

            Assert.That(bytes.ReadChar(2), Is.EqualTo('x'));

            bytes = new byte[] { 0, 0, 120, 0 };
            Assert.That(bytes.ReadChar(2, Endianness.LittleEndian), Is.EqualTo('x'));

            bytes = new byte[] { 0, 0, 39, 100 };
            Assert.That(bytes.ReadChar(2), Is.EqualTo('❤'));

            bytes = new byte[] { 0, 0, 100, 39 };
            Assert.That(bytes.ReadChar(2, Endianness.LittleEndian), Is.EqualTo('❤'));
        }

        [Test]
        public void WriteInt()
        {
            var bytes = new byte[8];

            Assert.Throws<ArgumentNullException>(() => ((byte[])null).WriteInt(2, 123456789));
            Assert.Throws<ArgumentOutOfRangeException>(() => bytes.WriteInt(-1, 123456789));
            Assert.Throws<ArgumentOutOfRangeException>(() => bytes.WriteInt(7, 123456789));

            bytes.WriteInt(2, 123456789);
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

            Assert.Throws<ArgumentNullException>(() => _= ((byte[])null).ReadInt(2));
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = bytes.ReadInt(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = bytes.ReadInt(7));

            Assert.That(bytes.ReadInt(2), Is.EqualTo(123456789));

            bytes = new byte[] { 0, 0, 21, 205, 91, 7, 0 };
            Assert.That(bytes.ReadInt(2, Endianness.LittleEndian), Is.EqualTo(123456789));
            Assert.That(bytes.ReadIntL(2), Is.EqualTo(123456789));
        }

        [Test]
        public void ReadIntSequence()
        {
            var bytes = new byte[] { 0 };
            var sequence = new ReadOnlySequence<byte>(bytes);

            Assert.Throws<ArgumentException>(() => _ = BytesExtensions.ReadInt(ref sequence));

            bytes = new byte[] { 7, 91, 205, 21, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            sequence = new ReadOnlySequence<byte>(bytes);
            Assert.That(BytesExtensions.ReadInt(ref sequence), Is.EqualTo(123456789));
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
            Assert.That(BytesExtensions.ReadInt(ref sequence), Is.EqualTo(123456789));
            Assert.That(sequence.Length, Is.EqualTo(12));
        }

        [Test]
        public void ReadIntSpan()
        {
            var bytes = new byte[] { 0 };
            var span = new ReadOnlySpan<byte>(bytes);

            try
            {
                _ = span.ReadInt();
                Assert.Fail("Expected an exception.");
            }
            catch (ArgumentException) { /* expected */ }

            bytes = new byte[] { 7, 91, 205, 21, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            span = new ReadOnlySpan<byte>(bytes);
            Assert.That(span.ReadInt(), Is.EqualTo(123456789));

            bytes = new byte[] { 21, 205, 91, 7, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            span = new ReadOnlySpan<byte>(bytes);
            Assert.That(span.ReadInt(Endianness.LittleEndian), Is.EqualTo(123456789));
        }

        [Test]
        public void WriteIntEnum()
        {
            var bytes = new byte[8];

            Assert.Throws<ArgumentNullException>(() => ((byte[])null).WriteInt(2, SomeEnum.Value1));
            Assert.Throws<ArgumentOutOfRangeException>(() => bytes.WriteInt(-1, SomeEnum.Value1));
            Assert.Throws<ArgumentOutOfRangeException>(() => bytes.WriteInt(7, SomeEnum.Value1));

            bytes.WriteInt(2, SomeEnum.Value1);
            AssertBytes(bytes, 0, 0, 7, 91, 205, 21, 0);

            bytes.WriteInt(2, SomeEnum.Value1, Endianness.LittleEndian);
            AssertBytes(bytes, 0, 0, 21, 205, 91, 7, 0);

            bytes.WriteIntL(2, SomeEnum.Value1);
            AssertBytes(bytes, 0, 0, 21, 205, 91, 7, 0);
        }

        [Test]
        public void WriteShort()
        {
            var bytes = new byte[8];

            Assert.Throws<ArgumentNullException>(() => ((byte[])null).WriteShort(2, 12345));
            Assert.Throws<ArgumentOutOfRangeException>(() => bytes.WriteShort(-1, 12345));
            Assert.Throws<ArgumentOutOfRangeException>(() => bytes.WriteShort(7, 12345));

            bytes.WriteShort(2, 12345);
            AssertBytes(bytes, 0, 0, 48, 57, 0, 0, 0);

            bytes.WriteShort(2, 12345, Endianness.LittleEndian);
            AssertBytes(bytes, 0, 0, 57, 48, 0, 0, 0);
        }

        [Test]
        public void ReadShort()
        {
            var bytes = new byte[] { 0, 0, 48, 57, 0, 0, 0 };

            Assert.Throws<ArgumentNullException>(() => _ = ((byte[])null).ReadShort(2));
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = bytes.ReadShort(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = bytes.ReadShort(7));

            Assert.That(bytes.ReadShort(2), Is.EqualTo(12345));

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
                _ = span.ReadShort();
                Assert.Fail("Expected an exception.");
            }
            catch (ArgumentException) { /* expected */ }

            bytes = new byte[] { 48, 57, 0, 0, 0 };
            span = new ReadOnlySpan<byte>(bytes);
            Assert.That(span.ReadShort(), Is.EqualTo(12345));

            bytes = new byte[] { 57, 48, 0, 0, 0 };
            span = new ReadOnlySpan<byte>(bytes);
            Assert.That(span.ReadShort(Endianness.LittleEndian), Is.EqualTo(12345));
        }

        [Test]
        public void WriteUShort()
        {
            var bytes = new byte[8];

            Assert.Throws<ArgumentNullException>(() => ((byte[])null).WriteUShort(2, 12345));
            Assert.Throws<ArgumentOutOfRangeException>(() => bytes.WriteUShort(-1, 12345));
            Assert.Throws<ArgumentOutOfRangeException>(() => bytes.WriteUShort(7, 12345));

            bytes.WriteUShort(2, 12345);
            AssertBytes(bytes, 0, 0, 48, 57, 0, 0, 0);

            bytes.WriteUShort(2, 12345, Endianness.LittleEndian);
            AssertBytes(bytes, 0, 0, 57, 48, 0, 0, 0);
        }

        [Test]
        public void ReadUShortSequence()
        {
            var bytes = new byte[] { 0 };
            var sequence = new ReadOnlySequence<byte>(bytes);

            Assert.Throws<ArgumentException>(() => _ = BytesExtensions.ReadUShort(ref sequence));

            bytes = new byte[] { 48, 57, 0, 0, 0 };
            sequence = new ReadOnlySequence<byte>(bytes);
            Assert.That(BytesExtensions.ReadUShort(ref sequence), Is.EqualTo(12345));
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
            Assert.That(BytesExtensions.ReadUShort(ref sequence), Is.EqualTo(12345));
            Assert.That(sequence.Length, Is.EqualTo(3));
        }

        [Test]
        public void ReadUShortSpan()
        {
            var bytes = new byte[] { 0 };
            var span = new ReadOnlySpan<byte>(bytes);

            try
            {
                _ = span.ReadUShort();
                Assert.Fail("Expected an exception.");
            }
            catch (ArgumentException) { /* expected */ }

            bytes = new byte[] { 48, 57, 0, 0, 0 };
            span = new ReadOnlySpan<byte>(bytes);
            Assert.That(span.ReadUShort(), Is.EqualTo(12345));

            bytes = new byte[] { 57, 48, 0, 0, 0 };
            span = new ReadOnlySpan<byte>(bytes);
            Assert.That(span.ReadUShort(Endianness.LittleEndian), Is.EqualTo(12345));
        }

        [Test]
        public void WriteLong()
        {
            var bytes = new byte[16];

            Assert.Throws<ArgumentNullException>(() => ((byte[])null).WriteLong(2, (long) int.MaxValue + 123456));
            Assert.Throws<ArgumentOutOfRangeException>(() => bytes.WriteLong(-1, (long)int.MaxValue + 123456));
            Assert.Throws<ArgumentOutOfRangeException>(() => bytes.WriteLong(14, (long)int.MaxValue + 123456));

            bytes.WriteLong(2, (long)int.MaxValue + 123456);
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

            Assert.Throws<ArgumentNullException>(() => _ = ((byte[])null).ReadLong(2));
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = bytes.ReadLong(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = bytes.ReadLong(15));

            Assert.That(bytes.ReadLong(2), Is.EqualTo((long)int.MaxValue + 123456));

            bytes = new byte[] { 0, 0, 63, 226, 1, 128, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            Assert.That(bytes.ReadLong(2, Endianness.LittleEndian), Is.EqualTo((long)int.MaxValue + 123456));
            Assert.That(bytes.ReadLongL(2), Is.EqualTo((long)int.MaxValue + 123456));
        }

        [Test]
        public void WriteGuid()
        {
            var bytes = new byte[24];

            Assert.Throws<ArgumentNullException>(() => ((byte[])null).WriteGuid(2, Guid.Empty));
            Assert.Throws<ArgumentOutOfRangeException>(() => bytes.WriteGuid(-1, Guid.Empty));
            Assert.Throws<ArgumentOutOfRangeException>(() => bytes.WriteGuid(23, Guid.Empty));

            var guid = Guid.NewGuid();
            var values = guid.ToByteArray();

            bytes.WriteGuid(2, guid);
            AssertBytes(bytes, 0, 0, 0,
                values[6], values[7], values[4], values[5],
                values[0], values[1], values[2], values[3],
                values[15], values[14], values[13], values[12],
                values[11], values[10], values[9], values[8]);

            bytes.WriteGuid(2, Guid.Empty);
            AssertBytes(bytes, 0, 0, 1,
                // not modified
                values[6], values[7], values[4], values[5],
                values[0], values[1], values[2], values[3],
                values[15], values[14], values[13], values[12],
                values[11], values[10], values[9], values[8]);

            // endianness is not an options for guids
            bytes.WriteGuidL(2, guid);
            AssertBytes(bytes, 0, 0, 0,
                values[6], values[7], values[4], values[5],
                values[0], values[1], values[2], values[3],
                values[15], values[14], values[13], values[12],
                values[11], values[10], values[9], values[8]);
        }

        [Test]
        public void ReadGuid()
        {
            var bytes = new byte[] { 0, 0, 1 };

            Assert.Throws<ArgumentNullException>(() => _ = ((byte[])null).ReadGuid(2));
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = bytes.ReadGuid(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = bytes.ReadGuid(2));

            var guid = Guid.NewGuid();
            var values = guid.ToByteArray();
            bytes = new byte[] { 0, 0, 0,
                values[6], values[7], values[4], values[5],
                values[0], values[1], values[2], values[3],
                values[15], values[14], values[13], values[12],
                values[11], values[10], values[9], values[8] };
            Assert.That(bytes.ReadGuid(2), Is.EqualTo(guid));
            Assert.That(bytes.ReadGuidL(2), Is.EqualTo(guid));

            bytes = new byte[] { 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            Assert.That(bytes.ReadGuid(2), Is.EqualTo(Guid.Empty));
            Assert.That(bytes.ReadGuidL(2), Is.EqualTo(Guid.Empty));
        }

        [Test]
        public void WriteUtf8Char()
        {
            var bytes = new byte[8];

            var pos = 0;
            Assert.Throws<ArgumentNullException>(() => ((byte[])null).WriteUtf8Char(ref pos, Utf8Char1Byte));
            pos = -1;
            Assert.Throws<ArgumentOutOfRangeException>(() => bytes.WriteUtf8Char(ref pos, Utf8Char1Byte));
            pos = 8;
            Assert.Throws<ArgumentOutOfRangeException>(() => bytes.WriteUtf8Char(ref pos, Utf8Char1Byte));
            pos = 7;
            Assert.Throws<ArgumentOutOfRangeException>(() => bytes.WriteUtf8Char(ref pos, Utf8Char2Bytes));
            pos = 6;
            Assert.Throws<ArgumentOutOfRangeException>(() => bytes.WriteUtf8Char(ref pos, Utf8Char3Bytes));

            pos = 2;
            bytes.WriteUtf8Char(ref pos, Utf8Char1Byte);
            Assert.That(pos, Is.EqualTo(3));
            AssertBytes(bytes, 0, 0, 0x78, 0, 0, 0, 0, 0);

            pos = 2;
            bytes.WriteUtf8Char(ref pos, Utf8Char2Bytes);
            Assert.That(pos, Is.EqualTo(4));
            AssertBytes(bytes, 0, 0, 0xc3, 0xa3, 0, 0, 0, 0);

            pos = 2;
            bytes.WriteUtf8Char(ref pos, Utf8Char3Bytes);
            Assert.That(pos, Is.EqualTo(5));
            AssertBytes(bytes, 0, 0, 0xe0, 0xa3, 0x9f, 0, 0, 0);

            // cannot write surrogate pairs
            // TODO: support writing surrogate pairs
            pos = 2;
            Assert.Throws<InvalidOperationException>(() => bytes.WriteUtf8Char(ref pos, Utf8Char4BytesH));
        }

        [Test]
        public void ReadUtf8Char()
        {
            var pos = 0;
            Assert.Throws<ArgumentNullException>(() => _ = ((byte[]) null).ReadUtf8Char(ref pos));
            var bytes = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0x78 };
            pos = 8;
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = bytes.ReadUtf8Char(ref pos));
            bytes = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0xc3 };
            pos = 7;
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = bytes.ReadUtf8Char(ref pos));
            bytes = new byte[] { 0, 0, 0, 0, 0, 0, 0xe0, 0xa3 };
            pos = 6;
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = bytes.ReadUtf8Char(ref pos));

            bytes = new byte[] { 0, 0, 0x78, 0, 0, 0, 0, 0 };
            pos = 2;
            Assert.That(bytes.ReadUtf8Char(ref pos), Is.EqualTo(Utf8Char1Byte));
            Assert.That(pos, Is.EqualTo(3));

            bytes = new byte[] { 0, 0, 0xc3, 0xe3, 0, 0, 0, 0 };
            pos = 2;
            Assert.That(bytes.ReadUtf8Char(ref pos), Is.EqualTo(Utf8Char2Bytes));
            Assert.That(pos, Is.EqualTo(4));

            bytes = new byte[] { 0, 0, 0xe0, 0xa3, 0x9f, 0, 0, 0 };
            pos = 2;
            Assert.That(bytes.ReadUtf8Char(ref pos), Is.EqualTo(Utf8Char3Bytes));
            Assert.That(pos, Is.EqualTo(5));

            // cannot read surrogate pairs
            // TODO: support reading surrogate pairs
            bytes = new byte[] { 0, 0, 0xf0, 0xa0, 0x8c, 0x8e, 0, 0 };
            pos = 2;
            Assert.Throws<InvalidOperationException>(() => _ = bytes.ReadUtf8Char(ref pos));
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

        [Test]
        public void ResolveEndianness()
        {
            Assert.That(Endianness.LittleEndian.Resolve(), Is.EqualTo(Endianness.LittleEndian));
            Assert.That(Endianness.BigEndian.Resolve(), Is.EqualTo(Endianness.BigEndian));
            Assert.That(Endianness.Unspecified.Resolve(), Is.EqualTo(Endianness.BigEndian));

            Assert.Throws<NotSupportedException>(() => _ = ((Endianness) 666).Resolve());

            Assert.That(Endianness.Native.Resolve(), Is.EqualTo(EndiannessExtensions.NativeEndianness));
            Assert.That(EndiannessExtensions.NativeEndianness, Is.EqualTo(BitConverter.IsLittleEndian ? Endianness.LittleEndian : Endianness.BigEndian));
        }

        private static void AssertBytes(byte[] bytes, params byte[] values)
        {
            var equals = true;
            for (var i = 0; i < bytes.Length && i < values.Length; i++)
                equals &= bytes[i] == values[i];

            if (equals) return;

            Assert.Fail($"Expected ({string.Join(" ", values)}) but got ({string.Join(" ", bytes)}).");
        }

        // http://www.i18nguy.com/unicode/supplementary-test.html
        //
        // '\u0078' ('x') LATIN SMALL LETTER X (U+0078) 78
        private const char Utf8Char1Byte = '\u0078';
        // '\u00E3' ('ã') LATIN SMALL LETTER A WITH TILDE (U+00E3) c3a3
        private const char Utf8Char2Bytes = '\u00e3';
        // '\u08DF' ARABIC SMALL HIGH WORD WAQFA (U+08DF) e0a39f
        private const char Utf8Char3Bytes = '\u08df';
        // there are no '4 bytes' chars in C#, surrogate pairs are 2 chars
        // '\u2070e' CJK UNIFIED IDEOGRAPH-2070E f0a09c8e
        private const char Utf8Char4BytesH = '\uf0a0';
        private const char Utf8Char4BytesL = '\u8c8e';

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
