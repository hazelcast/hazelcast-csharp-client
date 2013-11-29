namespace Hazelcast.Logging
{
    public interface ILoggerFactory
    {
        ILogger GetLogger(string name);
    }
}