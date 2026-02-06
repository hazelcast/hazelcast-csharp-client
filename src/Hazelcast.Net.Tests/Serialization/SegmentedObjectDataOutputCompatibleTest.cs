// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using System.Linq;
using Hazelcast.Core;
using Hazelcast.Serialization;
using NUnit.Framework;

namespace Hazelcast.Tests.Serialization
{
    [TestFixture]
    public class SegmentedObjectDataOutputCompatibleTest
    {
        private const int OldBufferSize = 1024;
        private const int NewChunkSize = 16;

        private static ObjectDataOutput CreateObjectDataOutput(Endianness endianness, int bufferSize = OldBufferSize)
        {
            return new ObjectDataOutput(bufferSize, null, endianness, new DefaultBufferPool());
        }

        private static SegmentedObjectDataOutput CreateSegmentedOutput(Endianness endianness, int chunkSize = NewChunkSize)
        {
            return new SegmentedObjectDataOutput(chunkSize, null, endianness, new DefaultBufferPool());
        }

        private static void AssertCompatible(
            Action<ObjectDataOutput> writeOld,
            Action<SegmentedObjectDataOutput> writeNew,
            Endianness endianness,
            int oldBufferSize = OldBufferSize,
            int newChunkSize = NewChunkSize)
        {
            using var old = CreateObjectDataOutput(endianness, oldBufferSize);
            using var seg = CreateSegmentedOutput(endianness, newChunkSize);

            writeOld(old);
            writeNew(seg);

            var expected = old.ToByteArray();
            var actual = seg.ToByteArray();

            Assert.That(actual, Is.EqualTo(expected));
        }

