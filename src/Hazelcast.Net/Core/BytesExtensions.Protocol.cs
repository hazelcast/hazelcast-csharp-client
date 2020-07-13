using System;
using Hazelcast.Core;

namespace Hazelcast.Core
{
    public static partial class BytesExtensions // Protocol
    {
        // use little endian where it makes sense
        // for some types (byte, bool...) it just does not make sense
        // for some types (guid...) endianness is just not an option

        public static void WriteLongL(this byte[] bytes, int position, long value)
            => bytes.WriteLong(position, value, Endianness.LittleEndian);

        public static void WriteIntL(this byte[] bytes, int position, int value)
            => bytes.WriteInt(position, value, Endianness.LittleEndian);

        public static void WriteIntL(this byte[] bytes, int position, Enum value)
            => bytes.WriteInt(position, value, Endianness.LittleEndian);

        public static void WriteBoolL(this byte[] bytes, int position, bool value)
            => bytes.WriteBool(position, value);

        public static void WriteGuidL(this byte[] bytes, int position, Guid value)
            => bytes.WriteGuid(position, value);

        public static void WriteByteL(this byte[] bytes, int position, byte value)
            => bytes.WriteByte(position, value);

        public static long ReadLongL(this byte[] bytes, int position)
            => bytes.ReadLong(position, Endianness.LittleEndian);

        public static int ReadIntL(this byte[] bytes, int position)
            => bytes.ReadInt(position, Endianness.LittleEndian);

        public static bool ReadBoolL(this byte[] bytes, int position)
            => bytes.ReadBool(position);

        public static Guid ReadGuidL(this byte[] bytes, int position)
            => bytes.ReadGuid(position);

        public static byte ReadByteL(this byte[] bytes, int position)
            => bytes.ReadByte(position);
    }
}
