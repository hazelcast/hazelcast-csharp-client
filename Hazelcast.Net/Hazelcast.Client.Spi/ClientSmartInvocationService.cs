// Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
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
    internal class ClientSmartInvocationService : ClientInvocationService
    {
        public ClientSmartInvocationService(HazelcastClient client) : base(client)
        {
        }

        public override IFuture<IClientMessage> InvokeListenerOnKeyOwner(IClientMessage request, object key,
            DistributedEventHandler handler,
            DecodeStartListenerResponse responseDecoder)
        {
            var partitionService = (ClientPartitionService) Client.GetClientPartitionService();
            var partitionId = partitionService.GetPartitionId(key);
            var owner = partitionService.GetPartitionOwner(partitionId);

            return Invoke(new ClientListenerInvocation(request, handler, responseDecoder, partitionId), owner);
        }

        public override IFuture<IClientMessage> InvokeListenerOnPartition(IClientMessage request, int partitionId,
            DistributedEventHandler handler,
            DecodeStartListenerResponse responseDecoder)
        {
            var partitionService = (ClientPartitionService) Client.GetClientPartitionService();
            var owner = partitionService.GetPartitionOwner(partitionId);
            return Invoke(new ClientListenerInvocation(request, handler, responseDecoder, partitionId), owner);
        }

        public override IFuture<IClientMessage> InvokeListenerOnRandomTarget(IClientMessage request,
            DistributedEventHandler handler,
            DecodeStartListenerResponse responseDecoder)
        {
            return Invoke(new ClientListenerInvocation(request, handler, responseDecoder));
        }

        public override IFuture<IClientMessage> InvokeListenerOnTarget(IClientMessage request, Address target,
            DistributedEventHandler handler,
            DecodeStartListenerResponse responseDecoder)
        {
            return Invoke(new ClientListenerInvocation(request, handler, responseDecoder, target), target);
        }

        public override IFuture<IClientMessage> InvokeOnKeyOwner(IClientMessage request, object key)
        {
            var partitionService = (ClientPartitionService) Client.GetClientPartitionService();
            var partitionId = partitionService.GetPartitionId(key);
            var owner = partitionService.GetPartitionOwner(partitionId);
            return Invoke(new ClientInvocation(request, partitionId), owner);
        }

        public override IFuture<IClientMessage> InvokeOnMember(IClientMessage request, IMember target)
        {
            return Invoke(new ClientInvocation(request, target.GetUuid()), target.GetAddress());
        }

        public override IFuture<IClientMessage> InvokeOnPartition(IClientMessage request, int partitionId)
        {
            var partitionService = (ClientPartitionService) Client.GetClientPartitionService();
            var owner = partitionService.GetPartitionOwner(partitionId);
            return Invoke(new ClientInvocation(request, partitionId), owner);
        }

        public override IFuture<IClientMessage> InvokeOnRandomTarget(IClientMessage request)
        {
            return Invoke(new ClientInvocation(request));
        }

        public override IFuture<IClientMessage> InvokeOnTarget(IClientMessage request, Address target)
        {
            return Invoke(new ClientInvocation(request, target), target);
        }
    }
}