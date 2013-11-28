using Hazelcast.Client;
using Hazelcast.Client.Connection;
using Hazelcast.IO;
using Hazelcast.Util;


namespace Hazelcast.Client.Connection
{
	
	public class DummyClientConnectionManager : SmartClientConnectionManager
	{
		private volatile Address address;

		public DummyClientConnectionManager(HazelcastClient client, Authenticator authenticator, LoadBalancer loadBalancer) : base(client, authenticator, loadBalancer)
		{
		}

		/// <exception cref="System.IO.IOException"></exception>
		public override IConnection FirstConnection(Address address, Authenticator authenticator)
		{
			IConnection connection = NewConnection(address, authenticator);
			this.address = connection.GetRemoteEndpoint();
			return connection;
		}

		/// <summary>get or create connection</summary>
		/// <param name="address"></param>
		/// <returns></returns>
		/// <exception cref="System.IO.IOException">System.IO.IOException</exception>
		public override IConnection GetConnection(Address address)
		{
			if (this.address != null)
			{
				return base.GetConnection(this.address);
			}
			else
			{
				return base.GetConnection(address);
			}
		}
	}
}
