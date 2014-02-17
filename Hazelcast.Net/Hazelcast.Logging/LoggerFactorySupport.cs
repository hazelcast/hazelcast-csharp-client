using System.Collections.Concurrent;

namespace Hazelcast.Logging
{
    internal abstract class LoggerFactorySupport : ILoggerFactory
    {
        internal readonly ConcurrentDictionary<string, ILogger> mapLoggers = new ConcurrentDictionary<string, ILogger>();

        //internal readonly Func<string, ILogger> loggerConstructor;

        public ILogger GetLogger(string name)
        {
            return mapLoggers.GetOrAdd(name, CreateLogger(name));
        }

        protected internal abstract ILogger CreateLogger(string name);
    }
}