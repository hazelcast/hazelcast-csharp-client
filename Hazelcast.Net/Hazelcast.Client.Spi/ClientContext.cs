// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Hazelcast.Config;
using Hazelcast.IO.Serialization;

#pragma warning disable CS1591
 namespace Hazelcast.Client.Spi
{
    internal sealed class ClientContext
    {
        private readonly ClientConfig _clientConfig;
        private readonly IClientClusterService _clusterService;

        private readonly IClientExecutionService _executionService;
        private readonly IClientInvocationService _invocationService;
        private readonly IClientListenerService _listenerService;
        private readonly IClientPartitionService _partitionService;

        private readonly ProxyManager _proxyManager;
        private readonly ISerializationService _serializationService;

        internal ClientContext(ISerializationService serializationService, IClientClusterService clusterService,
            IClientPartitionService partitionService, IClientInvocationService invocationService,
            IClientExecutionService executionService, IClientListenerService listenerService, ProxyManager proxyManager,
            ClientConfig clientConfig)
        {
            _serializationService = serializationService;
            _clusterService = clusterService;
            _partitionService = partitionService;
            _invocationService = invocationService;
            _executionService = executionService;
            _listenerService = listenerService;
            _proxyManager = proxyManager;
            _clientConfig = clientConfig;
        }

        public ClientConfig GetClientConfig()
        {
            return _clientConfig;
        }

        public IClientClusterService GetClusterService()
        {
            return _clusterService;
        }

        public IClientExecutionService GetExecutionService()
        {
            return _executionService;
        }

        public IClientInvocationService GetInvocationService()
        {
            return _invocationService;
        }

        public IClientListenerService GetListenerService()
        {
            return _listenerService;
        }

        public IClientPartitionService GetPartitionService()
        {
            return _partitionService;
        }

        public ISerializationService GetSerializationService()
        {
            return _serializationService;
        }

        public void RemoveProxy(ClientProxy proxy)
        {
            _proxyManager.RemoveProxy(proxy.GetServiceName(), proxy.GetName());
        }

        public HazelcastClient GetClient()
        {
            return _proxyManager.GetHazelcastInstance();
        }
    }
}