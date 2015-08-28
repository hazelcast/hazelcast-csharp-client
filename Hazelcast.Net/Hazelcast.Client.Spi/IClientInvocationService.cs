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
        Task<IClientMessage> InvokeOnTarget(IClientMessage request, Address target, DistributedEventHandler handler = null);
        Task<IClientMessage> InvokeOnMember(IClientMessage request, IMember member, DistributedEventHandler handler = null);
        Task<IClientMessage> InvokeOnKeyOwner(IClientMessage request, object key, DistributedEventHandler handler = null);
        Task<IClientMessage> InvokeOnRandomTarget(IClientMessage request, DistributedEventHandler handler = null);

        bool RemoveEventHandler(int correlationId);
    }
}