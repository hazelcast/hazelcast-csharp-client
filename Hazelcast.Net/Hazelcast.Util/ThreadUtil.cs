using System.Threading;

namespace Hazelcast.Util
{
    public sealed class ThreadUtil
    {
        public static long GetThreadId()
        {
            return Thread.CurrentThread.ManagedThreadId;
        }
    }
}