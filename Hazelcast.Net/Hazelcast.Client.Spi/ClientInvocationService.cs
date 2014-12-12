using System.Threading.Tasks;
using Hazelcast.Client.Request.Base;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Util;

namespace Hazelcast.Client.Spi
{
    internal sealed class ClientInvocationService : IClientInvocationService
    {
        private readonly HazelcastClient client;

        public ClientInvocationService(HazelcastClient client)
        {
            this.client = client;
        }

        public Task<IData> InvokeOnRandomTarget(ClientRequest request)
        {
            IRemotingService remotingService = GetRemotingService();
            return remotingService.Send(request);
        }

        public Task<IData> InvokeOnTarget(ClientRequest request, Address target)
        {
            IRemotingService remotingService = GetRemotingService();
            return remotingService.Send(request, target);
        }

        public Task<IData> InvokeOnKeyOwner(ClientRequest request, object key)
        {
            var partitionService = (ClientPartitionService) client.GetClientPartitionService();
            int partitionId = partitionService.GetPartitionId(key);
            Address owner = partitionService.GetPartitionOwner(partitionId);
            if (owner != null)
            {
                IRemotingService remotingService = GetRemotingService();
                return remotingService.Send(request, owner, partitionId);
            }
            return InvokeOnRandomTarget(request);
        }

        public Task<IData> InvokeOnRandomTarget(ClientRequest request, DistributedEventHandler handler)
        {
            IRemotingService remotingService = GetRemotingService();
            return remotingService.SendAndHandle(request, handler);
        }

        public Task<IData> InvokeOnTarget(ClientRequest request, Address target, DistributedEventHandler handler)
        {
            IRemotingService remotingService = GetRemotingService();
            return remotingService.SendAndHandle(request, target, handler);
        }

        public Task<IData> InvokeOnKeyOwner(ClientRequest request, object key, DistributedEventHandler handler)
        {
            var partitionService = (ClientPartitionService) client.GetClientPartitionService();
            Address owner = partitionService.GetPartitionOwner(partitionService.GetPartitionId(key));
            if (owner != null)
            {
                return InvokeOnTarget(request, owner, handler);
            }
            return InvokeOnRandomTarget(request, handler);
        }

        private IRemotingService GetRemotingService()
        {
            return client.GetRemotingService();
        }
    }
}