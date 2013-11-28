using System.Threading;
using Hazelcast.Util;


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
