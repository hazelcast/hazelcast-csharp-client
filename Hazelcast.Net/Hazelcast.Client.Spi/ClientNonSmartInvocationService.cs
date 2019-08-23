// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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

using Hazelcast.Client.Protocol;
using Hazelcast.Core;
using Hazelcast.IO;

#pragma warning disable CS1591
namespace Hazelcast.Client.Spi
{
    class ClientNonSmartInvocationService : ClientInvocationService
    {
        public ClientNonSmartInvocationService(HazelcastClient client) : base(client)
        {
        }

        public override void InvokeOnKeyOwner(IClientMessage request, IFuture<IClientMessage> future, object key)
        {
            var partitionService = (ClientPartitionService)Client.GetClientPartitionService();
            var partitionId = partitionService.GetPartitionId(key);

            SendToOwner(new ClientInvocation(request, future, partitionId));
        }

        public override void InvokeOnMember(IClientMessage request, IFuture<IClientMessage> future, IMember member)
        {
            SendToOwner(new ClientInvocation(request, future, member.GetUuid()));
        }

        public override void InvokeOnPartition(IClientMessage request, IFuture<IClientMessage> future, int partitionId)
        {
            SendToOwner(new ClientInvocation(request, future, partitionId));
        }

        public override void InvokeOnRandomTarget(IClientMessage request, IFuture<IClientMessage> future)
        {
            SendToOwner(new ClientInvocation(request, future));
        }

        public override void InvokeOnTarget(IClientMessage request, IFuture<IClientMessage> future, Address target)
        {
            SendToOwner(new ClientInvocation(request, future));
        }

        protected override Address GetRandomAddress()
        {
            return Client.GetClientClusterService().GetOwnerConnectionAddress();
        }

        void SendToOwner(ClientInvocation invocation)
        {
            var clusterService = Client.GetClientClusterService();
            var address = clusterService.GetOwnerConnectionAddress();
            Invoke(invocation, address);
        }
    }
}