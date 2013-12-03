using System;
using System.Net.Sockets;

namespace Hazelcast.Client
{
	public interface SocketInterceptor
	{
		void onConnect(Socket connectedSocket);
	}
}

