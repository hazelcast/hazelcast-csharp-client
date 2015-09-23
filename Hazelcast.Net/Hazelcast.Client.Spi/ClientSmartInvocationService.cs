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
    class ClientSmartInvocationService : ClientInvocationService
    {
        public ClientSmartInvocationService(HazelcastClient client) : base(client)
        {

        }

        public override IFuture<IClientMessage> InvokeOnMember(IClientMessage request, IMember target)
        {
            return Invoke(new ClientInvocation(request, target.GetUuid()), target.GetAddress());
        }

        public override IFuture<IClientMessage> InvokeOnTarget(IClientMessage request, Address target)
        {
            return Invoke(new ClientInvocation(request, target), target);
        }

        public override IFuture<IClientMessage> InvokeOnPartition(IClientMessage request, int partitionId)
        {
            var partitionService = (ClientPartitionService)Client.GetClientPartitionService();
            var owner = partitionService.GetPartitionOwner(partitionId);
            return Invoke(new ClientInvocation(request, partitionId), owner);
        }

        public override IFuture<IClientMessage> InvokeListenerOnPartition(IClientMessage request, int partitionId, DistributedEventHandler handler,
           DecodeStartListenerResponse responseDecoder)
        {
            var partitionService = (ClientPartitionService)Client.GetClientPartitionService();
            var owner = partitionService.GetPartitionOwner(partitionId);
            return Invoke(new ClientListenerInvocation(request, handler, responseDecoder, partitionId), owner);
        }

        public override IFuture<IClientMessage> InvokeListenerOnTarget(IClientMessage request, Address target, DistributedEventHandler handler,
           DecodeStartListenerResponse responseDecoder)
        {
            return Invoke(new ClientListenerInvocation(request, handler, responseDecoder, target), target);
        }

        public override IFuture<IClientMessage> InvokeOnKeyOwner(IClientMessage request, object key)
        {
            var partitionService = (ClientPartitionService)Client.GetClientPartitionService();
            var partitionId = partitionService.GetPartitionId(key);
            var owner = partitionService.GetPartitionOwner(partitionId);
            return Invoke(new ClientInvocation(request, partitionId), owner);
        }

        public override IFuture<IClientMessage> InvokeListenerOnKeyOwner(IClientMessage request, object key, DistributedEventHandler handler,
            DecodeStartListenerResponse responseDecoder)
        {
            var partitionService = (ClientPartitionService)Client.GetClientPartitionService();
            var partitionId = partitionService.GetPartitionId(key);
            var owner = partitionService.GetPartitionOwner(partitionId);

            return Invoke(new ClientListenerInvocation(request, handler, responseDecoder, partitionId), owner);
        }

        public override IFuture<IClientMessage> InvokeOnRandomTarget(IClientMessage request)
        {
            return Invoke(new ClientInvocation(request));
        }

        public override IFuture<IClientMessage> InvokeListenerOnRandomTarget(IClientMessage request, DistributedEventHandler handler,
            DecodeStartListenerResponse responseDecoder)
        {
            return Invoke(new ClientListenerInvocation(request, handler, responseDecoder));
        }
    }
}
