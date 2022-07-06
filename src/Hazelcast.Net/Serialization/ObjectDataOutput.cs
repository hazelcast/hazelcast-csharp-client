// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Serialization
{
    internal partial class ObjectDataOutput : IObjectDataOutput, IDisposable
    {
        private readonly int _initialBufferSize;
        private readonly SerializationService _serializationService;
        private byte[] _buffer;
        private int _position;

        internal ObjectDataOutput(int initialBufferSize, SerializationService serializationService, Endianness endianness)
        {
            _initialBufferSize = initialBufferSize;
            _buffer = new byte[_initialBufferSize];
            _serializationService = serializationService;
            Endianness = endianness;
        }

        public byte[] Buffer
        {
            get => _buffer;
            set => _buffer = value;
        }

        internal int Position
        {
            get => _position;
            private set => _position = value;
        }

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
                _buffer = new byte[_initialBufferSize * 8];
            }
        }

        public void Dispose()
        {
            Position = 0;
            _buffer = null;
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
                var newBuffer = new byte[newCap];
                System.Buffer.BlockCopy(_buffer, 0, newBuffer, 0, _buffer.Length);
                _buffer = newBuffer;
            }
            else
            {
                _buffer = new byte[count > _initialBufferSize / 2 ? count * 2 : _initialBufferSize];
            }
        }
    }
}
