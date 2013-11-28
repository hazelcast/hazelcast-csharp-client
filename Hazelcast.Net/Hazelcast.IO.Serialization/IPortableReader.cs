using System.Collections.Generic;

namespace Hazelcast.IO.Serialization
{
    public interface IPortableReader
    {
        int GetVersion();

        bool HasField(string fieldName);

        ICollection<string> GetFieldNames();

        FieldType GetFieldType(string fieldName);

        int GetFieldClassId(string fieldName);

        /// <exception cref="System.IO.IOException"></exception>
        int ReadInt(string fieldName);

        /// <exception cref="System.IO.IOException"></exception>
        long ReadLong(string fieldName);

        /// <exception cref="System.IO.IOException"></exception>
        string ReadUTF(string fieldName);

        /// <exception cref="System.IO.IOException"></exception>
        bool ReadBoolean(string fieldName);

        /// <exception cref="System.IO.IOException"></exception>
        byte ReadByte(string fieldName);

        /// <exception cref="System.IO.IOException"></exception>
        char ReadChar(string fieldName);

        /// <exception cref="System.IO.IOException"></exception>
        double ReadDouble(string fieldName);

        /// <exception cref="System.IO.IOException"></exception>
        float ReadFloat(string fieldName);

        /// <exception cref="System.IO.IOException"></exception>
        short ReadShort(string fieldName);

        /// <exception cref="System.IO.IOException"></exception>
        P ReadPortable<P>(string fieldName) where P : IPortable;

        /// <exception cref="System.IO.IOException"></exception>
        byte[] ReadByteArray(string fieldName);

        /// <exception cref="System.IO.IOException"></exception>
        char[] ReadCharArray(string fieldName);

        /// <exception cref="System.IO.IOException"></exception>
        int[] ReadIntArray(string fieldName);

        /// <exception cref="System.IO.IOException"></exception>
        long[] ReadLongArray(string fieldName);

        /// <exception cref="System.IO.IOException"></exception>
        double[] ReadDoubleArray(string fieldName);

        /// <exception cref="System.IO.IOException"></exception>
        float[] ReadFloatArray(string fieldName);

        /// <exception cref="System.IO.IOException"></exception>
        short[] ReadShortArray(string fieldName);

        /// <exception cref="System.IO.IOException"></exception>
        IPortable[] ReadPortableArray(string fieldName);

        /// <exception cref="System.IO.IOException"></exception>
        IObjectDataInput GetRawDataInput();
    }
}