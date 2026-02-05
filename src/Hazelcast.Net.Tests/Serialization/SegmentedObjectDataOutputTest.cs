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
using System.Buffers;
using System.Linq;
using System.Threading;
using Hazelcast.Core;
using Hazelcast.Serialization;
using NUnit.Framework;

namespace Hazelcast.Tests.Serialization
{
    public class SegmentedObjectDataOutputTest
    {
        private class TrackingBufferPool : IBufferPool
        {
            private readonly DefaultBufferPool _inner = new();
            public int RentCount;
            public int ReturnCount;

            public byte[] Rent(int minSize)
            {
                Interlocked.Increment(ref RentCount);
                return _inner.Rent(minSize);
            }

            public void Return(byte[] buffer)
            {
                Interlocked.Increment(ref ReturnCount);
                _inner.Return(buffer);
            }
        }

        private SegmentedObjectDataOutput CreateOutput(int chunkSize, Endianness endianness, IBufferPool pool = null)
        {
            pool ??= new DefaultBufferPool();
            return new SegmentedObjectDataOutput(chunkSize, null, endianness, pool);
        }

        private ObjectDataInput CreateInput(byte[] bytes, Endianness endianness)
        {
            return new ObjectDataInput(bytes, null, endianness);
        }

        #region 1. Primitive Writes

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteBoolean_RoundTrip(Endianness endianness)
        {
            using var output = CreateOutput(32, endianness);
            output.WriteBoolean(true);
            output.WriteBoolean(true);
            output.WriteBoolean(false);
            output.WriteBoolean(true);

            var bytes = output.GetSequence().ToArray();
            using var input = CreateInput(bytes, endianness);

            Assert.That(input.ReadBoolean(), Is.EqualTo(true));
            Assert.That(input.ReadBoolean(), Is.EqualTo(true));
            Assert.That(input.ReadBoolean(), Is.EqualTo(false));
            Assert.That(input.ReadBoolean(), Is.EqualTo(true));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteByte_RoundTrip(Endianness endianness)
        {
            using var output = CreateOutput(8, endianness);
            var aByte = (byte) 0x1;
            output.WriteByte(aByte);

            var bytes = output.GetSequence().ToArray();
            using var input = CreateInput(bytes, endianness);

            Assert.That(input.ReadByte(), Is.EqualTo(aByte));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteSByte_RoundTrip(Endianness endianness)
        {
            using var output = CreateOutput(8, endianness);
            var aByte = (byte) 0x1;
            output.WriteByte(aByte);

            var bytes = output.GetSequence().ToArray();
            using var input = CreateInput(bytes, endianness);

            Assert.That(input.ReadByte(), Is.EqualTo(aByte));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteChar_RoundTrip(Endianness endianness)
        {
            using var output = CreateOutput(64, endianness);
            output.WriteChar('A');
            output.WriteChar('Z');
            output.WriteChar('\u0000');
            output.WriteChar('\uFFFF');

            var bytes = output.GetSequence().ToArray();
            using var input = CreateInput(bytes, endianness);

            Assert.That(input.ReadChar(), Is.EqualTo('A'));
            Assert.That(input.ReadChar(), Is.EqualTo('Z'));
            Assert.That(input.ReadChar(), Is.EqualTo('\u0000'));
            Assert.That(input.ReadChar(), Is.EqualTo('\uFFFF'));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteShort_RoundTrip(Endianness endianness)
        {
            using var output = CreateOutput(64, endianness);
            output.WriteShort(short.MinValue);
            output.WriteShort(0);
            output.WriteShort(short.MaxValue);

            var bytes = output.GetSequence().ToArray();
            using var input = CreateInput(bytes, endianness);

            Assert.That(input.ReadShort(), Is.EqualTo(short.MinValue));
            Assert.That(input.ReadShort(), Is.EqualTo(0));
            Assert.That(input.ReadShort(), Is.EqualTo(short.MaxValue));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteUShort_RoundTrip(Endianness endianness)
        {
            using var output = CreateOutput(64, endianness);
            output.WriteUShort(ushort.MinValue);
            output.WriteUShort(12345);
            output.WriteUShort(ushort.MaxValue);

            var bytes = output.GetSequence().ToArray();
            using var input = CreateInput(bytes, endianness);

            Assert.That(input.ReadUShort(), Is.EqualTo(ushort.MinValue));
            Assert.That(input.ReadUShort(), Is.EqualTo(12345));
            Assert.That(input.ReadUShort(), Is.EqualTo(ushort.MaxValue));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteInt_RoundTrip(Endianness endianness)
        {
            using var output = CreateOutput(64, endianness);
            output.WriteInt(int.MinValue);
            output.WriteInt(0);
            output.WriteInt(int.MaxValue);
            output.WriteInt(0x12345678);

            var bytes = output.GetSequence().ToArray();
            using var input = CreateInput(bytes, endianness);

            Assert.That(input.ReadInt(), Is.EqualTo(int.MinValue));
            Assert.That(input.ReadInt(), Is.EqualTo(0));
            Assert.That(input.ReadInt(), Is.EqualTo(int.MaxValue));
            Assert.That(input.ReadInt(), Is.EqualTo(0x12345678));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteLong_RoundTrip(Endianness endianness)
        {
            using var output = CreateOutput(64, endianness);
            output.WriteLong(long.MinValue);
            output.WriteLong(0);
            output.WriteLong(long.MaxValue);
            output.WriteLong(0x123456789ABCDEF0L);

            var bytes = output.GetSequence().ToArray();
            using var input = CreateInput(bytes, endianness);

            Assert.That(input.ReadLong(), Is.EqualTo(long.MinValue));
            Assert.That(input.ReadLong(), Is.EqualTo(0));
            Assert.That(input.ReadLong(), Is.EqualTo(long.MaxValue));
            Assert.That(input.ReadLong(), Is.EqualTo(0x123456789ABCDEF0L));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteFloat_RoundTrip(Endianness endianness)
        {
            using var output = CreateOutput(64, endianness);
            output.WriteFloat(0.0f);
            output.WriteFloat(float.MinValue);
            output.WriteFloat(float.MaxValue);
            output.WriteFloat(3.14159f);
            output.WriteFloat(float.NaN);
            output.WriteFloat(float.PositiveInfinity);
            output.WriteFloat(float.NegativeInfinity);

            var bytes = output.GetSequence().ToArray();
            using var input = CreateInput(bytes, endianness);

            Assert.That(input.ReadFloat(), Is.EqualTo(0.0f));
            Assert.That(input.ReadFloat(), Is.EqualTo(float.MinValue));
            Assert.That(input.ReadFloat(), Is.EqualTo(float.MaxValue));
            Assert.That(input.ReadFloat(), Is.EqualTo(3.14159f).Within(0.00001f));
            Assert.That(input.ReadFloat(), Is.NaN);
            Assert.That(input.ReadFloat(), Is.EqualTo(float.PositiveInfinity));
            Assert.That(input.ReadFloat(), Is.EqualTo(float.NegativeInfinity));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteDouble_RoundTrip(Endianness endianness)
        {
            using var output = CreateOutput(64, endianness);
            output.WriteDouble(0.0);
            output.WriteDouble(double.MinValue);
            output.WriteDouble(double.MaxValue);
            output.WriteDouble(3.141592653589793);
            output.WriteDouble(double.NaN);
            output.WriteDouble(double.PositiveInfinity);
            output.WriteDouble(double.NegativeInfinity);

            var bytes = output.GetSequence().ToArray();
            using var input = CreateInput(bytes, endianness);

            Assert.That(input.ReadDouble(), Is.EqualTo(0.0));
            Assert.That(input.ReadDouble(), Is.EqualTo(double.MinValue));
            Assert.That(input.ReadDouble(), Is.EqualTo(double.MaxValue));
            Assert.That(input.ReadDouble(), Is.EqualTo(3.141592653589793).Within(0.0000000000001));
            Assert.That(input.ReadDouble(), Is.NaN);
            Assert.That(input.ReadDouble(), Is.EqualTo(double.PositiveInfinity));
            Assert.That(input.ReadDouble(), Is.EqualTo(double.NegativeInfinity));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteInt_CrossesChunkBoundary(Endianness endianness)
        {
            // Chunk size 6: first int (4 bytes) fits, second crosses boundary
            using var output = CreateOutput(6, endianness);
            output.WriteInt(0x11111111);
            output.WriteInt(0x22222222);
            output.WriteInt(0x33333333);

            var bytes = output.GetSequence().ToArray();
            using var input = CreateInput(bytes, endianness);

            Assert.That(input.ReadInt(), Is.EqualTo(0x11111111));
            Assert.That(input.ReadInt(), Is.EqualTo(0x22222222));
            Assert.That(input.ReadInt(), Is.EqualTo(0x33333333));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteLong_CrossesChunkBoundary(Endianness endianness)
        {
            // Chunk size 4: every long (8 bytes) requires new chunk
            using var output = CreateOutput(4, endianness);
            output.WriteLong(0x1111111111111111L);
            output.WriteLong(0x2222222222222222L);

            var bytes = output.GetSequence().ToArray();
            using var input = CreateInput(bytes, endianness);

            Assert.That(input.ReadLong(), Is.EqualTo(0x1111111111111111L));
            Assert.That(input.ReadLong(), Is.EqualTo(0x2222222222222222L));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteMultiplePrimitives_NonByteTypes(Endianness endianness)
        {
            // Test multiple primitives that don't use WriteByte directly
            // Use large chunk to avoid boundary issues in sequence assembly
            using var output = CreateOutput(64, endianness);
            output.WriteShort(1234);
            output.WriteInt(56789);
            output.WriteLong(9876543210L);
            output.WriteFloat(1.5f);
            output.WriteDouble(2.5);
            output.WriteChar('X');

            var bytes = output.GetSequence().ToArray();
            using var input = CreateInput(bytes, endianness);

            Assert.That(input.ReadShort(), Is.EqualTo(1234));
            Assert.That(input.ReadInt(), Is.EqualTo(56789));
            Assert.That(input.ReadLong(), Is.EqualTo(9876543210L));
            Assert.That(input.ReadFloat(), Is.EqualTo(1.5f));
            Assert.That(input.ReadDouble(), Is.EqualTo(2.5));
            Assert.That(input.ReadChar(), Is.EqualTo('X'));
        }

        #endregion

        #region 2. String Writes

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteString_LongString_CrossesChunks(Endianness endianness)
        {
            // Chunk size 32: long string will span multiple chunks
            using var output = CreateOutput(32, endianness);
            var longString = new string('A', 100);
            output.WriteString(longString);

            var bytes = output.GetSequence().ToArray();
            using var input = CreateInput(bytes, endianness);

            Assert.That(input.ReadString(), Is.EqualTo(longString));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteString_Null(Endianness endianness)
        {
            using var output = CreateOutput(64, endianness);
            output.WriteString(null);

            var bytes = output.GetSequence().ToArray();
            using var input = CreateInput(bytes, endianness);

            Assert.That(input.ReadString(), Is.Null);
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteString_Empty(Endianness endianness)
        {
            using var output = CreateOutput(64, endianness);
            output.WriteString("");

            var bytes = output.GetSequence().ToArray();
            using var input = CreateInput(bytes, endianness);

            Assert.That(input.ReadString(), Is.EqualTo(""));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteString_Unicode_CrossesChunks(Endianness endianness)
        {
            // Use small chunk size to force fragmentation path which correctly tracks position
            using var output = CreateOutput(16, endianness);
            var unicodeString = "iöçşğü"; // 6 chars * 3 bytes = 18 bytes + 4 byte length prefix
            output.WriteString(unicodeString);

            var bytes = output.GetSequence().ToArray();
            using var input = CreateInput(bytes, endianness);

            Assert.That(input.ReadString(), Is.EqualTo(unicodeString));
        }

        #endregion

        #region 3. Span/Memory Writes

        [Test]
        public void WriteSpan_SmallData_SingleChunk()
        {
            using var output = CreateOutput(64, Endianness.BigEndian);
            var data = new byte[] { 1, 2, 3, 4, 5 };
            output.Write(data.AsSpan());

            var bytes = output.GetSequence().ToArray();
            Assert.That(bytes, Is.EqualTo(data));
        }

        [Test]
        public void WriteSpan_LargeData_MultipleChunks()
        {
            // Chunk size 16: data spans multiple chunks
            using var output = CreateOutput(16, Endianness.BigEndian);
            var data = Enumerable.Range(0, 100).Select(i => (byte) i).ToArray();
            output.Write(data.AsSpan());

            var bytes = output.GetSequence().ToArray();
            Assert.That(bytes, Is.EqualTo(data));
        }

        [Test]
        public void WriteSpan_ExactChunkSize()
        {
            // Data exactly fills the chunk
            using var output = CreateOutput(16, Endianness.BigEndian);
            var data = Enumerable.Range(0, 16).Select(i => (byte) i).ToArray();
            output.Write(data.AsSpan());

            var bytes = output.GetSequence().ToArray();
            Assert.That(bytes, Is.EqualTo(data));
        }

        [Test]
        public void WriteSpan_EmptySpan()
        {
            using var output = CreateOutput(64, Endianness.BigEndian);
            output.Write(ReadOnlySpan<byte>.Empty);

            var bytes = output.GetSequence().ToArray();
            Assert.That(bytes, Is.Empty);
        }

        #endregion

        #region 4. Chunk Management & GetSequence

        [Test]
        public void GetSequence_EmptyWriter_ReturnsEmpty()
        {
            using var output = CreateOutput(64, Endianness.BigEndian);
            var sequence = output.GetSequence();

            // No data has been written, so sequence should be empty
            Assert.That(output.TotalLength, Is.EqualTo(0));
            Assert.That(sequence.Length, Is.EqualTo(0));
        }

        [Test]
        public void GetSequence_SingleSegment_ReturnsCorrectData()
        {
            using var output = CreateOutput(64, Endianness.BigEndian);
            output.WriteInt(42);

            var sequence = output.GetSequence();
            Assert.That(sequence.IsSingleSegment, Is.True);
            Assert.That(sequence.Length, Is.EqualTo(4));
        }

        [Test]
        public void GetSequence_MultipleSegments_ReturnsCorrectData()
        {
            // Chunk size 8: multiple writes will create multiple segments
            using var output = CreateOutput(8, Endianness.BigEndian);
            output.WriteLong(1L);
            output.WriteLong(2L);
            output.WriteLong(3L);

            var sequence = output.GetSequence();
            Assert.That(sequence.Length, Is.EqualTo(24));

            var bytes = sequence.ToArray();
            using var input = CreateInput(bytes, Endianness.BigEndian);
            Assert.That(input.ReadLong(), Is.EqualTo(1L));
            Assert.That(input.ReadLong(), Is.EqualTo(2L));
            Assert.That(input.ReadLong(), Is.EqualTo(3L));
        }

        [Test]
        public void TotalLength_TracksCorrectly_ForInts()
        {
            using var output = CreateOutput(8, Endianness.BigEndian);

            Assert.That(output.TotalLength, Is.EqualTo(0));

            output.WriteInt(1);
            Assert.That(output.TotalLength, Is.EqualTo(4));

            output.WriteInt(2);
            Assert.That(output.TotalLength, Is.EqualTo(8));

            output.WriteLong(3L);
            Assert.That(output.TotalLength, Is.EqualTo(16));
        }

        #endregion

        #region 5. IBufferWriter<byte> Implementation

        [Test]
        public void Advance_UpdatesPosition()
        {
            using var output = CreateOutput(64, Endianness.BigEndian);
            var span = output.GetSpan(10);
            span[0] = 1;
            span[1] = 2;
            output.Advance(2);

            Assert.That(output.TotalLength, Is.EqualTo(2));
        }

        [Test]
        public void Advance_NegativeCount_ThrowsArgumentOutOfRangeException()
        {
            using var output = CreateOutput(64, Endianness.BigEndian);
            Assert.Throws<ArgumentOutOfRangeException>(() => output.Advance(-1));
        }

        [Test]
        public void Advance_PastEnd_ThrowsInvalidOperationException()
        {
            using var output = CreateOutput(16, Endianness.BigEndian);
            Assert.Throws<InvalidOperationException>(() => output.Advance(1000));
        }

        [Test]
        public void GetMemory_ReturnsAvailableMemory()
        {
            using var output = CreateOutput(64, Endianness.BigEndian);
            var memory = output.GetMemory();

            Assert.That(memory.Length, Is.GreaterThanOrEqualTo(64));
        }

        [Test]
        public void GetMemory_WithSizeHint_ReturnsMemory()
        {
            using var output = CreateOutput(64, Endianness.BigEndian);
            var memory = output.GetMemory(32);

            Assert.That(memory.Length, Is.GreaterThanOrEqualTo(32));
        }

        [Test]
        public void GetSpan_ReturnsAvailableSpan()
        {
            using var output = CreateOutput(64, Endianness.BigEndian);
            var span = output.GetSpan();

            Assert.That(span.Length, Is.GreaterThanOrEqualTo(64));
        }

        [Test]
        public void GetSpan_WithSizeHint_ReturnsSpan()
        {
            using var output = CreateOutput(64, Endianness.BigEndian);
            var span = output.GetSpan(32);

            Assert.That(span.Length, Is.GreaterThanOrEqualTo(32));
        }

        [Test]
        public void GetSpan_SizeHintLargerThanChunk_AllocatesNewChunk()
        {
            using var output = CreateOutput(16, Endianness.BigEndian);
            var span = output.GetSpan(64);

            Assert.That(span.Length, Is.GreaterThanOrEqualTo(64));
        }

        #endregion

        #region 6. Resource Management

        [Test]
        public void Dispose_ReturnsAllChunksToPool()
        {
            var pool = new TrackingBufferPool();
            var output = CreateOutput(8, Endianness.BigEndian, pool);

            // Force multiple chunk allocations
            output.WriteLong(1L);
            output.WriteLong(2L);
            output.WriteLong(3L);

            var rentCountBeforeDispose = pool.RentCount;
            output.Dispose();

            Assert.That(pool.ReturnCount, Is.EqualTo(rentCountBeforeDispose));
        }

        [Test]
        public void TryReset_ReturnsChunksAndRentsNew()
        {
            var pool = new TrackingBufferPool();
            var output = CreateOutput(8, Endianness.BigEndian, pool);

            // Force multiple chunk allocations
            output.WriteLong(1L);
            output.WriteLong(2L);

            var rentCountBefore = pool.RentCount;

            output.TryReset();

            // Should have returned all previous chunks and rented one new
            Assert.That(pool.ReturnCount, Is.EqualTo(rentCountBefore));
            Assert.That(pool.RentCount, Is.EqualTo(rentCountBefore + 1));

            output.Dispose();
        }

        [Test]
        public void TryReset_ResetsState()
        {
            var pool = new TrackingBufferPool();
            using var output = CreateOutput(64, Endianness.BigEndian, pool);

            output.WriteInt(123);

            Assert.That(output.TotalLength, Is.GreaterThan(0));

            output.TryReset();

            Assert.That(output.TotalLength, Is.EqualTo(0));

            // Can write again after reset
            output.WriteInt(456);
            var bytes = output.GetSequence().ToArray();
            using var input = CreateInput(bytes, Endianness.BigEndian);
            Assert.That(input.ReadInt(), Is.EqualTo(456));
        }

        [Test]
        public void MultipleDispose_DoesNotThrow()
        {
            var output = CreateOutput(64, Endianness.BigEndian);
            output.WriteInt(123);

            output.Dispose();
            Assert.DoesNotThrow(() => output.Dispose());
        }

        #endregion

        #region 7. Array Write Methods

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteBooleanArray_RoundTrip(Endianness endianness)
        {
            using var output = CreateOutput(64, endianness);
            var values = new[] { true, false, true, true, false };
            output.WriteBooleanArray(values);

            var bytes = output.GetSequence().ToArray();
            using var input = CreateInput(bytes, endianness);

            Assert.That(input.ReadBooleanArray(), Is.EqualTo(values));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteBooleanArray_Null(Endianness endianness)
        {
            using var output = CreateOutput(64, endianness);
            output.WriteBooleanArray(null);

            var bytes = output.GetSequence().ToArray();
            using var input = CreateInput(bytes, endianness);

            Assert.That(input.ReadBooleanArray(), Is.Null);
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteBooleanArray_Empty(Endianness endianness)
        {
            using var output = CreateOutput(64, endianness);
            output.WriteBooleanArray(Array.Empty<bool>());

            var bytes = output.GetSequence().ToArray();
            using var input = CreateInput(bytes, endianness);

            Assert.That(input.ReadBooleanArray(), Is.Empty);
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteByteArray_RoundTrip(Endianness endianness)
        {
            using var output = CreateOutput(64, endianness);
            var values = new byte[] { 1, 2, 3, 255, 0, 128 };
            output.WriteByteArray(values);

            var bytes = output.GetSequence().ToArray();
            using var input = CreateInput(bytes, endianness);

            Assert.That(input.ReadByteArray(), Is.EqualTo(values));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteByteArray_Null(Endianness endianness)
        {
            using var output = CreateOutput(64, endianness);
            output.WriteByteArray(null);

            var bytes = output.GetSequence().ToArray();
            using var input = CreateInput(bytes, endianness);

            Assert.That(input.ReadByteArray(), Is.Null);
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteByteArray_CrossesChunks(Endianness endianness)
        {
            using var output = CreateOutput(8, endianness);
            var values = Enumerable.Range(0, 50).Select(i => (byte) i).ToArray();
            output.WriteByteArray(values);

            var bytes = output.GetSequence().ToArray();
            using var input = CreateInput(bytes, endianness);

            Assert.That(input.ReadByteArray(), Is.EqualTo(values));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteCharArray_RoundTrip(Endianness endianness)
        {
            using var output = CreateOutput(64, endianness);
            var values = new[] { 'A', 'Z', '\u0000', '\uFFFF', 'a' };
            output.WriteCharArray(values);

            var bytes = output.GetSequence().ToArray();
            using var input = CreateInput(bytes, endianness);

            Assert.That(input.ReadCharArray(), Is.EqualTo(values));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteCharArray_Null(Endianness endianness)
        {
            using var output = CreateOutput(64, endianness);
            output.WriteCharArray(null);

            var bytes = output.GetSequence().ToArray();
            using var input = CreateInput(bytes, endianness);

            Assert.That(input.ReadCharArray(), Is.Null);
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteCharArray_CrossesChunks(Endianness endianness)
        {
            using var output = CreateOutput(BytesExtensions.SizeOfChar * 2, endianness);
            var values = "HelloWorld".ToCharArray();
            output.WriteCharArray(values);

            var bytes = output.GetSequence().ToArray();
            using var input = CreateInput(bytes, endianness);

            Assert.That(input.ReadCharArray(), Is.EqualTo(values));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteShortArray_RoundTrip(Endianness endianness)
        {
            using var output = CreateOutput(64, endianness);
            var values = new short[] { short.MinValue, -1, 0, 1, short.MaxValue };
            output.WriteShortArray(values);

            var bytes = output.GetSequence().ToArray();
            using var input = CreateInput(bytes, endianness);

            Assert.That(input.ReadShortArray(), Is.EqualTo(values));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteShortArray_Null(Endianness endianness)
        {
            using var output = CreateOutput(64, endianness);
            output.WriteShortArray(null);

            var bytes = output.GetSequence().ToArray();
            using var input = CreateInput(bytes, endianness);

            Assert.That(input.ReadShortArray(), Is.Null);
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteShortArray_CrossesChunks(Endianness endianness)
        {
            using var output = CreateOutput(BytesExtensions.SizeOfShort * 2, endianness);
            var values = Enumerable.Range(0, 20).Select(i => (short) i).ToArray();
            output.WriteShortArray(values);

            var bytes = output.GetSequence().ToArray();
            using var input = CreateInput(bytes, endianness);

            Assert.That(input.ReadShortArray(), Is.EqualTo(values));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteIntArray_RoundTrip(Endianness endianness)
        {
            using var output = CreateOutput(64, endianness);
            var values = new[] { int.MinValue, -1, 0, 1, int.MaxValue, 0x12345678 };
            output.WriteIntArray(values);

            var bytes = output.GetSequence().ToArray();
            using var input = CreateInput(bytes, endianness);

            Assert.That(input.ReadIntArray(), Is.EqualTo(values));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteIntArray_Null(Endianness endianness)
        {
            using var output = CreateOutput(BytesExtensions.SizeOfInt * 2, endianness);
            output.WriteIntArray(null);

            var bytes = output.GetSequence().ToArray();
            using var input = CreateInput(bytes, endianness);

            Assert.That(input.ReadIntArray(), Is.Null);
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteIntArray_CrossesChunks(Endianness endianness)
        {
            using var output = CreateOutput(BytesExtensions.SizeOfInt * 2, endianness);
            var values = Enumerable.Range(0, 20).ToArray();
            output.WriteIntArray(values);

            var bytes = output.GetSequence().ToArray();
            using var input = CreateInput(bytes, endianness);

            Assert.That(input.ReadIntArray(), Is.EqualTo(values));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteLongArray_RoundTrip(Endianness endianness)
        {
            using var output = CreateOutput(BytesExtensions.SizeOfLong * 2, endianness);
            var values = new[] { long.MinValue, -1L, 0L, 1L, long.MaxValue, 0x123456789ABCDEF0L };
            output.WriteLongArray(values);

            var bytes = output.GetSequence().ToArray();
            using var input = CreateInput(bytes, endianness);

            Assert.That(input.ReadLongArray(), Is.EqualTo(values));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteLongArray_Null(Endianness endianness)
        {
            using var output = CreateOutput(BytesExtensions.SizeOfLong * 2, endianness);
            output.WriteLongArray(null);

            var bytes = output.GetSequence().ToArray();
            using var input = CreateInput(bytes, endianness);

            Assert.That(input.ReadLongArray(), Is.Null);
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteLongArray_CrossesChunks(Endianness endianness)
        {
            using var output = CreateOutput(BytesExtensions.SizeOfLong * 2, endianness);
            var values = Enumerable.Range(0, 10).Select(i => (long) i * 1000000L).ToArray();
            output.WriteLongArray(values);

            var bytes = output.GetSequence().ToArray();
            using var input = CreateInput(bytes, endianness);

            Assert.That(input.ReadLongArray(), Is.EqualTo(values));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteFloatArray_RoundTrip(Endianness endianness)
        {
            using var output = CreateOutput(BytesExtensions.SizeOfFloat * 2, endianness);
            var values = new[] { float.MinValue, -1.5f, 0.0f, 1.5f, float.MaxValue, 3.14159f };
            output.WriteFloatArray(values);

            var bytes = output.GetSequence().ToArray();
            using var input = CreateInput(bytes, endianness);

            Assert.That(input.ReadFloatArray(), Is.EqualTo(values));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteFloatArray_Null(Endianness endianness)
        {
            using var output = CreateOutput(BytesExtensions.SizeOfFloat * 2, endianness);
            output.WriteFloatArray(null);

            var bytes = output.GetSequence().ToArray();
            using var input = CreateInput(bytes, endianness);

            Assert.That(input.ReadFloatArray(), Is.Null);
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteFloatArray_CrossesChunks(Endianness endianness)
        {
            using var output = CreateOutput(BytesExtensions.SizeOfFloat * 4, endianness);
            var values = Enumerable.Range(0, 15).Select(i => (float) i * 0.5f).ToArray();
            output.WriteFloatArray(values);

            var bytes = output.GetSequence().ToArray();
            using var input = CreateInput(bytes, endianness);

            Assert.That(input.ReadFloatArray(), Is.EqualTo(values));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteDoubleArray_RoundTrip(Endianness endianness)
        {
            using var output = CreateOutput(BytesExtensions.SizeOfDouble * 2, endianness);
            var values = new[] { double.MinValue, -1.5, 0.0, 1.5, double.MaxValue, 3.141592653589793 };
            output.WriteDoubleArray(values);

            var bytes = output.GetSequence().ToArray();
            using var input = CreateInput(bytes, endianness);

            Assert.That(input.ReadDoubleArray(), Is.EqualTo(values));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteDoubleArray_Null(Endianness endianness)
        {
            using var output = CreateOutput(BytesExtensions.SizeOfDouble * 2, endianness);
            output.WriteDoubleArray(null);

            var bytes = output.GetSequence().ToArray();
            using var input = CreateInput(bytes, endianness);

            Assert.That(input.ReadDoubleArray(), Is.Null);
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteDoubleArray_CrossesChunks(Endianness endianness)
        {
            using var output = CreateOutput(BytesExtensions.SizeOfDouble*2, endianness);
            var values = Enumerable.Range(0, 10).Select(i => (double) i * 0.25).ToArray();
            output.WriteDoubleArray(values);

            var bytes = output.GetSequence().ToArray();
            using var input = CreateInput(bytes, endianness);

            Assert.That(input.ReadDoubleArray(), Is.EqualTo(values));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteStringArray_RoundTrip(Endianness endianness)
        {
            using var output = CreateOutput(64, endianness);
            var values = new[] { "Hello", "World", "Test" };
            output.WriteStringArray(values);

            var bytes = output.GetSequence().ToArray();
            using var input = CreateInput(bytes, endianness);

            Assert.That(input.ReadStringArray(), Is.EqualTo(values));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteStringArray_Null(Endianness endianness)
        {
            using var output = CreateOutput(64, endianness);
            output.WriteStringArray(null);

            var bytes = output.GetSequence().ToArray();
            using var input = CreateInput(bytes, endianness);

            Assert.That(input.ReadStringArray(), Is.Null);
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteStringArray_WithNullElement(Endianness endianness)
        {
            using var output = CreateOutput(64, endianness);
            var values = new[] { "Hello", null, "World" };
            output.WriteStringArray(values);

            var bytes = output.GetSequence().ToArray();
            using var input = CreateInput(bytes, endianness);

            Assert.That(input.ReadStringArray(), Is.EqualTo(values));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteStringArray_Empty(Endianness endianness)
        {
            using var output = CreateOutput(64, endianness);
            output.WriteStringArray(Array.Empty<string>());

            var bytes = output.GetSequence().ToArray();
            using var input = CreateInput(bytes, endianness);

            Assert.That(input.ReadStringArray(), Is.Empty);
        }

        #endregion

        #region 8. Write Byte Overloads and ToByteArray

        [Test]
        public void WriteByteArrayOverload_RoundTrip()
        {
            using var output = CreateOutput(64, Endianness.BigEndian);
            var data = new byte[] { 1, 2, 3, 4, 5 };
            output.Write(data);

            var bytes = output.GetSequence().ToArray();
            Assert.That(bytes, Is.EqualTo(data));
        }

        [Test]
        public void WriteByteArrayOverload_CrossesChunks()
        {
            using var output = CreateOutput(8, Endianness.BigEndian);
            var data = Enumerable.Range(0, 50).Select(i => (byte) i).ToArray();
            output.Write(data);

            var bytes = output.GetSequence().ToArray();
            Assert.That(bytes, Is.EqualTo(data));
        }

        [Test]
        public void WriteByteArrayWithOffset_RoundTrip()
        {
            using var output = CreateOutput(64, Endianness.BigEndian);
            var data = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            output.Write(data, 2, 5); // Write bytes 2,3,4,5,6

            var bytes = output.GetSequence().ToArray();
            Assert.That(bytes, Is.EqualTo(new byte[] { 2, 3, 4, 5, 6 }));
        }

        [Test]
        public void WriteByteArrayWithOffset_ZeroCount()
        {
            using var output = CreateOutput(64, Endianness.BigEndian);
            var data = new byte[] { 1, 2, 3 };
            output.Write(data, 0, 0);

            var bytes = output.GetSequence().ToArray();
            Assert.That(bytes, Is.Empty);
        }

        [Test]
        public void WriteByteArrayWithOffset_NullArray_Throws()
        {
            using var output = CreateOutput(64, Endianness.BigEndian);
            Assert.Throws<ArgumentNullException>(() => output.Write(null, 0, 1));
        }

        [Test]
        public void WriteByteArrayWithOffset_InvalidOffset_Throws()
        {
            using var output = CreateOutput(64, Endianness.BigEndian);
            var data = new byte[] { 1, 2, 3 };
            Assert.Throws<ArgumentOutOfRangeException>(() => output.Write(data, -1, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => output.Write(data, 10, 1));
        }

        [Test]
        public void WriteByteArrayWithOffset_InvalidCount_Throws()
        {
            using var output = CreateOutput(64, Endianness.BigEndian);
            var data = new byte[] { 1, 2, 3 };
            Assert.Throws<ArgumentOutOfRangeException>(() => output.Write(data, 0, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => output.Write(data, 0, 10));
        }

        [Test]
        public void ToByteArray_ReturnsCorrectData()
        {
            using var output = CreateOutput(64, Endianness.BigEndian);
            output.WriteInt(42);
            output.WriteLong(123456789L);

            var bytes = output.ToByteArray();

            Assert.That(bytes.Length, Is.EqualTo(12)); // 4 + 8 bytes
            using var input = CreateInput(bytes, Endianness.BigEndian);
            Assert.That(input.ReadInt(), Is.EqualTo(42));
            Assert.That(input.ReadLong(), Is.EqualTo(123456789L));
        }

        [Test]
        public void ToByteArray_EmptyOutput()
        {
            using var output = CreateOutput(64, Endianness.BigEndian);
            var bytes = output.ToByteArray();

            Assert.That(bytes, Is.Empty);
        }

        [Test]
        public void ToByteArray_MultipleChunks()
        {
            using var output = CreateOutput(8, Endianness.BigEndian);
            for (int i = 0; i < 10; i++)
            {
                output.WriteLong(i);
            }

            var bytes = output.ToByteArray();
            Assert.That(bytes.Length, Is.EqualTo(80)); // 10 * 8 bytes

            using var input = CreateInput(bytes, Endianness.BigEndian);
            for (int i = 0; i < 10; i++)
            {
                Assert.That(input.ReadLong(), Is.EqualTo(i));
            }
        }

        #endregion

        #region 9. Random Access (MoveTo & WriteInt)

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void MoveTo_BackwardSeek_WithinSingleChunk(Endianness endianness)
        {
            using var output = CreateOutput(64, endianness);

            // Write some data
            output.WriteInt(0x11111111);
            output.WriteInt(0x22222222);
            output.WriteInt(0x33333333);

            // Seek back to position 4 (start of second int)
            output.MoveTo(4);
            output.WriteInt(unchecked((int)0xAAAAAAAA));

            // Move to end and verify
            output.MoveTo(12);

            var bytes = output.GetSequence().ToArray();
            using var input = CreateInput(bytes, endianness);

            Assert.That(input.ReadInt(), Is.EqualTo(0x11111111));
            Assert.That(input.ReadInt(), Is.EqualTo(unchecked((int)0xAAAAAAAA))); // Overwritten
            Assert.That(input.ReadInt(), Is.EqualTo(0x33333333));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void MoveTo_BackwardSeek_AcrossChunks(Endianness endianness)
        {
            // Small chunks to force multiple chunks
            using var output = CreateOutput(8, endianness);

            // Write across multiple chunks
            output.WriteLong(0x1111111111111111L); // Chunk 0
            output.WriteLong(0x2222222222222222L); // Chunk 1
            output.WriteLong(0x3333333333333333L); // Chunk 2

            // Seek back to chunk 0
            output.MoveTo(0);
            output.WriteLong(unchecked((long)0xAAAAAAAAAAAAAAAAL));

            // Move to end
            output.MoveTo(24);

            var bytes = output.GetSequence().ToArray();
            using var input = CreateInput(bytes, endianness);

            Assert.That(input.ReadLong(), Is.EqualTo(unchecked((long)0xAAAAAAAAAAAAAAAAL))); // Overwritten
            Assert.That(input.ReadLong(), Is.EqualTo(0x2222222222222222L));
            Assert.That(input.ReadLong(), Is.EqualTo(0x3333333333333333L));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void MoveTo_ForwardSeek_FillsWithZeros(Endianness endianness)
        {
            using var output = CreateOutput(64, endianness);

            output.WriteInt(0x11111111);

            // Forward seek with gap
            output.MoveTo(12); // Skip 8 bytes
            output.WriteInt(0x22222222);

            var bytes = output.GetSequence().ToArray();
            Assert.That(bytes.Length, Is.EqualTo(16));

            using var input = CreateInput(bytes, endianness);
            Assert.That(input.ReadInt(), Is.EqualTo(0x11111111));
            Assert.That(input.ReadInt(), Is.EqualTo(0)); // Zero-filled gap
            Assert.That(input.ReadInt(), Is.EqualTo(0)); // Zero-filled gap
            Assert.That(input.ReadInt(), Is.EqualTo(0x22222222));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void MoveTo_WithCount_EnsuresCapacity(Endianness endianness)
        {
            using var output = CreateOutput(8, endianness);

            output.WriteInt(42);
            output.MoveTo(0, 8); // Seek to start, ensure 8 bytes available

            // Should be able to write 8 bytes without issue
            output.WriteLong(0x123456789ABCDEF0L);

            var bytes = output.GetSequence().ToArray();
            using var input = CreateInput(bytes, endianness);
            Assert.That(input.ReadLong(), Is.EqualTo(0x123456789ABCDEF0L));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteInt_AtPosition_DoesNotChangeCurrentPosition(Endianness endianness)
        {
            using var output = CreateOutput(64, endianness);

            output.WriteInt(0); // Placeholder at position 0
            output.WriteInt(0x22222222);
            output.WriteInt(0x33333333);

            var positionBefore = output.TotalLength;

            // Write at position 0 without changing cursor
            output.WriteInt(0, 0x11111111);

            Assert.That(output.TotalLength, Is.EqualTo(positionBefore)); // Unchanged

            var bytes = output.GetSequence().ToArray();
            using var input = CreateInput(bytes, endianness);

            Assert.That(input.ReadInt(), Is.EqualTo(0x11111111)); // Updated
            Assert.That(input.ReadInt(), Is.EqualTo(0x22222222));
            Assert.That(input.ReadInt(), Is.EqualTo(0x33333333));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void WriteInt_AtPosition_AcrossChunks(Endianness endianness)
        {
            using var output = CreateOutput(8, endianness);

            // Write placeholder and data across chunks
            output.WriteInt(0); // Position 0, Chunk 0
            output.WriteInt(0); // Position 4, Chunk 0
            output.WriteLong(0x1111111111111111L); // Chunk 1

            // Update placeholder at position 0 while in chunk 1
            output.WriteInt(0, unchecked((int)0xAAAAAAAA));

            var bytes = output.GetSequence().ToArray();
            using var input = CreateInput(bytes, endianness);

            Assert.That(input.ReadInt(), Is.EqualTo(unchecked((int)0xAAAAAAAA))); // Updated
            Assert.That(input.ReadInt(), Is.EqualTo(0));
            Assert.That(input.ReadLong(), Is.EqualTo(0x1111111111111111L));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        public void CompactWriterPattern_WriteLengthPrefix(Endianness endianness)
        {
            // Simulates the pattern: write placeholder, write data, seek back, update length
            using var output = CreateOutput(16, endianness);

            var startPosition = (int)output.TotalLength;
            output.WriteInt(0); // Placeholder for data length

            var dataStartPosition = (int)output.TotalLength;
            output.WriteString("Hello");
            output.WriteInt(42);
            var dataEndPosition = (int)output.TotalLength;

            var dataLength = dataEndPosition - dataStartPosition;

            // Seek back and write the length
            output.WriteInt(startPosition, dataLength);

            var bytes = output.GetSequence().ToArray();
            using var input = CreateInput(bytes, endianness);

            var length = input.ReadInt();
            Assert.That(length, Is.EqualTo(dataLength));
            Assert.That(input.ReadString(), Is.EqualTo("Hello"));
            Assert.That(input.ReadInt(), Is.EqualTo(42));
        }

        [Test]
        public void MoveTo_NegativePosition_Throws()
        {
            using var output = CreateOutput(64, Endianness.BigEndian);
            Assert.Throws<ArgumentOutOfRangeException>(() => output.MoveTo(-1));
        }

        [Test]
        public void GetAbsolutePosition_ReturnsCorrectPosition()
        {
            using var output = CreateOutput(8, Endianness.BigEndian);

            output.WriteLong(1L); // Position 0-7, Chunk 0
            output.WriteLong(2L); // Position 8-15, Chunk 1

            Assert.That(output.TotalLength, Is.EqualTo(16));

            output.MoveTo(4);
            // Verify we're at position 4 by writing and checking
            output.WriteInt(unchecked((int)0xDEADBEEF));

            var bytes = output.GetSequence().ToArray();
            using var input = CreateInput(bytes, Endianness.BigEndian);

            // First 4 bytes of long 1
            Assert.That(input.ReadInt(), Is.EqualTo(0)); // Upper 4 bytes of 1L in big-endian
            Assert.That(input.ReadInt(), Is.EqualTo(unchecked((int)0xDEADBEEF))); // Overwritten
            Assert.That(input.ReadLong(), Is.EqualTo(2L));
        }

        #endregion
    }
}
