using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Spi
{
	public interface IObjectNamespace : IDataSerializable
	{
		string GetServiceName();

		string GetObjectName();
	}
}
