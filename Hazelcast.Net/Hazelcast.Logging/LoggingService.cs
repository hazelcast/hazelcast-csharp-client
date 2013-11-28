using System;
using System.Diagnostics;
using Hazelcast.Logging;
using Hazelcast.Net.Ext;


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
