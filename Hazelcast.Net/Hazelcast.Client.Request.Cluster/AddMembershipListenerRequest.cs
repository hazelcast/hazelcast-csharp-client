using Hazelcast.Client;
using Hazelcast.Client.Request.Base;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Cluster
{
    internal sealed class AddMembershipListenerRequest : ClientRequest, IRetryableRequest
	{

		public override int GetFactoryId()
		{
            return ClientPortableHook.Id;
		}

		public override int GetClassId()
		{
            return ClientPortableHook.MembershipListener;
		}

		/// <exception cref="System.IO.IOException"></exception>
		public override void Write(IPortableWriter writer)
		{
		}
	}
}
