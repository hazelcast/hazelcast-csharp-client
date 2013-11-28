using System;
using System.Threading.Tasks;
using Hazelcast.Client.Spi;
using Hazelcast.Net.Ext;
using Hazelcast.Util;


namespace Hazelcast.Client.Spi
{
	public interface IClientExecutionService
	{

	    Task Submit(Action action);

	    Task Submit(Action<object> action, object state);

        Task<T> Submit<T>(Func<object, T> function);
        Task<T> Submit<T>(Func<object, T> function, object state);

        //Task<object> Schedule(Runnable command, long delay, TimeUnit unit);

        //Task<object> ScheduleAtFixedRate(Runnable command, long initialDelay, long period, TimeUnit unit);

		Task<object> ScheduleWithFixedDelay(Runnable command, long initialDelay, long period, TimeUnit unit);
	}
}
