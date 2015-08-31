using System;
using System.IO;
using Hazelcast.Net.Ext;
using Hazelcast.Util;

namespace Hazelcast.IO.Serialization
{
    public class ObjectDataOutputStream : OutputStream, IObjectDataOutput, IDisposable
    {
        private const int UTF_BUFFER_SIZE = 1024;

        //BinaryWriter is always LittleEndian
        private readonly BinaryWriter _binaryWriter;

        private readonly ByteOrder byteOrder;
        private readonly ISerializationService serializationService;

        private byte[] utfBuffer;

        public ObjectDataOutputStream(BinaryWriter binaryWriter, ISerializationService serializationService)
        {
            this.serializationService = serializationService;
            _binaryWriter = binaryWriter;
            byteOrder = serializationService.GetByteOrder();
        }

        public void Dispose()
        {
            _binaryWriter.Dispose();
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteBoolean(bool v)
        {
            _binaryWriter.Write(v);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteByte(int v)
        {
            _binaryWriter.Write(v);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteShort(int v)
        {
            if (!IsBigEndian())
            {
                _binaryWriter.Write((short) v);
            }
            else
            {
                _binaryWriter.Write(ByteFlipperUtil.ReverseBytes((short) v));
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteChar(int v)
        {
            if (!IsBigEndian())
            {
                _binaryWriter.Write((char) v);
            }
            else
            {
                _binaryWriter.Write(ByteFlipperUtil.ReverseBytes((char) v));
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteInt(int v)
        {
            if (!IsBigEndian())
            {
                _binaryWriter.Write(v);
            }
            else
            {
                _binaryWriter.Write(ByteFlipperUtil.ReverseBytes(v));
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteLong(long v)
        {
            if (!IsBigEndian())
            {
                _binaryWriter.Write(v);
            }
            else
            {
                _binaryWriter.Write(ByteFlipperUtil.ReverseBytes(v));
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteFloat(float v)
        {
            if (!IsBigEndian())
            {
                _binaryWriter.Write(v);
            }
            else
            {
                WriteInt(BitConverter.ToInt32(BitConverter.GetBytes(v), 0));
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteDouble(double v)
        {
            if (!IsBigEndian())
            {
                _binaryWriter.Write(v);
            }
            else
            {
                WriteLong(BitConverter.DoubleToInt64Bits(v));
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteBytes(string s)
        {
            _binaryWriter.Write(s);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteChars(string s)
        {
            int len = s.Length;
            for (int i = 0; i < len; i++)
            {
                int v = s[i];
                WriteChar(v);
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteByteArray(byte[] bytes)
        {
            int len = (bytes == null) ? 0 : bytes.Length;
            WriteInt(len);
            if (len > 0)
            {
                Write(bytes);
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteCharArray(char[] chars)
        {
            int len = chars != null ? chars.Length : 0;
            WriteInt(len);
            if (len > 0)
            {
                foreach (char c in chars)
                {
                    WriteChar(c);
                }
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteIntArray(int[] ints)
        {
            int len = ints != null ? ints.Length : 0;
            WriteInt(len);
            if (len > 0)
            {
                foreach (int i in ints)
                {
                    WriteInt(i);
                }
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteLongArray(long[] longs)
        {
            int len = longs != null ? longs.Length : 0;
            WriteInt(len);
            if (len > 0)
            {
                foreach (long l in longs)
                {
                    WriteLong(l);
                }
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteDoubleArray(double[] doubles)
        {
            int len = doubles != null ? doubles.Length : 0;
            WriteInt(len);
            if (len > 0)
            {
                foreach (double d in doubles)
                {
                    WriteDouble(d);
                }
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteFloatArray(float[] floats)
        {
            int len = floats != null ? floats.Length : 0;
            WriteInt(len);
            if (len > 0)
            {
                foreach (float f in floats)
                {
                    WriteFloat(f);
                }
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteShortArray(short[] shorts)
        {
            int len = shorts != null ? shorts.Length : 0;
            WriteInt(len);
            if (len > 0)
            {
                foreach (short s in shorts)
                {
                    WriteShort(s);
                }
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteUTF(string str)
        {
            if (utfBuffer == null)
            {
                utfBuffer = new byte[UTF_BUFFER_SIZE];
            }
            UTFEncoderDecoder.WriteUTF(this, str, utfBuffer);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteObject(object @object)
        {
            serializationService.WriteObject(this, @object);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteData(IData data)
        {
            serializationService.WriteData(this, data);
        }

        public virtual byte[] ToByteArray()
        {
            throw new NotSupportedException();
        }

        public virtual ByteOrder GetByteOrder()
        {
            return byteOrder;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void Write(int b)
        {
            WriteInt(b);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void Write(byte[] b, int off, int len)
        {
            _binaryWriter.Write(b, off, len);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void Write(byte[] b)
        {
            _binaryWriter.Write(b, 0, b.Length);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void Flush()
        {
            _binaryWriter.Flush();
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void Close()
        {
            _binaryWriter.Close();
        }

        private bool IsBigEndian()
        {
            return byteOrder == ByteOrder.BigEndian;
        }
    }
}