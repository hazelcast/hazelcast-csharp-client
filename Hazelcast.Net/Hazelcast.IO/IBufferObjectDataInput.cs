using System;
using Hazelcast.Net.Ext;

namespace Hazelcast.IO
{
    public interface IBufferObjectDataInput : IObjectDataInput, IDisposable
    {
        /// <exception cref="System.IO.IOException"/>
        int Read(int position);

        /// <exception cref="System.IO.IOException"/>
        int ReadInt(int position);

        /// <exception cref="System.IO.IOException"/>
        int ReadInt(ByteOrder byteOrder);

        /// <exception cref="System.IO.IOException"/>
        int ReadInt(int position, ByteOrder byteOrder);

        /// <exception cref="System.IO.IOException"/>
        long ReadLong(int position);

        /// <exception cref="System.IO.IOException"/>
        long ReadLong(ByteOrder byteOrder);

        /// <exception cref="System.IO.IOException"/>
        long ReadLong(int position, ByteOrder byteOrder);

        /// <exception cref="System.IO.IOException"/>
        bool ReadBoolean(int position);

        /// <exception cref="System.IO.IOException"/>
        byte ReadByte(int position);

        /// <exception cref="System.IO.IOException"/>
        char ReadChar(int position);

        /// <exception cref="System.IO.IOException"/>
        double ReadDouble(int position);

        /// <exception cref="System.IO.IOException"/>
        double ReadDouble(ByteOrder byteOrder);

        /// <exception cref="System.IO.IOException"/>
        double ReadDouble(int position, ByteOrder byteOrder);

        /// <exception cref="System.IO.IOException"/>
        float ReadFloat(int position);

        /// <exception cref="System.IO.IOException"/>
        float ReadFloat(ByteOrder byteOrder);

        /// <exception cref="System.IO.IOException"/>
        float ReadFloat(int position, ByteOrder byteOrder);

        /// <exception cref="System.IO.IOException"/>
        short ReadShort(int position);

        /// <exception cref="System.IO.IOException"/>
        short ReadShort(ByteOrder byteOrder);

        /// <exception cref="System.IO.IOException"/>
        short ReadShort(int position, ByteOrder byteOrder);

        int Position();

        void Position(int newPos);

        void Reset();
    }

    public static class BufferObjectDataInputConstants
    {
        public const int UtfBufferSize = 1024;
    }
}
