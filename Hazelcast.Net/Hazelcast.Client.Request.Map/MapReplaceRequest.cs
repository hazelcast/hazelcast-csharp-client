using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Map
{
    internal class MapReplaceRequest : MapPutRequest
    {
        public MapReplaceRequest()
        {
        }

        public MapReplaceRequest(string name, Data key, Data value, long threadId) : base(name, key, value, threadId)
        {
        }

        public MapReplaceRequest(string name, Data key, Data value, long threadId, long ttl)
            : base(name, key, value, threadId, ttl)
        {
        }

        public override int GetClassId()
        {
            return MapPortableHook.Replace;
        }
    }
}