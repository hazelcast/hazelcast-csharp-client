using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hazelcast.Client.Request.Base;
using Hazelcast.Core;
using Hazelcast.IO;

namespace Hazelcast.Client.Spi
{
    /// <summary>
    ///     Client Cluster Service
    /// </summary>
    public interface IClientClusterService
    {
        /// <summary>
        ///     start service
        /// </summary>
        void Start();

        /// <summary>
        ///     stop service
        /// </summary>
        void Stop();

        /// <summary>
        ///     get member for address
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        IMember GetMember(Address address);

        /// <summary>
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>
        IMember GetMember(string uuid);

        /// <summary>
        ///     memberlist
        /// </summary>
        /// <returns></returns>
        ICollection<IMember> GetMemberList();

        /// <summary>
        ///     master Address
        /// </summary>
        /// <returns></returns>
        Address GetMasterAddress();

        /// <summary>
        ///     cluster size
        /// </summary>
        /// <returns></returns>
        int GetSize();

        /// <summary>
        ///     cluster time
        /// </summary>
        /// <returns></returns>
        long GetClusterTime();

        /// <summary>
        ///     local client
        /// </summary>
        /// <returns></returns>
        IClient GetLocalClient();

        /// <summary>
        ///     member string
        /// </summary>
        /// <returns></returns>
        string MembersString();

        /// <summary>
        /// </summary>
        /// <param name="listener"></param>
        /// <returns></returns>
        string AddMembershipListener(IMembershipListener listener);

        /// <summary>
        /// </summary>
        /// <param name="registrationId"></param>
        /// <returns></returns>
        bool RemoveMembershipListener(string registrationId);


        ///// <summary>
        ///// </summary>
        ///// <param name="request"></param>
        ///// <typeparam name="T"></typeparam>
        ///// <returns></returns>
        //Task<T> Send<T>(ClientRequest request);

        ///// <summary>
        ///// </summary>
        ///// <param name="request"></param>
        ///// <param name="target"></param>
        ///// <typeparam name="T"></typeparam>
        ///// <returns></returns>
        //Task<T> Send<T>(ClientRequest request, Address target);

        ///// <summary>
        ///// </summary>
        ///// <param name="request"></param>
        ///// <param name="handler"></param>
        ///// <typeparam name="T"></typeparam>
        ///// <returns></returns>
        //Task<T> SendAndHandle<T>(ClientRequest request, EventHandler<T> handler) where T : EventArgs;

        ///// <summary>
        ///// </summary>
        ///// <param name="request"></param>
        ///// <param name="target"></param>
        ///// <param name="handler"></param>
        ///// <typeparam name="T"></typeparam>
        ///// <returns></returns>
        //Task<T> SendAndHandle<T>(ClientRequest request, Address target, EventHandler<T> handler) where T : EventArgs;

        ///// <summary>
        ///// </summary>
        ///// <param name="uuid"></param>
        ///// <param name="callId"></param>
        //void RegisterListener(string uuid, long callId);

        ///// <summary>
        ///// </summary>
        ///// <param name="uuid"></param>
        ///// <returns></returns>
        //bool UnregisterListener(string uuid);
    }
}