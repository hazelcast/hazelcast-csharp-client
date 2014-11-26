using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;

namespace Hazelcast.IO
{
    /// <summary>Provides serialization methods for arrays by extending DataOutput</summary>
    public interface IObjectDataOutput : IDataOutput
    {
        /// <param name="bytes">byte array to be written</param>
        /// <exception cref="System.IO.IOException">System.IO.IOException</exception>
        void WriteByteArray(byte[] bytes);

        /// <param name="chars">char array to be written</param>
        /// <exception cref="System.IO.IOException">System.IO.IOException</exception>
        void WriteCharArray(char[] chars);

        /// <param name="ints">int array to be written</param>
        /// <exception cref="System.IO.IOException">System.IO.IOException</exception>
        void WriteIntArray(int[] ints);

        /// <param name="longs">long array to be written</param>
        /// <exception cref="System.IO.IOException">System.IO.IOException</exception>
        void WriteLongArray(long[] longs);

        /// <param name="values">double to be written</param>
        /// <exception cref="System.IO.IOException">System.IO.IOException</exception>
        void WriteDoubleArray(double[] values);

        /// <param name="values">float to be written</param>
        /// <exception cref="System.IO.IOException">System.IO.IOException</exception>
        void WriteFloatArray(float[] values);

        /// <param name="values">short to be written</param>
        /// <exception cref="System.IO.IOException">System.IO.IOException</exception>
        void WriteShortArray(short[] values);

        /// <param name="object">object to be written</param>
        /// <exception cref="System.IO.IOException">System.IO.IOException</exception>
        void WriteObject(object @object);

        /// <param name="data">data to be written</param>
        /// <exception cref="System.IO.IOException">System.IO.IOException</exception>
        void WriteData(IData data);

        /// <returns>copy of internal byte array</returns>
        byte[] ToByteArray();

        /// <returns>ByteOrder BIG_ENDIAN or LITTLE_ENDIAN</returns>
        ByteOrder GetByteOrder();
    }
}