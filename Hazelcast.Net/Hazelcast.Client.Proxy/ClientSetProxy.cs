using System.Collections.Generic;
using Hazelcast.Client.Proxy;
using Hazelcast.Core;


namespace Hazelcast.Client.Proxy
{
	
	public class ClientSetProxy<E> : AbstractClientCollectionProxy<E>, IHazelcastSet<E>
	{
		public ClientSetProxy(string serviceName, string name) : base(serviceName, name)
		{
		}

	 
	}
}
