using System;
using Hazelcast.Security;

namespace Hazelcast.Client
{
	public interface ClientBinder
	{
		void bind(Connection connection, Credentials credentials);
	}
}

