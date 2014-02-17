using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Map
{
    internal class MapPutTransientRequest : MapPutRequest
    {
        public MapPutTransientRequest()
        {
        }

        public MapPutTransientRequest(string name, Data key, Data value, long threadId, long ttl)
            : base(name, key, value, threadId, ttl)
        {
        }

        public override int GetClassId()
        {
            return MapPortableHook.PutTransient;
        }
    }
}