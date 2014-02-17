using System;
using System.Diagnostics;

namespace Hazelcast.Logging
{
    internal sealed class Logger
    {
        private static volatile ILoggerFactory loggerFactory;

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
            ILoggerFactory _loggerFactory = null;
            string loggerClass = Environment.GetEnvironmentVariable("hazelcast.logging.class");
            if ("console".Equals(loggerClass))
            {
                _loggerFactory = new ConsoleLogFactory();
            }

            if (_loggerFactory == null)
            {
                _loggerFactory = (Debugger.IsAttached ? (ILoggerFactory) new TraceLogFactory() : new NoLogFactory());
            }

            return _loggerFactory;
        }
    }
}