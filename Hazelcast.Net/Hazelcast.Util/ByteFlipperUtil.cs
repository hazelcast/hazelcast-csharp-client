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
            var v = (ulong) i;
            //first swap every 1-8th with every 9-16th bit
            v = (v & 0x00FF00FF00FF00FFL) << 8  | ((v >> 8) & 0x00FF00FF00FF00FFL);
		    //then swap every 1-16th with every 17-32nd
		    v = (v & 0x0000FFFF0000FFFFL) << 16 | ((v >> 16) & 0x0000FFFF0000FFFFL);
		    //then swap 1-32nd with 33-64th
		    return (long)((v  << 32) | (v >> 32));
        }
    }
}