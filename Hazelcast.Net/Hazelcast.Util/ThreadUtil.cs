using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Spi;

namespace Hazelcast.Util
{
    internal sealed class ThreadUtil
    {
        public static int TaskOperationTimeOutMilliseconds = 250*1000;

        public static IList<IClientMessage> GetResult(IEnumerable<IFuture<IClientMessage>> futures)
        {
            return futures.Select(future => GetResult(future)).ToList();
        }

        public static IClientMessage GetResult(IFuture<IClientMessage> future, int? timeout = null)
        {
            if (timeout.HasValue) return future.GetResult(timeout.Value);
            return future.GetResult(TaskOperationTimeOutMilliseconds);
        }

        public static IClientMessage GetResult(Task<IClientMessage> task)
        {
            return GetResult(task, TaskOperationTimeOutMilliseconds);
        }

        public static IClientMessage GetResult(Task<IClientMessage> task, int timeout)
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
                throw ExceptionUtil.Rethrow(e);
            }
            return task.Result;
        }

        public static long GetThreadId()
        {
            return Thread.CurrentThread.ManagedThreadId;
        }
    }
}