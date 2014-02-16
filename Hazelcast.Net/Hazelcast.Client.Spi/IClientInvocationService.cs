using System;
using System.Threading.Tasks;
using Hazelcast.Client.Request.Base;
using Hazelcast.IO;
using Hazelcast.Util;

namespace Hazelcast.Client.Spi
{
    /// <summary>
    /// invocation service
    /// </summary>
    public interface IClientInvocationService
    {
        /// <exception cref="System.Exception"></exception>
        Task<T> InvokeOnRandomTarget<T>(ClientRequest request);

        /// <exception cref="System.Exception"></exception>
        Task<T> InvokeOnTarget<T>(ClientRequest request, Address target);

        /// <exception cref="System.Exception"></exception>
        Task<T> InvokeOnKeyOwner<T>(ClientRequest request, object key);

        /// <exception cref="System.Exception"></exception>
        Task<T> InvokeOnRandomTarget<T>(ClientRequest request, DistributedEventHandler handler);

        /// <exception cref="System.Exception"></exception>
        Task<T> InvokeOnTarget<T>(ClientRequest request, Address target, DistributedEventHandler handler);

        /// <exception cref="System.Exception"></exception>
        Task<T> InvokeOnKeyOwner<T>(ClientRequest request, object key, DistributedEventHandler handler);

    }
}