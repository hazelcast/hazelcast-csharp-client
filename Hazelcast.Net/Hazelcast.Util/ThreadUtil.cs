/*
* Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

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