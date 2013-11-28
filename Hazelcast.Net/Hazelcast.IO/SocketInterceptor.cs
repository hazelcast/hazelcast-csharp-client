using System.Collections.Generic;
using System.Net.Sockets;
using Hazelcast.IO;
using Hazelcast.Net.Ext;


namespace Hazelcast.IO
{
	public interface SocketInterceptor
	{
		void Init(Dictionary<string,string> properties);

		/// <exception cref="System.IO.IOException"></exception>
		void OnConnect(Socket connectedSocket);
	}
}
