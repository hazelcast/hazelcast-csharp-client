using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;

namespace Hazelcast.IO
{
    /// <summary>Provides serialization methods for arrays of primitive types</summary>
    public interface IObjectDataInput : IDataInput
    {
        /// <returns>the boolean array read</returns>
        /// <exception cref="System.IO.IOException">
        ///     if it reaches end of file before finish reading
        /// </exception>
        bool[] ReadBooleanArray();

        /// <returns>the byte array read</returns>
        /// <exception cref="System.IO.IOException">
        ///     if it reaches end of file before finish reading
        /// </exception>
        byte[] ReadByteArray();

        /// <returns>the char array read</returns>
        /// <exception cref="System.IO.IOException">
        ///     if it reaches end of file before finish reading
        /// </exception>
        char[] ReadCharArray();

        /// <returns>int array read</returns>
        /// <exception cref="System.IO.IOException">
        ///     if it reaches end of file before finish reading
        /// </exception>
        int[] ReadIntArray();

        /// <returns>long array read</returns>
        /// <exception cref="System.IO.IOException">
        ///     if it reaches end of file before finish reading
        /// </exception>
        long[] ReadLongArray();

        /// <returns>double array read</returns>
        /// <exception cref="System.IO.IOException">
        ///     if it reaches end of file before finish reading
        /// </exception>
        double[] ReadDoubleArray();

        /// <returns>float array read</returns>
        /// <exception cref="System.IO.IOException">
        ///     if it reaches end of file before finish reading
        /// </exception>
        float[] ReadFloatArray();

        /// <returns>short array read</returns>
        /// <exception cref="System.IO.IOException">
        ///     if it reaches end of file before finish reading
        /// </exception>
        short[] ReadShortArray();

        /// <returns>String array read</returns>
        /// <exception cref="System.IO.IOException">
        ///     if it reaches end of file before finish reading
        /// </exception>
        string[] ReadUTFArray();

        /// <returns>object array read</returns>
        /// <exception cref="System.IO.IOException">
        ///     if it reaches end of file before finish reading
        /// </exception>
        T ReadObject<T>();

        /// <returns>data read</returns>
        /// <exception cref="System.IO.IOException">
        ///     if it reaches end of file before finish reading
        /// </exception>
        IData ReadData();

        /// <returns>ByteOrder BIG_ENDIAN or LITTLE_ENDIAN</returns>
        ByteOrder GetByteOrder();
    }
}