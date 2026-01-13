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
using Hazelcast.Core;
using Hazelcast.Models;

namespace Hazelcast.Serialization
{
    internal partial class ObjectDataOutput : IObjectDataOutput, ICanHaveSchemas, IDisposable
    {
        private readonly int _initialBufferSize;
        private readonly IWriteObjectsToObjectDataOutput _objectsWriter;
        private byte[] _buffer;
        private int _position;
        private HashSet<long> _schemaIds;

        internal ObjectDataOutput(int initialBufferSize, IWriteObjectsToObjectDataOutput objectsReaderWriter, Endianness endianness)
        {
            _initialBufferSize = initialBufferSize;
            _buffer = AllocateBuffer(_initialBufferSize);
            _objectsWriter = objectsReaderWriter;
            Endianness = endianness;
        }

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

        public void Clear()
        {
            Position = 0;
            if (_buffer != null && _buffer.Length > _initialBufferSize * 8)
            {
                Dispose();
                _buffer = AllocateBuffer(_initialBufferSize * 8);
            }
        }

        public void Dispose()
        {
            Position = 0;
#if NETSTANDARD2_0            
            _buffer = null;
#elif  NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
            if(_buffer != null)
                ArrayPool<byte>.Shared.Return(_buffer);
            _buffer = null;
#endif
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
                _buffer = AllocateBuffer(count > _initialBufferSize / 2 ? count * 2 : _initialBufferSize);
            }
        }
        private void ResizeBuffer(int newCap)
        {
#if NETSTANDARD2_0                
                Array.Resize(ref _buffer, newCap);
#elif NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
            var newBuffer = AllocateBuffer(newCap);
            
            _buffer.AsSpan().CopyTo(newBuffer);
                
            ArrayPool<byte>.Shared.Return(_buffer);
            _buffer = newBuffer;
#endif
        }
        private byte[] AllocateBuffer(int size)
        {
#if NETSTANDARD2_0            
            return new byte[size];
#elif NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
            return ArrayPool<byte>.Shared.Rent(size);
#endif
        }
    }
}

