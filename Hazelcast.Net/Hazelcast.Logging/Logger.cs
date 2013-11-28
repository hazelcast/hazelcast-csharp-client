using System;
using Hazelcast.Logging;


namespace Hazelcast.Logging
{
	public sealed class Logger
	{
		private static volatile ILoggerFactory loggerFactory = null;

		private static readonly object factoryLock = new object();

		public static ILogger GetLogger(Type clazz)
		{
			return GetLogger(clazz.FullName);
		}

		public static ILogger GetLogger(string name)
		{
			//noinspection DoubleCheckedLocking
			if (loggerFactory == null)
			{
				//noinspection SynchronizationOnStaticField
				lock (factoryLock)
				{
					if (loggerFactory == null)
					{
						string loggerType = Environment.GetEnvironmentVariable("hazelcast.logging.type");
						loggerFactory = NewLoggerFactory(loggerType);
					}
				}
			}
			return loggerFactory.GetLogger(name);
		}

		public static ILoggerFactory NewLoggerFactory(string loggerType)
		{
			ILoggerFactory loggerFactory = null;
            string loggerClass = Environment.GetEnvironmentVariable("hazelcast.logging.class");
            //TODO FIX DIFFERENT LOGGERS

            if (loggerFactory == null)
            {
                loggerFactory = new TraceLogFactory();
            }
            return loggerFactory;
		}
	}
}
