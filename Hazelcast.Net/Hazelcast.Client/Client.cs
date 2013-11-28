using System.Net;
using Hazelcast.Core;


namespace Hazelcast.Client
{
	public class Client : IClient
	{
		private readonly string uuid;

		private readonly IPEndPoint socketAddress;

		public Client(string uuid, IPEndPoint socketAddress)
		{
			this.uuid = uuid;
			this.socketAddress = socketAddress;
		}

		public virtual string GetUuid()
		{
			return uuid;
		}

	    public virtual IPEndPoint GetSocketAddress()
		{
			return socketAddress;
		}

		public virtual ClientType GetClientType()
		{
			return ClientType.Java;
		}
	}
}
