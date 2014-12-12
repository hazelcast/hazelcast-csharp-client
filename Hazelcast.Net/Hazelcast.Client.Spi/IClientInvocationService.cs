using System;
using System.Threading.Tasks;
using Hazelcast.Client.Request.Base;
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
        Task<IData> InvokeOnRandomTarget(ClientRequest request);

        /// <exception cref="System.Exception"></exception>
        Task<IData> InvokeOnTarget(ClientRequest request, Address target);

        /// <exception cref="System.Exception"></exception>
        Task<IData> InvokeOnKeyOwner(ClientRequest request, object key);

        /// <exception cref="System.Exception"></exception>
        Task<IData> InvokeOnRandomTarget(ClientRequest request, DistributedEventHandler handler);

        /// <exception cref="System.Exception"></exception>
        Task<IData> InvokeOnTarget(ClientRequest request, Address target, DistributedEventHandler handler);

        /// <exception cref="System.Exception"></exception>
        Task<IData> InvokeOnKeyOwner(ClientRequest request, object key, DistributedEventHandler handler);

    }
}