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
        private byte[] byteArray;

        public SafeBuffer(byte[] buffer)
        {
            Wrap(buffer);
        }

        public virtual void PutLong(int index, long value)
        {
            Bits.WriteLongL(byteArray, index, value);
        }

        public virtual void PutInt(int index, int value)
        {
            Bits.WriteIntL(byteArray, index, value);
        }

        public virtual void PutShort(int index, short value)
        {
            Bits.WriteShortL(byteArray, index, value);
        }

        public virtual void PutByte(int index, byte value)
        {
            byteArray[index] = value;
        }

        public virtual void PutBytes(int index, byte[] src)
        {
            PutBytes(index, src, 0, src.Length);
        }

        public virtual void PutBytes(int index, byte[] src, int offset, int length)
        {
            BoundsCheck(index, length);
            BoundsCheck(src, offset, length);
            Array.Copy(src, offset, byteArray, offset + index, length);
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
            byteArray = buffer;
        }

        public byte[] ByteArray()
        {
            return byteArray;
        }

        public virtual int Capacity()
        {
            return byteArray.Length;
        }

        public virtual long GetLong(int index)
        {
            BoundsCheck(index, Bits.LongSizeInBytes);
            return Bits.ReadLongL(byteArray, index);
        }

        public virtual int GetInt(int index)
        {
            BoundsCheck(index, Bits.IntSizeInBytes);
            return Bits.ReadIntL(byteArray, index);
        }

        public virtual short GetShort(int index)
        {
            BoundsCheck(index, Bits.ShortSizeInBytes);
            return Bits.ReadShortL(byteArray, index);
        }

        public virtual byte GetByte(int index)
        {
            BoundsCheck(index, Bits.ByteSizeInBytes);
            return byteArray[index];
        }

        public virtual void GetBytes(int index, byte[] dst)
        {
            GetBytes(index, dst, 0, dst.Length);
        }

        public virtual void GetBytes(int index, byte[] dst, int offset, int length)
        {
            BoundsCheck(index, length);
            BoundsCheck(dst, offset, length);
            Array.Copy(byteArray, offset + index, dst, offset, length);
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