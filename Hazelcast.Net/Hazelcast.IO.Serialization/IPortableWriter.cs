namespace Hazelcast.IO.Serialization
{
    public interface IPortableWriter
    {
        int GetVersion();

        /// <exception cref="System.IO.IOException"></exception>
        void WriteInt(string fieldName, int value);

        /// <exception cref="System.IO.IOException"></exception>
        void WriteLong(string fieldName, long value);

        /// <exception cref="System.IO.IOException"></exception>
        void WriteUTF(string fieldName, string value);

        /// <exception cref="System.IO.IOException"></exception>
        void WriteBoolean(string fieldName, bool value);

        /// <exception cref="System.IO.IOException"></exception>
        void WriteByte(string fieldName, byte value);

        /// <exception cref="System.IO.IOException"></exception>
        void WriteChar(string fieldName, int value);

        /// <exception cref="System.IO.IOException"></exception>
        void WriteDouble(string fieldName, double value);

        /// <exception cref="System.IO.IOException"></exception>
        void WriteFloat(string fieldName, float value);

        /// <exception cref="System.IO.IOException"></exception>
        void WriteShort(string fieldName, short value);

        /// <exception cref="System.IO.IOException"></exception>
        void WritePortable(string fieldName, IPortable portable);

        /// <exception cref="System.IO.IOException"></exception>
        void WriteNullPortable(string fieldName, int factoryId, int classId);

        /// <exception cref="System.IO.IOException"></exception>
        void WriteByteArray(string fieldName, byte[] bytes);

        /// <exception cref="System.IO.IOException"></exception>
        void WriteCharArray(string fieldName, char[] chars);

        /// <exception cref="System.IO.IOException"></exception>
        void WriteIntArray(string fieldName, int[] ints);

        /// <exception cref="System.IO.IOException"></exception>
        void WriteLongArray(string fieldName, long[] longs);

        /// <exception cref="System.IO.IOException"></exception>
        void WriteDoubleArray(string fieldName, double[] values);

        /// <exception cref="System.IO.IOException"></exception>
        void WriteFloatArray(string fieldName, float[] values);

        /// <exception cref="System.IO.IOException"></exception>
        void WriteShortArray(string fieldName, short[] values);

        /// <exception cref="System.IO.IOException"></exception>
        void WritePortableArray(string fieldName, IPortable[] portables);

        /// <exception cref="System.IO.IOException"></exception>
        IObjectDataOutput GetRawDataOutput();
    }
}