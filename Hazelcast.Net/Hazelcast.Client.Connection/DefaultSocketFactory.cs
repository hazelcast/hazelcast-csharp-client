using System.Net.Sockets;
using Hazelcast.Client.Connection;


namespace Hazelcast.Client.Connection
{
	public class DefaultSocketFactory : ISocketFactory
	{
		/// <exception cref="System.IO.IOException"></exception>
		public virtual Socket CreateSocket()
		{
            return new Socket(AddressFamily.InterNetwork,SocketType.Stream, ProtocolType.Tcp);
		}
	}
}
