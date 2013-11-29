using Hazelcast.Net.Ext;

namespace Hazelcast.IO
{
    public interface IObjectDataOutput : IDataOutput
    {
        /// <exception cref="System.IO.IOException"></exception>
        void WriteCharArray(char[] chars);

        /// <exception cref="System.IO.IOException"></exception>
        void WriteIntArray(int[] ints);

        /// <exception cref="System.IO.IOException"></exception>
        void WriteLongArray(long[] longs);

        /// <exception cref="System.IO.IOException"></exception>
        void WriteDoubleArray(double[] values);

        /// <exception cref="System.IO.IOException"></exception>
        void WriteFloatArray(float[] values);

        /// <exception cref="System.IO.IOException"></exception>
        void WriteShortArray(short[] values);

        /// <exception cref="System.IO.IOException"></exception>
        void WriteObject(object @object);

        byte[] ToByteArray();

        bool IsBigEndian();
    }
}