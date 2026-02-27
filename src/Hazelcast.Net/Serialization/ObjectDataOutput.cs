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
        /// Detaches the buffer from the output, returning it to the caller. After this call,
        /// the output is no longer usable and should be reinitialized.
        /// </summary>
        /// <returns>buffer</returns>
        public byte[] DetachBuffer()
        {
            var buffer = _buffer;
            _buffer = null;
            _position = 0;
            _initialized = false;
            return buffer;
        }

        # endregion

        public void Clear()
        {
            Position = 0;
            _schemaIds?.Clear();
            
            // If not detached, return the buffer to the pool and detach it.
            // We don't want a buffer to be shared between outputs, but we want to reuse it if the output is reused.
            if (_initialized)
            {
                _bufferPool.Return(_buffer);
                DetachBuffer();
            }
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


        // TODO: Convert to Memory<byte> and Span<byte> and avoid copying arrays
        public byte[] Buffer
        {
            get => _buffer;
        }

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
            }
        }
        private void ResizeBuffer(int newCap)
        {
            var newBuffer = _bufferPool.Rent(newCap);
            _buffer.AsSpan(0, _buffer.Length).CopyTo(newBuffer);
            _bufferPool.Return(_buffer);
            _buffer = newBuffer;
        }


    }
}
