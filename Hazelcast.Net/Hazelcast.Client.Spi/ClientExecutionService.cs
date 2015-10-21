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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Net.Ext;
using Hazelcast.Util;

namespace Hazelcast.Client.Spi
{
    internal sealed class ClientExecutionService : IClientExecutionService
    {
        private readonly TaskFactory _taskFactory = Task.Factory;
        //private ExecutorService executor;

        //private ScheduledExecutorService scheduledExecutor;

        public ClientExecutionService(string name, int poolSize)
        {
            if (poolSize <= 0)
            {
                int cores = Environment.ProcessorCount;
                poolSize = cores*5;
            }
        }

        //TODO EXECUTER SERVICE
        //
        //        final PoolExecutorThreadFactory poolExecutorThreadFactory = new PoolExecutorThreadFactory(threadGroup, name + ".cached-",null);
        //        executor = Executors.newFixedThreadPool(poolSize, poolExecutorThreadFactory);
        ////        executor = Executors.newCachedThreadPool(new PoolExecutorThreadFactory(threadGroup, name + ".cached-", classLoader));
        //
        //        scheduledExecutor = Executors.newSingleThreadScheduledExecutor(new SingleExecutorThreadFactory(threadGroup, null, name + ".scheduled"));
        //public void Execute(Action<object> action, object state)
        //{
        //    Task.Factory.StartNew(action, state);
        //}

        public Task Submit(Action action)
        {
            return _taskFactory.StartNew(action);
        }

        public Task Submit(Action<object> action, object state)
        {
            return _taskFactory.StartNew(action, state);
        }

        public Task<T> Submit<T>(Func<object, T> function)
        {
            return _taskFactory.StartNew(function, null);
        }

        public Task<T> Submit<T>(Func<object, T> function, object state)
        {
            return _taskFactory.StartNew(function, state);
        }

        public Task SubmitWithDelay(Action action, int delayMilliseconds)
        {
            var tcs = new TaskCompletionSource<object>();
            var continueTask = tcs.Task.ContinueWith(t =>
            {
                if (t.IsCompleted)
                {
                    action();
                }
            });
            new Timer(o => tcs.SetResult(null)).Change(delayMilliseconds, Timeout.Infinite);
            return continueTask;
        }

        internal Task SubmitInternal(Action action)
        {
            //TODO: should use different thread pool
            return _taskFactory.StartNew(action);
        }

        public Task<object> ScheduleWithFixedDelay(Runnable command, long initialDelay, long period, TimeUnit unit)
        {
            throw new NotImplementedException();
            //return scheduledExecutor.ScheduleWithFixedDelay(new _Runnable_83(this, command), initialDelay, period, unit);
        }

        public Task<object> Schedule(Runnable command, long delay, TimeUnit unit)
        {
            throw new NotImplementedException();
            //return scheduledExecutor.Schedule(new _Runnable_65(this, command), delay, unit);
        }

        public Task<object> ScheduleAtFixedRate(Runnable command, long initialDelay, long period, TimeUnit unit)
        {
            throw new NotImplementedException();
            //return scheduledExecutor.ScheduleAtFixedRate(new _Runnable_74(this, command), initialDelay, period, unit);
        }

        public void Shutdown()
        {
            //scheduledExecutor.ShutdownNow();
            //executor.ShutdownNow();
        }
    }
}