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
using System.Collections.Generic;
using Hazelcast.Core;
using Hazelcast.Models;
using Microsoft.Extensions.ObjectPool;

namespace Hazelcast.Serialization
{
    internal partial class ObjectDataOutput : IObjectDataOutput, ICanHaveSchemas, IDisposable, IResettable
    {
        private readonly int _initialBufferSize;
        private readonly IWriteObjectsToObjectDataOutput _objectsWriter;
        private byte[] _buffer;
        private int _position;
        private int _prefixBias; // bytes reserved at start of buffer via ReservePrefix
        private HashSet<long> _schemaIds;
        private IBufferPool _bufferPool;
        private bool _initialized = false;
        internal ObjectDataOutput(int initialBufferSize, IWriteObjectsToObjectDataOutput objectsReaderWriter, Endianness endianness, IBufferPool bufferPool)
        {
            _initialBufferSize = initialBufferSize;
            _bufferPool = bufferPool;
            _objectsWriter = objectsReaderWriter;
            Endianness = endianness;
            Initialize();
        }

        # region Memory Owner Ship

        /// <summary>
        /// Initializes the output, renting a buffer from the pool. This must be called before using the output.
        /// </summary>
        public void Initialize()
        {
            if (_initialized) return;

            _buffer = _bufferPool.Rent(_initialBufferSize);
            _initialized = true;
        }

        /// <summary>
        /// Advances the write position by <paramref name="bytes"/> without writing any data,
        /// reserving that many bytes at the current position for the caller to fill in later.
        /// Must be called immediately after <see cref="Initialize"/> and before any writes.
        /// Sets <see cref="PayloadPosition"/> so that serializers storing absolute byte positions
        /// in the binary (e.g. the portable serializer) record positions relative to the payload
        /// start rather than the raw buffer start.
        /// </summary>
        /// <param name="bytes">Number of bytes to reserve.</param>
        public void ReservePrefix(int bytes)
        {
            EnsureAvailable(bytes);
            _position += bytes;
            _prefixBias = bytes;
        }

        /// <summary>
        /// Gets the current write position relative to the payload start (i.e. excluding the
        /// bytes reserved by <see cref="ReservePrefix"/>). Use this — not <see cref="Position"/> —
        /// whenever a position value is being stored inside the serialized binary (e.g. as a field
        /// offset in the portable format), so that the stored value is valid when the payload is
        /// later read back without the prefix.
        /// </summary>
        internal int PayloadPosition => _position - _prefixBias;

        /// <summary>
        /// Detaches the buffer from the output, returning the written content as a sliced
        /// <see cref="Memory{T}"/> and the backing array for pool return. After this call,
        /// the output is no longer usable and should be reinitialized.
        /// </summary>
        /// <returns>A tuple of the sliced content and the raw backing buffer.</returns>
        public (Memory<byte> content, byte[] backingBuffer) DetachBuffer()
        {
            var buffer = _buffer;
            var length = _position;
            _buffer = null;
            _position = 0;
            _initialized = false;
            return (new Memory<byte>(buffer, 0, length), buffer);
        }

        # endregion

        public void Clear()
        {
            _position = 0;
            _prefixBias = 0;
            _schemaIds?.Clear();

            // If the buffer is still held (e.g. exception path before DetachBuffer),
            // just reset position and keep it — EnsureAvailable() will resize if needed.
            // Do NOT return + re-rent: that causes an unnecessary ArrayPool round-trip per Clear().
            //
            // If the buffer was already detached (normal PUT path), leave _buffer null.
            // EnsureAvailable(), called on the next write, will rent lazily — spreading
            // Rent() calls across operation-start time rather than operation-end time,
            // reducing ArrayPool contention under high concurrency.
        }

        public bool TryReset()
        {
            Clear();
            return true;
        }

        public void Dispose()
        {
            Position = 0;
            _bufferPool?.Return(_buffer);
            _buffer = null;
            _schemaIds = null;
            _initialized = false;
        }

        // TODO: Convert to Memory<byte> while refactoring ObjectDataInput
        public byte[] Buffer => _buffer ?? Array.Empty<byte>();

        internal int Position
        {
            get => _position;
            set => _position = value;
        }

        // no need for thread-safety because we don't parallel-serialize
        public HashSet<long> SchemaIds => _schemaIds ??= new HashSet<long>();

        public bool HasSchemas => _schemaIds != null;

        public void WriteInt(int position, int value)
        {
            EnsureAvailable(BytesExtensions.SizeOfInt);
            _buffer.WriteInt(position, value, Endianness);
        }

        public void WriteIntBigEndian(int value)
        {
            EnsureAvailable(BytesExtensions.SizeOfInt);
            _buffer.WriteInt(_position, value, Endianness.BigEndian);
            _position += BytesExtensions.SizeOfInt;
        }

        public void WriteBits(byte bits, byte mask)
        {
            EnsureAvailable(BytesExtensions.SizeOfByte);
            _buffer.WriteBits(_position, bits, mask);
            _position += BytesExtensions.SizeOfByte;
        }

        public void WriteZeroBytes(int count)
        {
            EnsureAvailable(count);
            for (var i = 0; i < count; i++)
            {
                _buffer.WriteByte(_position, 0);
                _position += BytesExtensions.SizeOfByte;
            }
        }


        public void MoveTo(int position, int count = 0)
        {
            Position = 0;
            EnsureAvailable(position + count);
            Position = position;
        }

        internal void EnsureAvailable(int count)
        {
            // TODO - input/output should work with memory and span and not copy arrays

            if (_buffer != null)
            {
                if (_buffer.Length - Position >= count) return;
                var newCap = Math.Max(_buffer.Length << 1, _buffer.Length + count);
                ResizeBuffer(newCap);
            }
            else
            {
                _buffer = _bufferPool.Rent(count > _initialBufferSize / 2 ? count * 2 : _initialBufferSize);
                _initialized = true;
            }
        }
        private void ResizeBuffer(int newCap)
        {
            var newBuffer = _bufferPool.Rent(newCap);
            _buffer.AsSpan(0, _position).CopyTo(newBuffer);
            _bufferPool.Return(_buffer);
            _buffer = newBuffer;
        }


    }
}
