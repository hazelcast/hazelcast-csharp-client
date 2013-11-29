using System;

namespace Hazelcast.Net.Ext
{
    public interface IDataInput
    {
        /// <exception cref="System.IO.IOException"></exception>
        void ReadFully(byte[] b);

        /// <exception cref="System.IO.IOException"></exception>
        void ReadFully(byte[] b, int off, int len);

        /// <exception cref="System.IO.IOException"></exception>
        int SkipBytes(int n);

        /// <exception cref="System.IO.IOException"></exception>
        bool ReadBoolean();

        /// <exception cref="System.IO.IOException"></exception>
        byte ReadByte();

        /// <exception cref="System.IO.IOException"></exception>
        int ReadUnsignedByte();

        /// <exception cref="System.IO.IOException"></exception>
        short ReadShort();

        /// <exception cref="System.IO.IOException"></exception>
        int ReadUnsignedShort();

        /// <exception cref="System.IO.IOException"></exception>
        char ReadChar();

        /// <exception cref="System.IO.IOException"></exception>
        int ReadInt();

        /// <exception cref="System.IO.IOException"></exception>
        long ReadLong();

        /// <exception cref="System.IO.IOException"></exception>
        float ReadFloat();

        /// <exception cref="System.IO.IOException"></exception>
        double ReadDouble();

        /// <exception cref="System.IO.IOException"></exception>
        String ReadLine();

        /// <exception cref="System.IO.IOException"></exception>
        String ReadUTF();
    }
}