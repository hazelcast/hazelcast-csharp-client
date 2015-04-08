using System;
using Hazelcast.Net.Ext;

namespace Hazelcast.IO
{
    public interface IBufferObjectDataOutput : IObjectDataOutput, IDisposable
    {
        void Write(int position, int b);

        /// <exception cref="System.IO.IOException"/>
        void WriteInt(int position, int v);

        /// <exception cref="System.IO.IOException"/>
        void WriteInt(int v, ByteOrder byteOrder);

        /// <exception cref="System.IO.IOException"/>
        void WriteInt(int position, int v, ByteOrder byteOrder);

        /// <exception cref="System.IO.IOException"/>
        void WriteLong(int position, long v);

        /// <exception cref="System.IO.IOException"/>
        void WriteLong(long v, ByteOrder byteOrder);

        /// <exception cref="System.IO.IOException"/>
        void WriteLong(int position, long v, ByteOrder byteOrder);

        /// <exception cref="System.IO.IOException"/>
        void WriteBoolean(int position, bool v);

        /// <exception cref="System.IO.IOException"/>
        void WriteByte(int position, int v);

        void WriteZeroBytes(int count);

        /// <exception cref="System.IO.IOException"/>
        void WriteChar(int position, int v);

        /// <exception cref="System.IO.IOException"/>
        void WriteDouble(int position, double v);

        /// <exception cref="System.IO.IOException"/>
        void WriteDouble(double v, ByteOrder byteOrder);

        /// <exception cref="System.IO.IOException"/>
        void WriteDouble(int position, double v, ByteOrder byteOrder);

        /// <exception cref="System.IO.IOException"/>
        void WriteFloat(int position, float v);

        /// <exception cref="System.IO.IOException"/>
        void WriteFloat(float v, ByteOrder byteOrder);

        /// <exception cref="System.IO.IOException"/>
        void WriteFloat(int position, float v, ByteOrder byteOrder);

        /// <exception cref="System.IO.IOException"/>
        void WriteShort(int position, int v);

        /// <exception cref="System.IO.IOException"/>
        void WriteShort(int v, ByteOrder byteOrder);

        /// <exception cref="System.IO.IOException"/>
        void WriteShort(int position, int v, ByteOrder byteOrder);

        int Position();

        void Position(int newPos);

        void Clear();
    }

    public static class BufferObjectDataOutputConstants
    {
        public const int UtfBufferSize = 1024;
    }
}
