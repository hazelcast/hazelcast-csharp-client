using System.Net.Sockets;

namespace Hazelcast.IO
{
	public interface IMemberSocketInterceptor : ISocketInterceptor
	{
		/// <exception cref="System.IO.IOException"></exception>
		void OnAccept(Socket acceptedSocket);
	}
}
