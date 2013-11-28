using System.Net.Sockets;
using Hazelcast.IO;


namespace Hazelcast.IO
{
	public interface IMemberSocketInterceptor : SocketInterceptor
	{
		/// <exception cref="System.IO.IOException"></exception>
		void OnAccept(Socket acceptedSocket);
	}
}
