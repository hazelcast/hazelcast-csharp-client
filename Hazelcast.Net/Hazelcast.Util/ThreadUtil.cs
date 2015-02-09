using System;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Util
{
    internal sealed class ThreadUtil
    {
        public static int TaskOperationTimeOutMilliseconds = 250 * 1000;

        public static long GetThreadId()
        {
            return Thread.CurrentThread.ManagedThreadId;
        }

        public static IData GetResult(Task<IData> task)
        {
            try
            {
                var responseReady = task.Wait(TimeSpan.FromMilliseconds(TaskOperationTimeOutMilliseconds));
                if (!responseReady)
                {
                    throw new TimeoutException("Operation time-out! No response received from the server.");
                }
            }
            catch (AggregateException e)
            {
                e.Handle(exception =>
                {
                    ExceptionUtil.Rethrow(exception);
                    return true;
                });
                
                throw;
            }
            return task.Result;
        }
    }
}