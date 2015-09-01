using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hazelcast.Client.Protocol;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.Util;

namespace Hazelcast.Client.Spi
{
    class ClientNonSmartInvocationService : ClientInvocationService
    {
        public ClientNonSmartInvocationService(HazelcastClient client) : base(client)
        {
        }

        public override IFuture<IClientMessage> InvokeListenerOnKeyOwner(IClientMessage request, object key, DistributedEventHandler handler,
            DecodeStartListenerResponse responseDecoder)
        {
            var partitionService = (ClientPartitionService)Client.GetClientPartitionService();
            var partitionId = partitionService.GetPartitionId(key);

            return SendToOwner(new ClientListenerInvocation(request, handler, responseDecoder, partitionId));
        }

        public override IFuture<IClientMessage> InvokeListenerOnRandomTarget(IClientMessage request, DistributedEventHandler handler,
            DecodeStartListenerResponse responseDecoder)
        {
            return SendToOwner(new ClientListenerInvocation(request, handler, responseDecoder));
        }

        public override IFuture<IClientMessage> InvokeListenerOnTarget(IClientMessage request, Address target, DistributedEventHandler handler,
            DecodeStartListenerResponse responseDecoder)
        {
            return SendToOwner(new ClientListenerInvocation(request, handler, responseDecoder));
        }

        public override IFuture<IClientMessage> InvokeOnKeyOwner(IClientMessage request, object key)
        {
            var partitionService = (ClientPartitionService)Client.GetClientPartitionService();
            var partitionId = partitionService.GetPartitionId(key);

            return SendToOwner(new ClientInvocation(request, partitionId));
        }

        public override IFuture<IClientMessage> InvokeOnMember(IClientMessage request, IMember member)
        {
            return SendToOwner(new ClientInvocation(request, memberUuid:member.GetUuid()));
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
            return Send(GetConnection(address), invocation);
        } 
    }
}
