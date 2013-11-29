using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Map
{
    public class MapSetRequest : MapPutRequest
    {
        public MapSetRequest()
        {
        }

        public MapSetRequest(string name, Data key, Data value, int threadId) : base(name, key, value, threadId)
        {
        }

        public MapSetRequest(string name, Data key, Data value, int threadId, long ttl)
            : base(name, key, value, threadId, ttl)
        {
        }

        public override int GetClassId()
        {
            return MapPortableHook.Set;
        }
    }
}