using Hazelcast.Client.Protocol;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.Util;

namespace Hazelcast.Client.Spi
{
    /// <summary>
    ///     invocation service
    /// </summary>
    public interface IClientInvocationService
    {
        IFuture<IClientMessage> InvokeListenerOnKeyOwner(IClientMessage request, object key,
            DistributedEventHandler handler, DecodeStartListenerResponse responseDecoder);
        IFuture<IClientMessage> InvokeListenerOnPartition(IClientMessage request, int partitionId,
            DistributedEventHandler handler,
            DecodeStartListenerResponse responseDecoder);
        IFuture<IClientMessage> InvokeListenerOnRandomTarget(IClientMessage request,
            DistributedEventHandler handler, DecodeStartListenerResponse responseDecoder);
        IFuture<IClientMessage> InvokeListenerOnTarget(IClientMessage request, Address target,
            DistributedEventHandler handler, DecodeStartListenerResponse responseDecoder);

        IFuture<IClientMessage> InvokeOnKeyOwner(IClientMessage request, object key);
        IFuture<IClientMessage> InvokeOnMember(IClientMessage request, IMember member);
        IFuture<IClientMessage> InvokeOnPartition(IClientMessage request, int partitionId);
        IFuture<IClientMessage> InvokeOnRandomTarget(IClientMessage request);
        IFuture<IClientMessage> InvokeOnTarget(IClientMessage request, Address target);
        bool RemoveEventHandler(int correlationId);
    }
}