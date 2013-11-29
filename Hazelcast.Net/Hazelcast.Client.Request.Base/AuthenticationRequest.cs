using Hazelcast.IO.Serialization;
using Hazelcast.Security;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Base
{
    public sealed class AuthenticationRequest : IPortable
    {
        private ICredentials credentials;
        private bool firstConnection;

        private ClientPrincipal principal;

        private bool reAuth;

        public AuthenticationRequest()
        {
        }

        public AuthenticationRequest(ICredentials credentials)
        {
            this.credentials = credentials;
        }

        public AuthenticationRequest(ICredentials credentials, ClientPrincipal principal)
        {
            this.credentials = credentials;
            this.principal = principal;
        }

        public int GetFactoryId()
        {
            return ClientPortableHook.Id;
        }

        public int GetClassId()
        {
            return ClientPortableHook.Auth;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WritePortable(IPortableWriter writer)
        {
            writer.WritePortable("credentials", credentials);
            if (principal != null)
            {
                writer.WritePortable("principal", principal);
            }
            else
            {
                writer.WriteNullPortable("principal", ClientPortableHook.Id, ClientPortableHook.Principal);
            }
            writer.WriteBoolean("reAuth", reAuth);
            writer.WriteBoolean("firstConnection", firstConnection);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void ReadPortable(IPortableReader reader)
        {
            credentials = reader.ReadPortable<ICredentials>("credentials");
            principal = reader.ReadPortable<ClientPrincipal>("principal");
            reAuth = reader.ReadBoolean("reAuth");
            firstConnection = reader.ReadBoolean("firstConnection");
        }

        public void SetReAuth(bool reAuth)
        {
            this.reAuth = reAuth;
        }

        public bool IsFirstConnection()
        {
            return firstConnection;
        }

        public void SetFirstConnection(bool firstConnection)
        {
            this.firstConnection = firstConnection;
        }
    }
}