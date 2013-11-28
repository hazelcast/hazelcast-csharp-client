using Hazelcast.Client.Request.Map;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Client.Request.Map
{
	public class MapPutIfAbsentRequest : MapPutRequest
	{
		public MapPutIfAbsentRequest()
		{
		}

		public MapPutIfAbsentRequest(string name, Data key, Data value, int threadId, long ttl) : base(name, key, value, threadId, ttl)
		{
		}

		public override int GetClassId()
		{
			return MapPortableHook.PutIfAbsent;
		}
	}
}
