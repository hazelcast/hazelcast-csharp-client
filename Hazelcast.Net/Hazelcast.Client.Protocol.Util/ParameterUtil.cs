using Hazelcast.IO;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Protocol.Util
{
    internal class ParameterUtil
    {
        public static int CalculateStringDataSize(string @string)
        {
            return Bits.IntSizeInBytes + @string.Length*4;
        }

        public static int CalculateByteArrayDataSize(byte[] bytes)
        {
            return Bits.IntSizeInBytes + bytes.Length;
        }

        public static int CalculateDataSize(IData key)
        {
            return CalculateByteArrayDataSize(key.ToByteArray());
        }
    }
}