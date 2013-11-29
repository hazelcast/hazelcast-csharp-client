using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Map
{
    public class MapReplaceRequest : MapPutRequest
    {
        public MapReplaceRequest()
        {
        }

        public MapReplaceRequest(string name, Data key, Data value, int threadId) : base(name, key, value, threadId)
        {
        }

        public MapReplaceRequest(string name, Data key, Data value, int threadId, long ttl)
            : base(name, key, value, threadId, ttl)
        {
        }

        public override int GetClassId()
        {
            return MapPortableHook.Replace;
        }
    }
}