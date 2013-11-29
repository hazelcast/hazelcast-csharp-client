using System.Threading;

namespace Hazelcast.Util
{
    public sealed class ThreadUtil
    {
        public static int GetThreadId()
        {
            return Thread.CurrentThread.ManagedThreadId;
        }
    }
}