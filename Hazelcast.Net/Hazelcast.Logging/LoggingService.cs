using System;
using System.Diagnostics;

namespace Hazelcast.Logging
{
    public interface ILoggingService
    {
        void AddLogListener(TraceLevel level, LogListener logListener);

        void RemoveLogListener(LogListener logListener);

        ILogger GetLogger(string name);

        ILogger GetLogger(Type type);
    }
}