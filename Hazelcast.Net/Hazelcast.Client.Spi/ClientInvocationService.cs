using System;
using System.Threading.Tasks;
using Hazelcast.Client.Connection;
using Hazelcast.Client.Request.Base;
using Hazelcast.IO;
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

        private IRemotingService GetRemotingService()
        {
            return client.GetRemotingService();
        }

        public Task<T> InvokeOnRandomTarget<T>(ClientRequest request)
        {
            var remotingService = GetRemotingService();
            return remotingService.Send<T>(request);
        }

        public Task<T> InvokeOnTarget<T>(ClientRequest request, Address target)
        {
            var remotingService = GetRemotingService();
            return remotingService.Send<T>(request, target);
        }

        public Task<T> InvokeOnKeyOwner<T>(ClientRequest request, object key)
        {
            var partitionService = (ClientPartitionService)client.GetClientPartitionService();
            var partitionId = partitionService.GetPartitionId(key);
            Address owner = partitionService.GetPartitionOwner(partitionId);
            if (owner != null)
            {
                return InvokeOnTarget<T>(request, owner);
            }
            return InvokeOnRandomTarget<T>(request);
        }

        public Task<T> InvokeOnRandomTarget<T>(ClientRequest request, DistributedEventHandler handler)
        {
            var remotingService = GetRemotingService();
            return remotingService.SendAndHandle<T>(request, handler);

        }

        public Task<T> InvokeOnTarget<T>(ClientRequest request, Address target, DistributedEventHandler handler)
        {
            var remotingService = GetRemotingService();
            return remotingService.SendAndHandle<T>(request, target, handler);
        }

        public Task<T> InvokeOnKeyOwner<T>(ClientRequest request, object key, DistributedEventHandler handler)
        {
            var partitionService = (ClientPartitionService)client.GetClientPartitionService();
            Address owner = partitionService.GetPartitionOwner(partitionService.GetPartitionId(key));
            if (owner != null)
            {
                return InvokeOnTarget<T>(request, owner, handler);
            }
            return InvokeOnRandomTarget<T>(request, handler);
        }


    }
}