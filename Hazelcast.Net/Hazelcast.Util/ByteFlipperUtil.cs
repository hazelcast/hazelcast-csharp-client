namespace Hazelcast.Util
{
    internal static class ByteFlipperUtil
    {
        internal static int ReverseBytes(int i)
        {
            return (
                ((int) (((uint) i) >> 24))) |
                   ((i >> 8) & 0xFF00) |
                   ((i << 8) & 0xFF0000) |
                   ((i << 24)
                       );
        }

        internal static short ReverseBytes(short input)
        {
            return (short) (((input & 0xFF00) >> 8) | (input << 8));
        }

        internal static char ReverseBytes(char input)
        {
            return (char) (((input & 0xFF00) >> 8) | (input << 8));
        }

        internal static long ReverseBytes(long i)
        {
            i = (i & 0x00ff00ff00ff00ffL) << 8 | ((long) (((ulong) i) >> 8)) & 0x00ff00ff00ff00ffL;
            return
                (i << 48) |
                ((i & 0xffff0000L) << 16) |
                ((long) ((((ulong) i) >> 16)) & 0xffff0000L) |
                ((long) (((ulong) i)) >> 48);
        }
    }
}