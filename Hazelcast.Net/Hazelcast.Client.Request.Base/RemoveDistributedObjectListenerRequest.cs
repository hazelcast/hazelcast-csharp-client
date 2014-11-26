using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Base
{
	/// <summary>Client request to add a distributed object listener to a remote node.</summary>
	/// <remarks>Client request to add a distributed object listener to a remote node.</remarks>
	internal class RemoveDistributedObjectListenerRequest : BaseClientRemoveListenerRequest
	{
		public RemoveDistributedObjectListenerRequest(string registrationId) : base(null, 
			registrationId)
		{
		}

		public override int GetFactoryId()
		{
            return ClientPortableHook.Id;
		}

		public override int GetClassId()
		{
            return ClientPortableHook.RemoveListener;
		}
	}
}
