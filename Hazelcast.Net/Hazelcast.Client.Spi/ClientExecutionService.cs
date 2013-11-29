using System;
using System.Threading.Tasks;
using Hazelcast.Net.Ext;
using Hazelcast.Util;

namespace Hazelcast.Client.Spi
{
    public sealed class ClientExecutionService : IClientExecutionService
    {
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
            return Task.Factory.StartNew(action);
        }

        public Task Submit(Action<object> action, object state)
        {
            return Task.Factory.StartNew(action, state);
        }

        public Task<T> Submit<T>(Func<object, T> function)
        {
            return Task.Factory.StartNew(function, null);
        }

        public Task<T> Submit<T>(Func<object, T> function, object state)
        {
            return Task.Factory.StartNew(function, state);
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