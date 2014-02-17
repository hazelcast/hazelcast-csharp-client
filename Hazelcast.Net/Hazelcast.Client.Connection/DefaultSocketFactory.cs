using System.Net.Sockets;

namespace Hazelcast.Client.Connection
{
    internal class DefaultSocketFactory : ISocketFactory
    {
        /// <exception cref="System.IO.IOException"></exception>
        public virtual Socket CreateSocket()
        {
            return new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
    }
}