using System.Net;
using Hazelcast.Core;

namespace Hazelcast.Client
{
    public class Client : IClient
    {
        private readonly IPEndPoint socketAddress;
        private readonly string uuid;

        public Client(string uuid, IPEndPoint socketAddress)
        {
            this.uuid = uuid;
            this.socketAddress = socketAddress;
        }

        public virtual string GetUuid()
        {
            return uuid;
        }

        public virtual IPEndPoint GetSocketAddress()
        {
            return socketAddress;
        }

        public virtual ClientType GetClientType()
        {
            return ClientType.Java;
        }
    }
}