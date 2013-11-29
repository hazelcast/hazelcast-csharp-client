using System;

namespace Hazelcast.Net.Ext
{
    public interface IDataOutput
    {
        /// <exception cref="System.IO.IOException"></exception>
        void Write(int b);

        /// <exception cref="System.IO.IOException"></exception>
        void Write(byte[] b);

        /// <exception cref="System.IO.IOException"></exception>
        void Write(byte[] b, int off, int len);

        /// <exception cref="System.IO.IOException"></exception>
        void WriteBoolean(bool v);

        /// <exception cref="System.IO.IOException"></exception>
        void WriteByte(byte v);

        /// <exception cref="System.IO.IOException"></exception>
        void WriteShort(int v);

        /// <exception cref="System.IO.IOException"></exception>
        void WriteChar(int v);

        /// <exception cref="System.IO.IOException"></exception>
        void WriteInt(int v);

        /// <exception cref="System.IO.IOException"></exception>
        void WriteLong(long v);

        /// <exception cref="System.IO.IOException"></exception>
        void WriteFloat(float v);

        /// <exception cref="System.IO.IOException"></exception>
        void WriteDouble(double v);

        /// <exception cref="System.IO.IOException"></exception>
        void WriteBytes(String s);

        /// <exception cref="System.IO.IOException"></exception>
        void WriteChars(String s);

        /// <exception cref="System.IO.IOException"></exception>
        void WriteUTF(String s);
    }
}