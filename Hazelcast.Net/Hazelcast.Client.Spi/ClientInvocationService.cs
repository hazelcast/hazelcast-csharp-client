using Hazelcast.IO;

namespace Hazelcast.Client.Spi
{
    public sealed class ClientInvocationService : IClientInvocationService
    {
        private readonly HazelcastClient client;

        public ClientInvocationService(HazelcastClient client)
        {
            this.client = client;
        }

        /// <exception cref="System.Exception"></exception>
        public T InvokeOnRandomTarget<T>(object request)
        {
            ClientClusterService clusterService = GetClusterService();
            return clusterService.SendAndReceive<T>(null, request);
        }

        /// <exception cref="System.Exception"></exception>
        public T InvokeOnTarget<T>(object request, Address target)
        {
            ClientClusterService clusterService = GetClusterService();
            return clusterService.SendAndReceive<T>(target, request);
        }

        /// <exception cref="System.Exception"></exception>
        public T InvokeOnKeyOwner<T>(object request, object key)
        {
            var partitionService = (ClientPartitionService) client.GetClientPartitionService();
            Address owner = partitionService.GetPartitionOwner(partitionService.GetPartitionId(key));
            if (owner != null)
            {
                return InvokeOnTarget<T>(request, owner);
            }
            return InvokeOnRandomTarget<T>(request);
        }

        /// <exception cref="System.Exception"></exception>
        public void InvokeOnRandomTarget(object request, ResponseHandler handler)
        {
            ClientClusterService clusterService = GetClusterService();
            clusterService.SendAndHandle(null, request, handler);
        }

        /// <exception cref="System.Exception"></exception>
        public void InvokeOnTarget(object request, Address target, ResponseHandler handler)
        {
            ClientClusterService clusterService = GetClusterService();
            clusterService.SendAndHandle(target, request, handler);
        }

        /// <exception cref="System.Exception"></exception>
        public void InvokeOnKeyOwner(object request, object key, ResponseHandler handler)
        {
            var partitionService = (ClientPartitionService) client.GetClientPartitionService();
            Address owner = partitionService.GetPartitionOwner(partitionService.GetPartitionId(key));
            if (owner != null)
            {
                InvokeOnTarget(request, owner, handler);
            }
            InvokeOnRandomTarget(request, handler);
        }

        private ClientClusterService GetClusterService()
        {
            return (ClientClusterService) client.GetClientClusterService();
        }
    }
}