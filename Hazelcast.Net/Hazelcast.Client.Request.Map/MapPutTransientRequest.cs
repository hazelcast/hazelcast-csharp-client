using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Map
{
    internal class MapPutTransientRequest : MapPutRequest
    {
        public MapPutTransientRequest(string name, IData key, IData value, long threadId, long ttl)
            : base(name, key, value, threadId, ttl)
        {
        }

        public override int GetClassId()
        {
            return MapPortableHook.PutTransient;
        }
    }
}