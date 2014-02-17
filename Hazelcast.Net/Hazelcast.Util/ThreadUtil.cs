using System.Threading;

namespace Hazelcast.Util
{
    internal sealed class ThreadUtil
    {
        public static long GetThreadId()
        {
            return Thread.CurrentThread.ManagedThreadId;
        }
    }
}