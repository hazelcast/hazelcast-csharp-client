// Copyright (c) 2008-2017, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Util;

namespace Hazelcast.Client.Spi
{
    internal class ClientNonSmartInvocationService : ClientInvocationService
    {
        public ClientNonSmartInvocationService(HazelcastClient client) : base(client)
        {
        }

        public override IFuture<IClientMessage> InvokeListenerOnKeyOwner(IClientMessage request, object key,
            DistributedEventHandler handler,
            DecodeStartListenerResponse responseDecoder)
        {
            var partitionService = (ClientPartitionService) Client.GetClientPartitionService();
            var partitionId = partitionService.GetPartitionId(key);

            return SendToOwner(new ClientListenerInvocation(request, handler, responseDecoder, partitionId));
        }

        public override IFuture<IClientMessage> InvokeListenerOnPartition(IClientMessage request, int partitionId,
            DistributedEventHandler handler,
            DecodeStartListenerResponse responseDecoder)
        {
            return SendToOwner(new ClientListenerInvocation(request, handler, responseDecoder, partitionId));
        }

        public override IFuture<IClientMessage> InvokeListenerOnRandomTarget(IClientMessage request,
            DistributedEventHandler handler,
            DecodeStartListenerResponse responseDecoder)
        {
            return SendToOwner(new ClientListenerInvocation(request, handler, responseDecoder));
        }

        public override IFuture<IClientMessage> InvokeListenerOnTarget(IClientMessage request, Address target,
            DistributedEventHandler handler,
            DecodeStartListenerResponse responseDecoder)
        {
            return SendToOwner(new ClientListenerInvocation(request, handler, responseDecoder));
        }

        public override IFuture<IClientMessage> InvokeOnKeyOwner(IClientMessage request, object key)
        {
            var partitionService = (ClientPartitionService) Client.GetClientPartitionService();
            var partitionId = partitionService.GetPartitionId(key);

            return SendToOwner(new ClientInvocation(request, partitionId));
        }

        public override IFuture<IClientMessage> InvokeOnMember(IClientMessage request, IMember member)
        {
            return SendToOwner(new ClientInvocation(request, member.GetUuid()));
        }

        public override IFuture<IClientMessage> InvokeOnPartition(IClientMessage request, int partitionId)
        {
            return SendToOwner(new ClientInvocation(request, partitionId));
        }

        public override IFuture<IClientMessage> InvokeOnRandomTarget(IClientMessage request)
        {
            return SendToOwner(new ClientInvocation(request));
        }

        public override IFuture<IClientMessage> InvokeOnTarget(IClientMessage request, Address target)
        {
            return SendToOwner(new ClientInvocation(request));
        }

        private IFuture<IClientMessage> SendToOwner(ClientInvocation invocation)
        {
            var clusterService = Client.GetClientClusterService();
            var address = clusterService.GetOwnerConnectionAddress();
            return Invoke(invocation, address);
        }
    }
}