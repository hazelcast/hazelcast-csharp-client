/*
* Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using Hazelcast.Client.Connection;
using Hazelcast.Config;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Spi
{
    internal sealed class ClientContext
    {
        private readonly ClientConfig clientConfig;
        private readonly IClientClusterService clusterService;

        private readonly IClientExecutionService executionService;
        private readonly IClientInvocationService invocationService;
        private readonly IClientPartitionService partitionService;

        private readonly ProxyManager proxyManager;
        private readonly ISerializationService serializationService;
        private readonly IClientListenerService listenerService;

        internal ClientContext(ISerializationService serializationService, IClientClusterService clusterService,
            IClientPartitionService partitionService, IClientInvocationService invocationService,
            IClientExecutionService executionService, IClientListenerService listenerService, ProxyManager proxyManager, ClientConfig clientConfig)
        {
            this.serializationService = serializationService;
            this.clusterService = clusterService;
            this.partitionService = partitionService;
            this.invocationService = invocationService;
            this.executionService = executionService;
            this.listenerService = listenerService;
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

        public IClientListenerService GetListenerService()
        {
            return listenerService;
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