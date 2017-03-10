// Copyright (c) 2008-2017, Hazelcast, Inc. All Rights Reserved.
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
        private const bool ShouldBoundsCheck = true;
        private byte[] _byteArray;

        public SafeBuffer(byte[] buffer)
        {
            Wrap(buffer);
        }

        public virtual void PutLong(int index, long value)
        {
            Bits.WriteLongL(_byteArray, index, value);
        }

        public virtual void PutInt(int index, int value)
        {
            Bits.WriteIntL(_byteArray, index, value);
        }

        public virtual void PutShort(int index, short value)
        {
            Bits.WriteShortL(_byteArray, index, value);
        }

        public virtual void PutByte(int index, byte value)
        {
            _byteArray[index] = value;
        }

        public virtual void PutBytes(int index, byte[] src)
        {
            PutBytes(index, src, 0, src.Length);
        }

        public virtual void PutBytes(int index, byte[] src, int offset, int length)
        {
            BoundsCheck(index, length);
            BoundsCheck(src, offset, length);
            Array.Copy(src, offset, _byteArray, index, length);
        }

        public virtual int PutStringUtf8(int index, string value)
        {
            return PutStringUtf8(index, value, int.MaxValue);
        }

        public virtual int PutStringUtf8(int index, string value, int maxEncodedSize)
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

        public virtual int Capacity()
        {
            return _byteArray.Length;
        }

        public virtual long GetLong(int index)
        {
            BoundsCheck(index, Bits.LongSizeInBytes);
            return Bits.ReadLongL(_byteArray, index);
        }

        public virtual int GetInt(int index)
        {
            BoundsCheck(index, Bits.IntSizeInBytes);
            return Bits.ReadIntL(_byteArray, index);
        }

        public virtual short GetShort(int index)
        {
            BoundsCheck(index, Bits.ShortSizeInBytes);
            return Bits.ReadShortL(_byteArray, index);
        }

        public virtual byte GetByte(int index)
        {
            BoundsCheck(index, Bits.ByteSizeInBytes);
            return _byteArray[index];
        }

        public virtual void GetBytes(int index, byte[] dst)
        {
            GetBytes(index, dst, 0, dst.Length);
        }

        public virtual void GetBytes(int index, byte[] dst, int offset, int length)
        {
            BoundsCheck(index, length);
            BoundsCheck(dst, offset, length);
            Array.Copy(_byteArray, offset + index, dst, offset, length);
        }

        public virtual string GetStringUtf8(int offset, int length)
        {
            var stringInBytes = new byte[length];
            GetBytes(offset + Bits.IntSizeInBytes, stringInBytes);
            return Encoding.UTF8.GetString(stringInBytes);
        }

        ///////////////////////////////////////////////////////////////////////////
        public void BoundsCheck(int index, int length)
        {
            if (ShouldBoundsCheck)
            {
                if (index < 0 || length < 0 || (index + length) > Capacity())
                {
                    throw new IndexOutOfRangeException(string.Format("index=%d, length=%d, capacity=%d", index, length,
                        Capacity()));
                }
            }
        }

        private static void BoundsCheck(byte[] buffer, int index, int length)
        {
            if (ShouldBoundsCheck)
            {
                var capacity = buffer.Length;
                if (index < 0 || length < 0 || (index + length) > capacity)
                {
                    throw new IndexOutOfRangeException(string.Format("index=%d, length=%d, capacity=%d", index, length,
                        capacity));
                }
            }
        }
    }
}