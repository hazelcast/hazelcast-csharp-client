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
            var clientConnection = GetConnection(target.GetAddress());
            return Send(clientConnection, new ClientInvocation(request, memberUuid: target.GetUuid()));
        }

        public override IFuture<IClientMessage> InvokeOnTarget(IClientMessage request, Address target)
        {
            var clientConnection = GetConnection(target);
            return Send(clientConnection, new ClientInvocation(request));
        }

        public override IFuture<IClientMessage> InvokeListenerOnTarget(IClientMessage request, Address target, DistributedEventHandler handler,
           DecodeStartListenerResponse responseDecoder)
        {
            var clientConnection = GetConnection(target);
            return Send(clientConnection, new ClientListenerInvocation(request, handler, responseDecoder));
        }

        public override IFuture<IClientMessage> InvokeOnKeyOwner(IClientMessage request, object key)
        {
            var partitionService = (ClientPartitionService)Client.GetClientPartitionService();
            var partitionId = partitionService.GetPartitionId(key);
            var owner = partitionService.GetPartitionOwner(partitionId);
            var connection = GetConnection(owner);

            return Send(connection, new ClientInvocation(request, partitionId));
        }

        public override IFuture<IClientMessage> InvokeListenerOnKeyOwner(IClientMessage request, object key, DistributedEventHandler handler,
            DecodeStartListenerResponse responseDecoder)
        {
            var partitionService = (ClientPartitionService)Client.GetClientPartitionService();
            var partitionId = partitionService.GetPartitionId(key);
            var owner = partitionService.GetPartitionOwner(partitionId);
            var connection = GetConnection(owner);

            return Send(connection, new ClientListenerInvocation(request, handler, responseDecoder, partitionId));
        }

        public override IFuture<IClientMessage> InvokeOnRandomTarget(IClientMessage request)
        {
            var clientConnection = GetConnection();
            return Send(clientConnection, new ClientInvocation(request));
        }

        public override IFuture<IClientMessage> InvokeListenerOnRandomTarget(IClientMessage request, DistributedEventHandler handler,
            DecodeStartListenerResponse responseDecoder)
        {
            var clientConnection = GetConnection();
            return Send(clientConnection, new ClientListenerInvocation(request, handler, responseDecoder));
        }
    }
}
