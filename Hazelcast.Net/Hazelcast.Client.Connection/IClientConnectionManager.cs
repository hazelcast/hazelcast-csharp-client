using Hazelcast.Client.Spi;
using Hazelcast.IO;
using Hazelcast.Util;

namespace Hazelcast.Client.Connection
{
    internal interface IClientConnectionManager : IRemotingService
    {
        bool Live { get; }

        void AddConnectionHeartBeatListener(IConnectionHeartbeatListener connectonHeartbeatListener);
        void AddConnectionListener(IConnectionListener connectionListener);
        Address BindToRandomAddress();
        void DestroyConnection(ClientConnection clientConnection);

        /// <exception cref="System.IO.IOException"></exception>
        ClientConnection GetOrConnect(Address address, Authenticator authenticator);

        bool Shutdown();
        void Start();
    }
}