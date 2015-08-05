using System.Collections.Generic;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Protocol.Util
{
    internal sealed class ParameterUtil
    {
        private const int Utf8MaxBytesPerChar = 3;

        private ParameterUtil()
        {
        }

        public static int CalculateDataSize(string @string)
        {
            return Bits.IntSizeInBytes + @string.Length*Utf8MaxBytesPerChar;
        }

        public static int CalculateDataSize(IData data)
        {
            return CalculateDataSize(data.ToByteArray());
        }

        public static int CalculateDataSize(KeyValuePair<IData, IData> entry)
        {
            return CalculateDataSize(entry.Key.ToByteArray()) + CalculateDataSize(entry.Value.ToByteArray());
        }

        public static int CalculateDataSize(byte[] bytes)
        {
            return Bits.IntSizeInBytes + bytes.Length;
        }

        public static int CalculateDataSize(int data)
        {
            return Bits.IntSizeInBytes;
        }

        public static int CalculateDataSize(bool data)
        {
            return Bits.BooleanSizeInBytes;
        }
    }
}