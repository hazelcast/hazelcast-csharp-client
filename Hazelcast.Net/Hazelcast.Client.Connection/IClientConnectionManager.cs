using System;
using System.Threading.Tasks;
using Hazelcast.Client.Request.Base;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Util;

namespace Hazelcast.Client.Connection
{
    public interface IClientConnectionManager:IRemotingService
    {
        void Start();
        bool Shutdown();

        bool Live { get; }

        void FireConnectionEvent(bool disconnected);

        ///// <exception cref="System.IO.IOException"></exception>
        //bool WriteToOwner(Data data);

        /// <exception cref="System.IO.IOException"></exception>
        Data ReadFromOwner();

        /// <exception cref="System.IO.IOException"></exception>
        object SendAndReceiveFromOwner(ClientRequest clientRequest);

        Address BindToRandomAddress();

        void HandleMembershipEvent(MembershipEvent membershipEvent);

        Address OwnerAddress();

        //void RemoveConnectionCalls(ClientConnection connection);

        ///// <exception cref="System.IO.IOException"></exception>
        //ClientConnection GetRandomConnection();

        ///// <exception cref="System.IO.IOException"></exception>
        //ClientConnection GetOrConnect(Address address);

        ///// <exception cref="System.IO.IOException"></exception>
        //ClientConnection OwnerConnection(Address address, Authenticator authenticator);

        ///// <exception cref="System.IO.IOException"></exception>
        //void DestroyConnection(ClientConnection clientConnection);
    }
}