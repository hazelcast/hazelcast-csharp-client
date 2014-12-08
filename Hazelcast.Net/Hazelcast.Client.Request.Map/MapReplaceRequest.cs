using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Map
{
    internal class MapReplaceRequest : MapPutRequest
	{
		public MapReplaceRequest(string name, IData key, IData value, long threadId) : base(name, key, value, threadId)
		{
		}

		public MapReplaceRequest(string name, IData key, IData value, long threadId, long ttl) : base(name, key, value, threadId, ttl)
		{
		}

        public override int GetClassId()
        {
            return MapPortableHook.Replace;
        }
    }
}
