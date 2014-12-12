using System.Threading.Tasks;
using Hazelcast.Client.Request.Base;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Util;

namespace Hazelcast.Client.Spi
{
    public interface IRemotingService
    {

        /// <summary>
        /// </summary>
        /// <param name="request"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<IData> Send(ClientRequest request);

        /// <summary>
        /// </summary>
        /// <param name="request"></param>
        /// <param name="target"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<IData> Send(ClientRequest request, Address target);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="request"></param>
        /// <param name="target"></param>
        /// <param name="partitionId"></param>
        /// <returns></returns>
        Task<IData> Send(ClientRequest request, Address target, int partitionId);

        /// <summary>
        /// </summary>
        /// <param name="request"></param>
        /// <param name="handler"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<IData> SendAndHandle(ClientRequest request, DistributedEventHandler handler);

        /// <summary>
        /// </summary>
        /// <param name="request"></param>
        /// <param name="target"></param>
        /// <param name="handler"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<IData> SendAndHandle(ClientRequest request, Address target, DistributedEventHandler handler);

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
