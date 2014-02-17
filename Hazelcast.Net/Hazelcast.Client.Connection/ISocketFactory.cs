using System.Net.Sockets;

namespace Hazelcast.Client.Connection
{
    public interface ISocketFactory
    {
        /// <exception cref="System.IO.IOException"></exception>
        Socket CreateSocket();
    }
}