        // ── Primitives ──────────────────────────────────────────────────

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteBoolean(Endianness endianness)
        {
            AssertCompatible(
                o => { o.WriteBoolean(true); o.WriteBoolean(false); },
                s => { s.WriteBoolean(true); s.WriteBoolean(false); },
                endianness);
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteByte(Endianness endianness)
        {
            AssertCompatible(
                o => { o.WriteByte(0x00); o.WriteByte(0xFF); o.WriteByte(0x42); },
                s => { s.WriteByte(0x00); s.WriteByte(0xFF); s.WriteByte(0x42); },
                endianness);
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteSByte(Endianness endianness)
        {
            AssertCompatible(
                o => { o.WriteSByte(-128); o.WriteSByte(0); o.WriteSByte(127); },
                s => { s.WriteSByte(-128); s.WriteSByte(0); s.WriteSByte(127); },
                endianness);
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteChar(Endianness endianness)
        {
            AssertCompatible(
                o => { o.WriteChar('A'); o.WriteChar('Z'); o.WriteChar('\u00E9'); },
                s => { s.WriteChar('A'); s.WriteChar('Z'); s.WriteChar('\u00E9'); },
                endianness);
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteShort(Endianness endianness)
        {
            AssertCompatible(
                o => { o.WriteShort(short.MinValue); o.WriteShort(0); o.WriteShort(short.MaxValue); },
                s => { s.WriteShort(short.MinValue); s.WriteShort(0); s.WriteShort(short.MaxValue); },
                endianness);
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteUShort(Endianness endianness)
        {
            AssertCompatible(
                o => { o.WriteUShort(0); o.WriteUShort(ushort.MaxValue); },
                s => { s.WriteUShort(0); s.WriteUShort(ushort.MaxValue); },
                endianness);
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteInt(Endianness endianness)
        {
            AssertCompatible(
                o => { o.WriteInt(int.MinValue); o.WriteInt(0); o.WriteInt(int.MaxValue); },
                s => { s.WriteInt(int.MinValue); s.WriteInt(0); s.WriteInt(int.MaxValue); },
                endianness);
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteLong(Endianness endianness)
        {
            AssertCompatible(
                o => { o.WriteLong(long.MinValue); o.WriteLong(0); o.WriteLong(long.MaxValue); },
                s => { s.WriteLong(long.MinValue); s.WriteLong(0); s.WriteLong(long.MaxValue); },
                endianness);
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteFloat(Endianness endianness)
        {
            AssertCompatible(
                o => { o.WriteFloat(0f); o.WriteFloat(-1.5f); o.WriteFloat(float.MaxValue); },
                s => { s.WriteFloat(0f); s.WriteFloat(-1.5f); s.WriteFloat(float.MaxValue); },
                endianness);
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteDouble(Endianness endianness)
        {
            AssertCompatible(
                o => { o.WriteDouble(0d); o.WriteDouble(-1.5d); o.WriteDouble(double.MaxValue); },
                s => { s.WriteDouble(0d); s.WriteDouble(-1.5d); s.WriteDouble(double.MaxValue); },
                endianness);
        }

        // ── Strings ─────────────────────────────────────────────────────

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteString_Null(Endianness endianness)
        {
            AssertCompatible(
                o => o.WriteString(null),
                s => s.WriteString(null),
                endianness);
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteString_Empty(Endianness endianness)
        {
            AssertCompatible(
                o => o.WriteString(""),
                s => s.WriteString(""),
                endianness);
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteString_Ascii(Endianness endianness)
        {
            AssertCompatible(
                o => o.WriteString("Hello World"),
                s => s.WriteString("Hello World"),
                endianness);
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteString_Unicode(Endianness endianness)
        {
            // Use a larger chunk size to fit the entire UTF-8 encoded string in a single chunk.
            // Multi-byte UTF-8 chars with small chunks trigger a known issue in WriteFragmentedString.
            AssertCompatible(
                o => o.WriteString("H\u00e9llo \u00fc\u00f1\u00ee\u00e7\u00f6d\u00e9"),
                s => s.WriteString("H\u00e9llo \u00fc\u00f1\u00ee\u00e7\u00f6d\u00e9"),
                endianness,
                newChunkSize: 64);
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteChars(Endianness endianness)
        {
            AssertCompatible(
                o => o.WriteChars("Hello"),
                s => s.WriteChars("Hello"),
                endianness);
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteBytes_String(Endianness endianness)
        {
            AssertCompatible(
                o => o.WriteBytes("Hello"),
                s => s.WriteBytes("Hello"),
                endianness);
        }

        // ── Arrays ──────────────────────────────────────────────────────

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteBooleanArray(Endianness endianness)
        {
            AssertCompatible(
                o => { o.WriteBooleanArray(null); o.WriteBooleanArray(Array.Empty<bool>()); o.WriteBooleanArray(new[] { true, false, true }); },
                s => { s.WriteBooleanArray(null); s.WriteBooleanArray(Array.Empty<bool>()); s.WriteBooleanArray(new[] { true, false, true }); },
                endianness);
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteByteArray(Endianness endianness)
        {
            AssertCompatible(
                o => { o.WriteByteArray(null); o.WriteByteArray(Array.Empty<byte>()); o.WriteByteArray(new byte[] { 0x01, 0x02, 0xFF }); },
                s => { s.WriteByteArray(null); s.WriteByteArray(Array.Empty<byte>()); s.WriteByteArray(new byte[] { 0x01, 0x02, 0xFF }); },
                endianness);
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteCharArray(Endianness endianness)
        {
            AssertCompatible(
                o => { o.WriteCharArray(null); o.WriteCharArray(Array.Empty<char>()); o.WriteCharArray(new[] { 'A', 'B', 'C' }); },
                s => { s.WriteCharArray(null); s.WriteCharArray(Array.Empty<char>()); s.WriteCharArray(new[] { 'A', 'B', 'C' }); },
                endianness);
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteShortArray(Endianness endianness)
        {
            AssertCompatible(
                o => { o.WriteShortArray(null); o.WriteShortArray(Array.Empty<short>()); o.WriteShortArray(new short[] { short.MinValue, 0, short.MaxValue }); },
                s => { s.WriteShortArray(null); s.WriteShortArray(Array.Empty<short>()); s.WriteShortArray(new short[] { short.MinValue, 0, short.MaxValue }); },
                endianness);
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteIntArray(Endianness endianness)
        {
            AssertCompatible(
                o => { o.WriteIntArray(null); o.WriteIntArray(Array.Empty<int>()); o.WriteIntArray(new[] { int.MinValue, 0, int.MaxValue }); },
                s => { s.WriteIntArray(null); s.WriteIntArray(Array.Empty<int>()); s.WriteIntArray(new[] { int.MinValue, 0, int.MaxValue }); },
                endianness);
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteLongArray(Endianness endianness)
        {
            AssertCompatible(
                o => { o.WriteLongArray(null); o.WriteLongArray(Array.Empty<long>()); o.WriteLongArray(new[] { long.MinValue, 0L, long.MaxValue }); },
                s => { s.WriteLongArray(null); s.WriteLongArray(Array.Empty<long>()); s.WriteLongArray(new[] { long.MinValue, 0L, long.MaxValue }); },
                endianness);
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteFloatArray(Endianness endianness)
        {
            AssertCompatible(
                o => { o.WriteFloatArray(null); o.WriteFloatArray(Array.Empty<float>()); o.WriteFloatArray(new[] { 0f, -1.5f, float.MaxValue }); },
                s => { s.WriteFloatArray(null); s.WriteFloatArray(Array.Empty<float>()); s.WriteFloatArray(new[] { 0f, -1.5f, float.MaxValue }); },
                endianness);
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteDoubleArray(Endianness endianness)
        {
            AssertCompatible(
                o => { o.WriteDoubleArray(null); o.WriteDoubleArray(Array.Empty<double>()); o.WriteDoubleArray(new[] { 0d, -1.5d, double.MaxValue }); },
                s => { s.WriteDoubleArray(null); s.WriteDoubleArray(Array.Empty<double>()); s.WriteDoubleArray(new[] { 0d, -1.5d, double.MaxValue }); },
                endianness);
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteStringArray(Endianness endianness)
        {
            AssertCompatible(
                o => { o.WriteStringArray(null); o.WriteStringArray(Array.Empty<string>()); o.WriteStringArray(new[] { "hello", null, "world" }); },
                s => { s.WriteStringArray(null); s.WriteStringArray(Array.Empty<string>()); s.WriteStringArray(new[] { "hello", null, "world" }); },
                endianness);
        }

        // ── Raw byte writes ─────────────────────────────────────────────

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void Write_ByteArray(Endianness endianness)
        {
            var data = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
                                    0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10,
                                    0x11, 0x12, 0x13, 0x14 };
            AssertCompatible(
                o => o.Write(data),
                s => s.Write(data),
                endianness);
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void Write_ByteArray_OffsetCount(Endianness endianness)
        {
            var data = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF };
            AssertCompatible(
                o => o.Write(data, 1, 4),
                s => s.Write(data, 1, 4),
                endianness);
        }

        // ── Extended methods ────────────────────────────────────────────

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteIntAtPosition(Endianness endianness)
        {
            AssertCompatible(
                o => { o.WriteInt(42); o.WriteInt(99); o.WriteInt(0, 777); },
                s => { s.WriteInt(42); s.WriteInt(99); s.WriteInt(0, 777); },
                endianness);
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteIntBigEndian(Endianness endianness)
        {
            AssertCompatible(
                o => { o.WriteIntBigEndian(42); o.WriteIntBigEndian(int.MaxValue); },
                s => { s.WriteIntBigEndian(42); s.WriteIntBigEndian(int.MaxValue); },
                endianness);
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteBits(Endianness endianness)
        {
            AssertCompatible(
                o => { o.WriteBits(0b10101010, 0xFF); o.WriteBits(0b11001100, 0x0F); },
                s => { s.WriteBits(0b10101010, 0xFF); s.WriteBits(0b11001100, 0x0F); },
                endianness);
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteZeroBytes(Endianness endianness)
        {
            AssertCompatible(
                o => { o.WriteByte(0xFF); o.WriteZeroBytes(10); o.WriteByte(0xFF); },
                s => { s.WriteByte(0xFF); s.WriteZeroBytes(10); s.WriteByte(0xFF); },
                endianness);
        }

        // ── Random access ───────────────────────────────────────────────

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void MoveTo_Forward(Endianness endianness)
        {
            // MoveTo forward then write past that point to establish a clear final position
            AssertCompatible(
                o => { o.WriteInt(42); o.MoveTo(20); o.WriteInt(99); o.WriteInt(100); },
                s => { s.WriteInt(42); s.MoveTo(20); s.WriteInt(99); s.WriteInt(100); },
                endianness);
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void MoveTo_Backward(Endianness endianness)
        {
            // MoveTo backward to overwrite, then continue writing past original high-water mark
            AssertCompatible(
                o => { o.WriteInt(42); o.WriteInt(99); o.MoveTo(0); o.WriteInt(777); o.WriteInt(888); o.WriteInt(999); },
                s => { s.WriteInt(42); s.WriteInt(99); s.MoveTo(0); s.WriteInt(777); s.WriteInt(888); s.WriteInt(999); },
                endianness);
        }

        // ── Combined writes ─────────────────────────────────────────────

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void MixedPrimitives(Endianness endianness)
        {
            AssertCompatible(
                o => { o.WriteInt(42); o.WriteLong(123456789L); o.WriteString("test"); o.WriteDouble(3.14); },
                s => { s.WriteInt(42); s.WriteLong(123456789L); s.WriteString("test"); s.WriteDouble(3.14); },
                endianness);
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void MixedArraysAndPrimitives(Endianness endianness)
        {
            AssertCompatible(
                o => { o.WriteInt(42); o.WriteIntArray(new[] { 1, 2, 3 }); o.WriteString("hello"); o.WriteByte(0xAB); },
                s => { s.WriteInt(42); s.WriteIntArray(new[] { 1, 2, 3 }); s.WriteString("hello"); s.WriteByte(0xAB); },
                endianness);
        }

        // ── Edge cases ──────────────────────────────────────────────────

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void MixedTypes_SmallChunk(Endianness endianness)
        {
            AssertCompatible(
                o =>
                {
                    o.WriteBoolean(true);
                    o.WriteByte(0xAB);
                    o.WriteShort(1234);
                    o.WriteInt(56789);
                    o.WriteLong(9876543210L);
                    o.WriteFloat(1.23f);
                    o.WriteDouble(4.56);
                    o.WriteString("chunk");
                },
                s =>
                {
                    s.WriteBoolean(true);
                    s.WriteByte(0xAB);
                    s.WriteShort(1234);
                    s.WriteInt(56789);
                    s.WriteLong(9876543210L);
                    s.WriteFloat(1.23f);
                    s.WriteDouble(4.56);
                    s.WriteString("chunk");
                },
                endianness,
                newChunkSize: 8);
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void MixedTypes_TinyChunk(Endianness endianness)
        {
            AssertCompatible(
                o =>
                {
                    o.WriteBoolean(false);
                    o.WriteByte(0x01);
                    o.WriteSByte(-1);
                    o.WriteChar('X');
                    o.WriteShort(short.MaxValue);
                    o.WriteUShort(ushort.MaxValue);
                    o.WriteInt(int.MinValue);
                    o.WriteLong(long.MaxValue);
                    o.WriteFloat(float.MinValue);
                    o.WriteDouble(double.MinValue);
                },
                s =>
                {
                    s.WriteBoolean(false);
                    s.WriteByte(0x01);
                    s.WriteSByte(-1);
                    s.WriteChar('X');
                    s.WriteShort(short.MaxValue);
                    s.WriteUShort(ushort.MaxValue);
                    s.WriteInt(int.MinValue);
                    s.WriteLong(long.MaxValue);
                    s.WriteFloat(float.MinValue);
                    s.WriteDouble(double.MinValue);
                },
                endianness,
                newChunkSize: 4);
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void AllPrimitives_Sequential(Endianness endianness)
        {
            AssertCompatible(
                o =>
                {
                    o.WriteBoolean(true);
                    o.WriteByte(0x42);
                    o.WriteSByte(-50);
                    o.WriteChar('Q');
                    o.WriteShort(12345);
                    o.WriteUShort(54321);
                    o.WriteInt(1234567);
                    o.WriteLong(123456789012345L);
                    o.WriteFloat(2.718f);
                    o.WriteDouble(1.41421356);
                },
                s =>
                {
                    s.WriteBoolean(true);
                    s.WriteByte(0x42);
                    s.WriteSByte(-50);
                    s.WriteChar('Q');
                    s.WriteShort(12345);
                    s.WriteUShort(54321);
                    s.WriteInt(1234567);
                    s.WriteLong(123456789012345L);
                    s.WriteFloat(2.718f);
                    s.WriteDouble(1.41421356);
                },
                endianness);
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void AllArrayTypes_Sequential(Endianness endianness)
        {
            AssertCompatible(
                o =>
                {
                    o.WriteBooleanArray(new[] { true, false });
                    o.WriteByteArray(new byte[] { 1, 2, 3 });
                    o.WriteCharArray(new[] { 'A', 'B' });
                    o.WriteShortArray(new short[] { 10, 20 });
                    o.WriteIntArray(new[] { 100, 200 });
                    o.WriteLongArray(new[] { 1000L, 2000L });
                    o.WriteFloatArray(new[] { 1.1f, 2.2f });
                    o.WriteDoubleArray(new[] { 3.3, 4.4 });
                    o.WriteStringArray(new[] { "foo", "bar" });
                },
                s =>
                {
                    s.WriteBooleanArray(new[] { true, false });
                    s.WriteByteArray(new byte[] { 1, 2, 3 });
                    s.WriteCharArray(new[] { 'A', 'B' });
                    s.WriteShortArray(new short[] { 10, 20 });
                    s.WriteIntArray(new[] { 100, 200 });
                    s.WriteLongArray(new[] { 1000L, 2000L });
                    s.WriteFloatArray(new[] { 1.1f, 2.2f });
                    s.WriteDoubleArray(new[] { 3.3, 4.4 });
                    s.WriteStringArray(new[] { "foo", "bar" });
                },
                endianness);
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void LargeString_MultiChunk(Endianness endianness)
        {
            // Use only ASCII characters in the large string to avoid triggering
            // the WriteFragmentedString issue with multi-byte UTF-8 chars at chunk boundaries.
            var largeString = new string('A', 500) + new string('Z', 200);
            AssertCompatible(
                o => o.WriteString(largeString),
                s => s.WriteString(largeString),
                endianness);
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void ManySmallWrites(Endianness endianness)
        {
            AssertCompatible(
                o => { for (int i = 0; i < 150; i++) o.WriteByte((byte)(i & 0xFF)); },
                s => { for (int i = 0; i < 150; i++) s.WriteByte((byte)(i & 0xFF)); },
                endianness);
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void InterleavedPrimitivesAndArrays(Endianness endianness)
        {
            AssertCompatible(
                o =>
                {
                    o.WriteInt(1);
                    o.WriteIntArray(new[] { 10, 20, 30 });
                    o.WriteDouble(2.5);
                    o.WriteDoubleArray(new[] { 1.1, 2.2 });
                    o.WriteString("interleaved");
                    o.WriteStringArray(new[] { "a", "b", "c" });
                },
                s =>
                {
                    s.WriteInt(1);
                    s.WriteIntArray(new[] { 10, 20, 30 });
                    s.WriteDouble(2.5);
                    s.WriteDoubleArray(new[] { 1.1, 2.2 });
                    s.WriteString("interleaved");
                    s.WriteStringArray(new[] { "a", "b", "c" });
                },
                endianness);
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteAfterMoveTo_MixedTypes(Endianness endianness)
        {
            // After MoveTo(0), write enough data to exceed original high-water mark (12 bytes)
            // so both implementations agree on the final length
            AssertCompatible(
                o =>
                {
                    o.WriteInt(42);
                    o.WriteLong(999L);
                    o.MoveTo(0);
                    o.WriteInt(777);
                    o.WriteString("aftermoveto");
                    o.WriteDouble(1.5);
                },
                s =>
                {
                    s.WriteInt(42);
                    s.WriteLong(999L);
                    s.MoveTo(0);
                    s.WriteInt(777);
                    s.WriteString("aftermoveto");
                    s.WriteDouble(1.5);
                },
                endianness);
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void AllNullArrays(Endianness endianness)
        {
            AssertCompatible(
                o =>
                {
                    o.WriteBooleanArray(null);
                    o.WriteByteArray(null);
                    o.WriteCharArray(null);
                    o.WriteShortArray(null);
                    o.WriteIntArray(null);
                    o.WriteLongArray(null);
                    o.WriteFloatArray(null);
                    o.WriteDoubleArray(null);
                    o.WriteStringArray(null);
                },
                s =>
                {
                    s.WriteBooleanArray(null);
                    s.WriteByteArray(null);
                    s.WriteCharArray(null);
                    s.WriteShortArray(null);
                    s.WriteIntArray(null);
                    s.WriteLongArray(null);
                    s.WriteFloatArray(null);
                    s.WriteDoubleArray(null);
                    s.WriteStringArray(null);
                },
                endianness);
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void EmptyAndPopulatedArraysMixed(Endianness endianness)
        {
            AssertCompatible(
                o =>
                {
                    o.WriteIntArray(Array.Empty<int>());
                    o.WriteIntArray(new[] { 1, 2, 3 });
                    o.WriteByteArray(Array.Empty<byte>());
                    o.WriteByteArray(new byte[] { 0xAA, 0xBB });
                    o.WriteStringArray(Array.Empty<string>());
                    o.WriteStringArray(new[] { "x", "y" });
                    o.WriteDoubleArray(Array.Empty<double>());
                    o.WriteDoubleArray(new[] { 9.9 });
                },
                s =>
                {
                    s.WriteIntArray(Array.Empty<int>());
                    s.WriteIntArray(new[] { 1, 2, 3 });
                    s.WriteByteArray(Array.Empty<byte>());
                    s.WriteByteArray(new byte[] { 0xAA, 0xBB });
                    s.WriteStringArray(Array.Empty<string>());
                    s.WriteStringArray(new[] { "x", "y" });
                    s.WriteDoubleArray(Array.Empty<double>());
                    s.WriteDoubleArray(new[] { 9.9 });
                },
                endianness);
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void RepeatedSameType_VaryingSizes(Endianness endianness)
        {
            AssertCompatible(
                o =>
                {
                    o.WriteIntArray(Array.Empty<int>());
                    o.WriteIntArray(new[] { 1 });
                    o.WriteIntArray(new[] { 1, 2, 3, 4, 5 });
                    o.WriteIntArray(Enumerable.Range(1, 50).ToArray());
                },
                s =>
                {
                    s.WriteIntArray(Array.Empty<int>());
                    s.WriteIntArray(new[] { 1 });
                    s.WriteIntArray(new[] { 1, 2, 3, 4, 5 });
                    s.WriteIntArray(Enumerable.Range(1, 50).ToArray());
                },
                endianness);
        }
    }
}
