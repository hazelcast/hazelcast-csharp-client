using Hazelcast.IO.Serialization;
using Hazelcast.Security;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Base
{
    internal sealed class AuthenticationRequest : ClientRequest
	{
		private ICredentials credentials;

		private ClientPrincipal principal;

		private bool reAuth;

		private bool firstConnection = false;

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

		/// <exception cref="System.IO.IOException"></exception>
		public override void Write(IPortableWriter writer)
		{
			writer.WritePortable("credentials", (IPortable)credentials);
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
	}
}
