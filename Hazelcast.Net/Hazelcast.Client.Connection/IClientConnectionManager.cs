using Hazelcast.Client.Spi;
using Hazelcast.IO;
using Hazelcast.Util;

namespace Hazelcast.Client.Connection
{
    internal interface IClientConnectionManager
    {
        bool Live { get; }

        void AddConnectionHeartBeatListener(IConnectionHeartbeatListener connectonHeartbeatListener);
        void AddConnectionListener(IConnectionListener connectionListener);
        Address BindToRandomAddress();
        void DestroyConnection(ClientConnection clientConnection);

        /// <exception cref="System.IO.IOException"></exception>
        ClientConnection GetOrConnect(Address address, Authenticator authenticator);
        ClientConnection GetOrConnectWithRetry(Address address);

        bool Shutdown();
        void Start();
    }
}