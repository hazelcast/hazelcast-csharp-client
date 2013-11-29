using System.Collections.Generic;
using Hazelcast.Client.Connection;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.Util;

namespace Hazelcast.Client.Spi
{
    public interface IClientClusterService
    {
        void Start();
        void Stop();
        IMember GetMember(Address address);
        IMember GetMember(string uuid);
        ICollection<IMember> GetMemberList();

        Address GetMasterAddress();
        int GetSize();
        long GetClusterTime();
        Client GetLocalClient();

        /// <exception cref="System.IO.IOException"></exception>
        T SendAndReceiveFixedConnection<T>(IConnection conn, object obj);

        Authenticator GetAuthenticator();
        string MembersString();
        string AddMembershipListener(IMembershipListener listener);
        bool RemoveMembershipListener(string registrationId);
    }
}