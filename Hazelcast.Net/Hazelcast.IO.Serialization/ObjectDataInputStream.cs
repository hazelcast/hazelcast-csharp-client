using System;
using System.IO;
using System.Linq;
using Hazelcast.Net.Ext;

namespace Hazelcast.IO.Serialization
{
    public class ObjectDataInputStream : InputStream, IObjectDataInput, IDisposable
    {
        private const int UTFBufferSize = 1024;

        private readonly BinaryReader _binaryReader;
        private readonly ByteOrder byteOrder;
        private readonly ISerializationService serializationService;
        private byte[] utfBuffer;

        public ObjectDataInputStream(BinaryReader binaryReader, ISerializationService serializationService)
        {
            this.serializationService = serializationService;
            byteOrder = serializationService.GetByteOrder();
        }

        public void Dispose()
        {
            _binaryReader.Dispose();
        }

        /// <exception cref="System.IO.IOException"></exception>
        public int Read()
        {
            return ReadByte();
        }

        /// <exception cref="System.IO.IOException"></exception>
        public long Skip(long n)
        {
            long remaining = n;
            int nr;
            if (n <= 0)
            {
                return 0;
            }
            var size = (int) Math.Min(2048, remaining);
            var skipBuffer = new byte[size];
            while (remaining > 0)
            {
                nr = Read(skipBuffer, 0, (int) Math.Min(size, remaining));
                if (nr < 0)
                {
                    break;
                }
                remaining -= nr;
            }
            return n - remaining;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public int Available()
        {
            return (int) _binaryReader.BaseStream.Length;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public int Read(byte[] b)
        {
            return _binaryReader.Read(b, 0, b.Length);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public int Read(byte[] b, int off, int len)
        {
            return _binaryReader.Read(b, off, len);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void Close()
        {
            if (_binaryReader != null)
            {
                _binaryReader.Close();
            }
        }

        public void Mark(int readlimit)
        {
            throw new NotSupportedException();
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void Reset()
        {
            throw new NotSupportedException();
        }

        public bool MarkSupported()
        {
            return false;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void ReadFully(byte[] b)
        {
            ReadFully(b, 0, b.Length);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void ReadFully(byte[] b, int off, int len)
        {
            if (len < 0)
            {
                throw new IndexOutOfRangeException();
            }
            int n = 0;
            while (n < len)
            {
                int count = _binaryReader.Read(b, off + n, len - n);
                if (count < 0)
                    throw new EndOfStreamException();
                n += count;
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual int SkipBytes(int n)
        {
            int total = 0;
            int cur = 0;
            while ((total < n) && ((cur = (int) Skip(n - total)) > 0))
            {
                total += cur;
            }
            return total;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual bool ReadBoolean()
        {
            return _binaryReader.ReadBoolean();
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual byte ReadByte()
        {
            return _binaryReader.ReadByte();
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual int ReadUnsignedByte()
        {
            return _binaryReader.ReadByte();
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual short ReadShort()
        {
            short v = _binaryReader.ReadInt16();
            return !IsBigEndian() ? v : BitConverter.ToInt16(BitConverter.GetBytes(v).Reverse().ToArray(), 0);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual int ReadUnsignedShort()
        {
            ushort v = _binaryReader.ReadUInt16();
            return !IsBigEndian() ? v : BitConverter.ToUInt16(BitConverter.GetBytes(v).Reverse().ToArray(), 0);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual char ReadChar()
        {
            char v = _binaryReader.ReadChar();
            return !IsBigEndian() ? v : BitConverter.ToChar(BitConverter.GetBytes(v).Reverse().ToArray(), 0);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual int ReadInt()
        {
            int v = _binaryReader.ReadInt32();
            return !IsBigEndian() ? v : BitConverter.ToInt32(BitConverter.GetBytes(v).Reverse().ToArray(), 0);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual long ReadLong()
        {
            long v = _binaryReader.ReadInt64();
            return !IsBigEndian() ? v : BitConverter.ToInt64(BitConverter.GetBytes(v).Reverse().ToArray(), 0);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual float ReadFloat()
        {
            float v = _binaryReader.ReadSingle();
            return !IsBigEndian() ? v : BitConverter.ToSingle(BitConverter.GetBytes(v).Reverse().ToArray(), 0);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual double ReadDouble()
        {
            double v = _binaryReader.ReadSingle();
            return !IsBigEndian() ? v : BitConverter.ToDouble(BitConverter.GetBytes(v).Reverse().ToArray(), 0);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual byte[] ReadByteArray()
        {
            int len = ReadInt();
            if (len > 0)
            {
                var b = new byte[len];
                ReadFully(b);
                return b;
            }
            return new byte[0];
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual char[] ReadCharArray()
        {
            int len = ReadInt();
            if (len > 0)
            {
                var values = new char[len];
                for (int i = 0; i < len; i++)
                {
                    values[i] = ReadChar();
                }
                return values;
            }
            return new char[0];
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual int[] ReadIntArray()
        {
            int len = ReadInt();
            if (len > 0)
            {
                var values = new int[len];
                for (int i = 0; i < len; i++)
                {
                    values[i] = ReadInt();
                }
                return values;
            }
            return new int[0];
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual long[] ReadLongArray()
        {
            int len = ReadInt();
            if (len > 0)
            {
                var values = new long[len];
                for (int i = 0; i < len; i++)
                {
                    values[i] = ReadLong();
                }
                return values;
            }
            return new long[0];
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual double[] ReadDoubleArray()
        {
            int len = ReadInt();
            if (len > 0)
            {
                var values = new double[len];
                for (int i = 0; i < len; i++)
                {
                    values[i] = ReadDouble();
                }
                return values;
            }
            return new double[0];
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual float[] ReadFloatArray()
        {
            int len = ReadInt();
            if (len > 0)
            {
                var values = new float[len];
                for (int i = 0; i < len; i++)
                {
                    values[i] = ReadFloat();
                }
                return values;
            }
            return new float[0];
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual short[] ReadShortArray()
        {
            int len = ReadInt();
            if (len > 0)
            {
                var values = new short[len];
                for (int i = 0; i < len; i++)
                {
                    values[i] = ReadShort();
                }
                return values;
            }
            return new short[0];
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual string ReadUTF()
        {
            if (utfBuffer == null)
            {
                utfBuffer = new byte[UTFBufferSize];
            }
            return UTFEncoderDecoder.ReadUTF(this, utfBuffer);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual IData ReadData()
        {
            return serializationService.ReadData(this);
        }

        public virtual ByteOrder GetByteOrder()
        {
            return byteOrder;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public T ReadObject<T>()
        {
            return serializationService.ReadObject<T>(this);
        }

        private bool IsBigEndian()
        {
            return byteOrder == ByteOrder.BigEndian;
        }
    }
}