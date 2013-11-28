using Hazelcast.Logging;


namespace Hazelcast.Logging
{
	public interface ILoggerFactory
	{
		ILogger GetLogger(string name);
	}
}
