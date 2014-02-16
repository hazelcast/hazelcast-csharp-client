using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hazelcast.Client.Request.Base;
using Hazelcast.IO;
using Hazelcast.Util;

namespace Hazelcast.Client.Connection
{
    public interface IRemotingService
    {

        /// <summary>
        /// </summary>
        /// <param name="request"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<TResult> Send<TResult>(ClientRequest request);

        /// <summary>
        /// </summary>
        /// <param name="request"></param>
        /// <param name="target"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<TResult> Send<TResult>(ClientRequest request, Address target);

        /// <summary>
        /// </summary>
        /// <param name="request"></param>
        /// <param name="handler"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<TResult> SendAndHandle<TResult>(ClientRequest request, DistributedEventHandler handler);

        /// <summary>
        /// </summary>
        /// <param name="request"></param>
        /// <param name="target"></param>
        /// <param name="handler"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<TResult> SendAndHandle<TResult>(ClientRequest request, Address target, DistributedEventHandler handler);

        /// <summary>
        /// </summary>
        /// <param name="uuid"></param>
        /// <param name="callId"></param>
        void RegisterListener(string registrationId, int callId);

        /// <summary>
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>
        bool UnregisterListener(string registrationId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uuidregistrationId"></param>
        /// <param name="alias"></param>
        /// <param name="callId"></param>
        void ReRegisterListener(string uuidregistrationId, string alias, int callId);

    }
}
