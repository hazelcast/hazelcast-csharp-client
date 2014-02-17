using System;
using System.Threading;

namespace Hazelcast.Net.Ext
{
    internal static class Extensions
    {
        public static bool IsInterrupted(this Thread thread)
        {
            try
            {
                Thread.Sleep(0); // get exception if interrupted.
            }
            catch (ThreadInterruptedException)
            {
                return true;
            }
            return false;
        }

        public static DateTime CreateDateTime(this DateTime dateTime, long sinceEpoxMillis)
        {
            return new DateTime(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local).Ticks + (sinceEpoxMillis*10000));
        }

        public static DateTime EpoxDateTime(this DateTime dateTime)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local);
        }
    }
}