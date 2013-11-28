using Hazelcast.Client.Request.Base;
using Hazelcast.Client.Request.Multimap;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Client.Request.Multimap
{
	
	public class KeySetRequest : MultiMapAllPartitionRequest, IRetryableRequest
	{
		public KeySetRequest()
		{
		}

		public KeySetRequest(string name) : base(name)
		{
		}

		public override int GetClassId()
		{
			return MultiMapPortableHook.KeySet;
		}
	}
}
