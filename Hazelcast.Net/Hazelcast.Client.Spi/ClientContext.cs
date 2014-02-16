using Hazelcast.Client.Connection;
using Hazelcast.Config;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Spi
{
    public sealed class ClientContext
    {
        private readonly ClientConfig clientConfig;
        private readonly IClientClusterService clusterService;

        private readonly IClientExecutionService executionService;
        private readonly IClientInvocationService invocationService;
        private readonly IClientPartitionService partitionService;

        private readonly ProxyManager proxyManager;
        private readonly ISerializationService serializationService;
        private IRemotingService _remotingService;

        internal ClientContext(ISerializationService serializationService, IClientClusterService clusterService,
            IClientPartitionService partitionService, IClientInvocationService invocationService,
            IClientExecutionService executionService, IRemotingService remotingService,ProxyManager proxyManager, ClientConfig clientConfig)
        {
            this.serializationService = serializationService;
            this.clusterService = clusterService;
            this.partitionService = partitionService;
            this.invocationService = invocationService;
            this.executionService = executionService;
            this._remotingService = remotingService;
            this.proxyManager = proxyManager;
            this.clientConfig = clientConfig;
        }

        public ISerializationService GetSerializationService()
        {
            return serializationService;
        }

        public IClientClusterService GetClusterService()
        {
            return clusterService;
        }

        public IClientPartitionService GetPartitionService()
        {
            return partitionService;
        }

        public IClientInvocationService GetInvocationService()
        {
            return invocationService;
        }

        public IClientExecutionService GetExecutionService()
        {
            return executionService;
        }

        public IRemotingService GetRemotingService()
        {
            return _remotingService;
        }

        public void RemoveProxy(ClientProxy proxy)
        {
            proxyManager.RemoveProxy(proxy.GetServiceName(), proxy.GetName());
        }

        public ClientConfig GetClientConfig()
        {
            return clientConfig;
        }
    }
}