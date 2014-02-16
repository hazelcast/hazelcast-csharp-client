using Hazelcast.IO.Serialization;
using Hazelcast.Security;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Base
{
    public sealed class AuthenticationRequest : ClientRequest
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

        public override int GetFactoryId()
        {
            return ClientPortableHook.Id;
        }

        public override int GetClassId()
        {
            return ClientPortableHook.Auth;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
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

        public void SetReAuth(bool reAuth)
        {
            this.reAuth = reAuth;
        }

        public void SetFirstConnection(bool firstConnection)
        {
            this.firstConnection = firstConnection;
        }
    }
}