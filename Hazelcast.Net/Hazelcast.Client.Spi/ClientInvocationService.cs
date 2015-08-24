using System.Threading.Tasks;
using Hazelcast.Client.Protocol;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.Util;

namespace Hazelcast.Client.Spi
{
    internal sealed class ClientInvocationService : IClientInvocationService
    {
        private readonly HazelcastClient _client;

        public ClientInvocationService(HazelcastClient client)
        {
            this._client = client;
        }

        public Task<IClientMessage> InvokeOnMember(IClientMessage request, IMember target,
            DistributedEventHandler handler = null)
        {
            var remotingService = GetRemotingService();
            return remotingService.Send(request, target, handler: handler);
        }

        public Task<IClientMessage> InvokeOnKeyOwner(IClientMessage request, object key,
            DistributedEventHandler handler = null)
        {
            var partitionService = (ClientPartitionService) _client.GetClientPartitionService();
            var partitionId = partitionService.GetPartitionId(key);
            var owner = partitionService.GetPartitionOwner(partitionId);
            if (owner != null)
            {
                var remotingService = GetRemotingService();
                return remotingService.Send(request, owner, partitionId, handler);
            }
            return InvokeOnRandomTarget(request, handler);
        }

        public Task<IClientMessage> InvokeOnRandomTarget(IClientMessage request, DistributedEventHandler handler = null)
        {
            var remotingService = GetRemotingService();
            return remotingService.Send(request, handler);
        }

        public Task<IClientMessage> InvokeOnTarget(IClientMessage request, Address target,
            DistributedEventHandler handler = null)
        {
            var remotingService = GetRemotingService();
            return remotingService.Send(request, target, handler: handler);
        }

        private IRemotingService GetRemotingService()
        {
            return _client.GetRemotingService();
        }
    }
}