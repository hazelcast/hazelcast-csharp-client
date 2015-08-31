using System;
using System.Threading.Tasks;
using Hazelcast.Client.Protocol;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Util;

namespace Hazelcast.Client.Spi
{
    /// <summary>
    /// invocation service
    /// </summary>
    public interface IClientInvocationService
    {
        IFuture<IClientMessage> InvokeOnTarget(IClientMessage request, Address target, DistributedEventHandler handler = null);
        IFuture<IClientMessage> InvokeOnMember(IClientMessage request, IMember member, DistributedEventHandler handler = null);
        IFuture<IClientMessage> InvokeOnKeyOwner(IClientMessage request, object key, DistributedEventHandler handler = null);
        IFuture<IClientMessage> InvokeOnRandomTarget(IClientMessage request, DistributedEventHandler handler = null);

        bool RemoveEventHandler(int correlationId);
    }
}