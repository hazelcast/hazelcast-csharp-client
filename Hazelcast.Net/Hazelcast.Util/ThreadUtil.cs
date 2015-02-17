using System;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Client.Connection;
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

        public static IData GetResult(Task<IData> task, int timeout)
        {
            try
            {
                var taskData = task.AsyncState as TaskData;
                taskData.Wait();
                var responseReady = task.Wait(TimeSpan.FromMilliseconds(TaskOperationTimeOutMilliseconds));
                if (!responseReady)
                {
                    throw new TimeoutException("Operation time-out! No response received from the server.");
                }
            }
            catch (AggregateException e)
            {
                ExceptionUtil.Rethrow(e);
            }
            return task.Result;
        } 
        
        public static IData GetResult(Task<IData> task)
        {
            return GetResult(task, TaskOperationTimeOutMilliseconds);
        }
    }
}