using System.Threading.Tasks;
using Hazelcast.Client.Connection;
using Hazelcast.Client.Protocol;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.Util;

namespace Hazelcast.Client.Spi
{
    internal sealed class ClientInvocationService : IClientInvocationService
    {
        private readonly HazelcastClient _client;
        private readonly ClientConnectionManager _clientConnectionManager;

        public ClientInvocationService(HazelcastClient client)
        {
            _client = client;
            _clientConnectionManager = (ClientConnectionManager)client.GetConnectionManager();
        }

        public Task<IClientMessage> InvokeOnMember(IClientMessage request, IMember target, DistributedEventHandler handler = null)
        {
            return Send(request, target, handler: handler);
        }

        public Task<IClientMessage> InvokeOnTarget(IClientMessage request, Address target, DistributedEventHandler handler = null)
        {
            return Send(request, target, handler: handler);
        }

        public Task<IClientMessage> InvokeOnKeyOwner(IClientMessage request, object key, DistributedEventHandler handler = null)
        {
            var partitionService = (ClientPartitionService) _client.GetClientPartitionService();
            var partitionId = partitionService.GetPartitionId(key);
            var owner = partitionService.GetPartitionOwner(partitionId);
            if (owner != null)
            {
                return Send(request, owner, partitionId, handler);
            }
            return InvokeOnRandomTarget(request);
        }

        public Task<IClientMessage> InvokeOnRandomTarget(IClientMessage request, DistributedEventHandler handler = null)
        {
            return Send(request, handler);
        }

        private Task<IClientMessage> Send(IClientMessage request, Address target, int partitionId = -1, DistributedEventHandler handler = null)
        {
            //TODO: add retry mechanism
            var clientConnection = GetConnection(target);
            return clientConnection.Send(new ClientInvocation(request, partitionId, handler: handler));
        }

        private Task<IClientMessage> Send(IClientMessage request, DistributedEventHandler handler = null)
        {
            var clientConnection = GetConnection(null);
            return clientConnection.Send(new ClientInvocation(request, handler: handler));
        }

        private Task<IClientMessage> Send(IClientMessage request, IMember target, int partitionId = -1, DistributedEventHandler handler = null)
        {
            var clientConnection = GetConnection(target.GetAddress());
            return clientConnection.Send(new ClientInvocation(request, partitionId, target.GetUuid(), handler));
        }


        private ClientConnection GetConnection(Address address)
        {
            return _clientConnectionManager.GetOrConnectWithRetry(address);
        }
    }
}