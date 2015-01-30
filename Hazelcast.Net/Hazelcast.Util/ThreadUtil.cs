using System;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Util
{
    internal sealed class ThreadUtil
    {
        public static long GetThreadId()
        {
            return Thread.CurrentThread.ManagedThreadId;
        }

        public static IData GetResult(Task<IData> task)
        {
            var responseReady = task.Wait(TimeSpan.FromSeconds(250));
            if (!responseReady)
            {
                throw new TimeoutException("Operation time-out! No response received from the server.");
            }
            return task.Result;
        }
    }
}