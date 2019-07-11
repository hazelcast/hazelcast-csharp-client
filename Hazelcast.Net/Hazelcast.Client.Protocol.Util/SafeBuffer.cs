// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
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
using System.Text;
using Hazelcast.IO;

namespace Hazelcast.Client.Protocol.Util
{
    /// <summary>Implementation of IClientProtocolBuffer that is used by default in clients.</summary>
    /// <remarks>
    ///     Implementation of IClientProtocolBuffer that is used by default in clients.
    ///     There is another unsafe implementation which is configurable .
    /// </remarks>
    internal class SafeBuffer : IClientProtocolBuffer
    {
        private byte[] _byteArray;

        public SafeBuffer(byte[] buffer)
        {
            Wrap(buffer);
        }

        public void PutLong(int index, long value)
        {
            Bits.WriteLongL(_byteArray, index, value);
        }

        public void PutInt(int index, int value)
        {
            Bits.WriteIntL(_byteArray, index, value);
        }

        public void PutShort(int index, short value)
        {
            Bits.WriteShortL(_byteArray, index, value);
        }

        public void PutByte(int index, byte value)
        {
            _byteArray[index] = value;
        }

        public void PutBytes(int index, byte[] src)
        {
            PutBytes(index, src, 0, src.Length);
        }

        public void PutBytes(int index, byte[] src, int offset, int length)
        {
            Buffer.BlockCopy(src, offset, _byteArray, index, length);
        }

        public int PutStringUtf8(int index, string value)
        {
            return PutStringUtf8(index, value, int.MaxValue);
        }

        public int PutStringUtf8(int index, string value, int maxEncodedSize)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            if (bytes.Length > maxEncodedSize)
            {
                throw new ArgumentException("Encoded string larger than maximum size: " + maxEncodedSize);
            }
            PutInt(index, bytes.Length);
            PutBytes(index + Bits.IntSizeInBytes, bytes);
            return Bits.IntSizeInBytes + bytes.Length;
        }

        public void Wrap(byte[] buffer)
        {
            _byteArray = buffer;
        }

        public byte[] ByteArray()
        {
            return _byteArray;
        }

        public int Capacity()
        {
            return _byteArray.Length;
        }

        public long GetLong(int index)
        {
            return Bits.ReadLongL(_byteArray, index);
        }

        public int GetInt(int index)
        {
            return Bits.ReadIntL(_byteArray, index);
        }

        public short GetShort(int index)
        {
            return Bits.ReadShortL(_byteArray, index);
        }

        public byte GetByte(int index)
        {
            return _byteArray[index];
        }

        public void GetBytes(int index, byte[] dst)
        {
            GetBytes(index, dst, 0, dst.Length);
        }

        public ArraySegment<byte> GetBytesSegment(int index, int length)
        {
            return new ArraySegment<byte>(_byteArray, index, length);
        }

        public void GetBytes(int index, byte[] dst, int offset, int length)
        {
            Buffer.BlockCopy(_byteArray, offset + index, dst, offset, length);
        }

        public string GetStringUtf8(int offset, int length)
        {
            var s = GetBytesSegment(offset + Bits.IntSizeInBytes, length);
            return Encoding.UTF8.GetString(s.Array, s.Offset, s.Count);
        }
    }
}