using Hazelcast.Client.Connection;
using Hazelcast.IO;
using Hazelcast.Util;


namespace Hazelcast.Client.Connection
{
	public interface IClientConnectionManager
	{
		void Shutdown();

		/// <exception cref="System.IO.IOException"></exception>
		IConnection GetRandomConnection();

		/// <exception cref="System.IO.IOException"></exception>
        IConnection GetConnection(Address address);

		void RemoveConnectionPool(Address address);

		/// <exception cref="System.IO.IOException"></exception>
        IConnection NewConnection(Address address, Authenticator authenticator);

		/// <exception cref="System.IO.IOException"></exception>
        IConnection FirstConnection(Address address, Authenticator authenticator);
	}
}
