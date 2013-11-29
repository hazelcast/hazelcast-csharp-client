using System.Diagnostics;

namespace Hazelcast.Logging
{
    public interface LogListener
    {
        void Log(TraceEventType logEvent);
    }
}