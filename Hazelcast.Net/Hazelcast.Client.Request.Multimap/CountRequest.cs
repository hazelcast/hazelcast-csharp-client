using Hazelcast.Client.Request.Base;
using Hazelcast.Client.Request.Multimap;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Client.Request.Multimap
{
	
	public class CountRequest : MultiMapKeyBasedRequest, IRetryableRequest
	{
		public CountRequest()
		{
		}

		public CountRequest(string name, Data key) : base(name, key)
		{
		}

		public override int GetClassId()
		{
			return MultiMapPortableHook.Count;
		}
	}
}
