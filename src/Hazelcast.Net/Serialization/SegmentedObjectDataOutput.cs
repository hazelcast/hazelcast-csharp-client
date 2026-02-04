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
using System.Collections.Generic;
using System.Text;
using Hazelcast.Core;
using Hazelcast.Models;
using Microsoft.Extensions.ObjectPool;
using Hazelcast.Polyfills;
namespace Hazelcast.Serialization
{
    /// <summary>
    /// A Zero-Copy, segmented buffer writer that manages a linked list of arrays
    /// rented from the ArrayPool. It eliminates large object allocations and buffer resizing.
    /// </summary>
    internal sealed partial class SegmentedObjectDataOutput : IObjectDataOutput, ICanHaveSchemas, IDisposable, IResettable, IBufferWriter<byte>
    {
        // Track all rented arrays so we can return them to the pool
        private readonly List<byte[]> _rentedArrays = new List<byte[]>();

        // Track committed length for each chunk (except current chunk which uses _positionInChunk)
        private readonly List<int> _committedLengths = new List<int>();

        private byte[] _currentChunk;
        private int _positionInChunk;
        private long _totalLength;
        private readonly int _minChunkSize;
        private HashSet<long> _schemaIds;
        private IBufferPool _bufferPool;
        private readonly IWriteObjectsToObjectDataOutput _objectsWriter;

        public SegmentedObjectDataOutput(int initialChunkSize, IWriteObjectsToObjectDataOutput objectsReaderWriter, Endianness endianness, IBufferPool bufferPool)
        {
            _bufferPool = bufferPool;
            _objectsWriter = objectsReaderWriter;
            Endianness = endianness;
            _minChunkSize = initialChunkSize;
            _currentChunk = _bufferPool.Rent(_minChunkSize);
            _rentedArrays.Add(_currentChunk);
        }
        public Endianness Endianness { get; }
        public long TotalLength => _totalLength;
        
        public int SizeLeftInCurrentChunk => _currentChunk.Length - _positionInChunk;

        /// <summary>
        /// Writes a block of memory using efficient Span slicing (Fill & Spill).
        /// Note: This method advances the position.
        /// </summary>
        public void Write(ReadOnlySpan<byte> source)
        {
            while (!source.IsEmpty)
            {
                // 1. Get available memory in the current chunk.
                //    If the current chunk is full, this rents a new one.
                var destination = GetMemory();

                // 2. Determine how many bytes we can copy in this iteration.
                //    We take the smaller of:
                //    A) The data remaining to be written (source.Length)
                //    B) The space remaining in the current chunk (destination.Length)
                var writeCount = Math.Min(source.Length, destination.Length);

                // 3. Do fast block copy (memcpy).
                source.Slice(0, writeCount).CopyTo(destination.Span.Slice(0, writeCount));

                // 4. Advance cursors.
                Advance(writeCount);

                // 5. Slice the source to remove the part we just wrote.
                source = source.Slice(writeCount);
            }
        }
        
        private void WriteFragmentedString(ReadOnlySpan<char> source)
        {
            var encoder = Encoding.UTF8.GetEncoder();
            var destination = GetSpan(2); // start with at least 2 bytes for single UTF-8 character
            var charSource = source;
            
            while (!charSource.IsEmpty)
            {
                // Write as much as possible into the current destination
                encoder.Convert(
                    charSource, 
                    destination, 
                    true, 
                    out var charsUsed, 
                    out var bytesUsed, 
                    out var completed
                );
                
                Advance(bytesUsed);
                
                charSource = charSource.Slice(charsUsed);

                if (charSource.IsEmpty) break;

                // We filled the current chunk. Request a new one.
                // We ask for at least 1 byte, which triggers the SegmentedWriter to rent a NEW chunk.
                destination = GetSpan(1); 
            }
        }

        /// <summary>
        /// Returns a ReadOnlySequence wrapping all the chunks.
        /// This is a lightweight view; no copying or assembly happens here.
        /// </summary>
        public ReadOnlySequence<byte> GetSequence()
        {
            if (_rentedArrays.Count == 0)
                return ReadOnlySequence<byte>.Empty;

            if (_rentedArrays.Count == 1)
            {
                // single segment
                return new ReadOnlySequence<byte>(_rentedArrays[0], 0, _positionInChunk);
            }

            // Multi-segment: use committed lengths for completed chunks
            var firstLength = _committedLengths[0];
            var firstSegment = new Segment(_rentedArrays[0], 0, firstLength);
            var lastSegment = firstSegment;

            for (int i = 1; i < _rentedArrays.Count; i++)
            {
                var isLast = i == _rentedArrays.Count - 1;
                var length = isLast ? _positionInChunk : _committedLengths[i];

                // Don't append empty last segments (if we just filled the previous one exactly)
                if (length == 0 && isLast) break;

                lastSegment = lastSegment.Append(_rentedArrays[i], length);
            }

            return new ReadOnlySequence<byte>(firstSegment, 0, lastSegment, lastSegment.Memory.Length);
        }
        
