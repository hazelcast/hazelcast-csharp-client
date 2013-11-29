using Hazelcast.Net.Ext;

namespace Hazelcast.IO
{
    public interface IObjectDataInput : IDataInput
    {
        /// <exception cref="System.IO.IOException"></exception>
        char[] ReadCharArray();

        /// <exception cref="System.IO.IOException"></exception>
        int[] ReadIntArray();

        /// <exception cref="System.IO.IOException"></exception>
        long[] ReadLongArray();

        /// <exception cref="System.IO.IOException"></exception>
        double[] ReadDoubleArray();

        /// <exception cref="System.IO.IOException"></exception>
        float[] ReadFloatArray();

        /// <exception cref="System.IO.IOException"></exception>
        short[] ReadShortArray();

        /// <exception cref="System.IO.IOException"></exception>
        T ReadObject<T>();

        bool IsBigEndian();
    }
}