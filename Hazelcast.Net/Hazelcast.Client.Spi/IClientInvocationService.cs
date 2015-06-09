using System;
using System.Threading.Tasks;
using Hazelcast.Client.Protocol;
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
        /// <exception cref="System.Exception"></exception>
        Task<IClientMessage> InvokeOnRandomTarget(IClientMessage request);

        /// <exception cref="System.Exception"></exception>
        Task<IClientMessage> InvokeOnTarget(IClientMessage request, Address target);

        /// <exception cref="System.Exception"></exception>
        Task<IClientMessage> InvokeOnKeyOwner(IClientMessage request, object key);

        /// <exception cref="System.Exception"></exception>
        Task<IClientMessage> InvokeOnRandomTarget(IClientMessage request, DistributedEventHandler handler);

        /// <exception cref="System.Exception"></exception>
        Task<IClientMessage> InvokeOnTarget(IClientMessage request, Address target, DistributedEventHandler handler);

        /// <exception cref="System.Exception"></exception>
        Task<IClientMessage> InvokeOnKeyOwner(IClientMessage request, object key, DistributedEventHandler handler);

    }
}