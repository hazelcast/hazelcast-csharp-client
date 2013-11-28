using System.Diagnostics;
using Hazelcast.Logging;


namespace Hazelcast.Logging
{
	public interface LogListener
	{
		void Log(TraceEventType logEvent);
	}
}
