using Hazelcast.Client.Request.Base;
using Hazelcast.Client.Request.Multimap;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Client.Request.Multimap
{
	
	public class ClearRequest : MultiMapAllPartitionRequest, IRetryableRequest
	{
		public ClearRequest()
		{
		}

		public ClearRequest(string name) : base(name)
		{
		}

		public override int GetClassId()
		{
			return MultiMapPortableHook.Clear;
		}
	}
}