        /// <summary>
        /// Ensures we have a contiguous block of memory for a primitive write.
        /// If the current chunk doesn't have space, we move to a new one immediately.
        /// Note: This method advances the position.
        /// </summary>
        public Span<byte> GetSpanForPrimitive(int count)
        {
            // 1. Check if the current chunk has enough space
            if (_currentChunk != null && _currentChunk.Length - _positionInChunk >= count)
            {
                var span = _currentChunk.AsSpan(_positionInChunk, count);
                _positionInChunk += count;
                _totalLength += count;
                return span;
            }

            // 2. Not enough space -> Commit the current chunk as-is and get a new one.
            //    Note: The unused bytes at the end of the old chunk are simply ignored by the sequence
            AppendNewChunk(Math.Max(count, _minChunkSize));

            // 3. Return the start of the new chunk
            var newSpan = _currentChunk.AsSpan(0, count);
            _positionInChunk += count;
            _totalLength += count;
            return newSpan;
        }
  

        #region IBufferWriter<byte> Implementation

        public void Advance(int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (_positionInChunk + count > _currentChunk.Length)
                throw new InvalidOperationException("Cannot advance past the end of the current buffer.");

            _positionInChunk += count;
            _totalLength += count;
        }

        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            EnsureCapacity(sizeHint);
            // Returns a memory from the current position to the end of the current chunk
            return _currentChunk.AsMemory(_positionInChunk);
        }

        /// <summary>
        /// Gets a span of bytes from the current position, ensuring capacity.
        /// Note: This does not advance the position; call <see cref="Advance"/> after writing.
        /// </summary>
        public Span<byte> GetSpan(int sizeHint = 0)
        {
            EnsureCapacity(sizeHint);
            // Returns a span from the current position to the end of the current chunk
            return _currentChunk.AsSpan(_positionInChunk);
        }

        /// <summary>
        /// Skip to a new chunk directly, appending it to the list. Useful for primitive writes that need contiguous space.
        /// </summary>
        /// <param name="sizeHint">Calculate the size before calling it.</param>
        private void AppendNewChunk(int sizeHint)
        {
            // Commit the current chunk's used length before moving to a new one
            _committedLengths.Add(_positionInChunk);

            _currentChunk = _bufferPool.Rent(sizeHint);
            _rentedArrays.Add(_currentChunk);
            _positionInChunk = 0;
        }
        
        private void EnsureCapacity(int sizeHint)
        {
            if (sizeHint < 0) throw new ArgumentOutOfRangeException(nameof(sizeHint));
            if (sizeHint == 0) sizeHint = 1; // Always ensure at least 1 byte if requested

            // If we have space in the current chunk, do nothing
            if (_currentChunk.Length - _positionInChunk >= sizeHint)
                return;

            // Otherwise, rent a new chunk
            // We do not copy old data; we just move to the new buffer.
            var nextSize = Math.Max(sizeHint, _minChunkSize);
            AppendNewChunk(nextSize);
        }

        #endregion

        /// <summary>
        /// Returns all rented arrays to the pool.
        /// </summary>
        public void Dispose()
        {
            foreach (var buffer in _rentedArrays)
            {
                _bufferPool.Return(buffer);
            }
            _rentedArrays.Clear();
            _committedLengths.Clear();
            _currentChunk = null;
            _totalLength = 0;
        }

        /// <summary>
        /// Resets the writer to initial state (to back in to object pool), returning all rented arrays to the pool.
        /// </summary>
        public bool TryReset()
        {
            Dispose();
            _currentChunk = _bufferPool.Rent(_minChunkSize);
            _rentedArrays.Add(_currentChunk);
            // No committed length for the first chunk - it uses _positionInChunk
            _positionInChunk = 0;
            _totalLength = 0;
            return true;
        }
        
        private static void CastAndCopyCharsToBytes(ReadOnlySpan<char> source, Span<byte> destination)
        {
            for (int i = 0; i < source.Length; i++)
            {
                destination[i] = (byte)source[i];
            }
        }

        /// <summary>
        /// Linked-list wrapper of chunks into a ReadOnlySequence.
        /// </summary>
        private sealed class Segment : ReadOnlySequenceSegment<byte>
        {
            public Segment(byte[] array, int start, int length)
            {
                Memory = new ReadOnlyMemory<byte>(array, start, length);
                RunningIndex = 0;
            }

            public Segment Append(byte[] array, int length)
            {
                var next = new Segment(array, 0, length)
                {
                    RunningIndex = RunningIndex + Memory.Length
                };
                Next = next;
                return next;
            }
        }
    }
}
