using System;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Util;

namespace Hazelcast.Client.Spi
{
    public interface IClientExecutionService
    {
        Task Submit(Action action);

        Task Submit(Action<object> action, object state);

        Task<T> Submit<T>(Func<object, T> function);
        Task<T> Submit<T>(Func<object, T> function, object state);

        Task<object> ScheduleWithFixedDelay(Runnable command, long initialDelay, long period, TimeUnit unit);
    }
}