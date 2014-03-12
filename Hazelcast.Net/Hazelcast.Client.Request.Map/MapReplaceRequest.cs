using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Map
{
    internal class MapReplaceRequest : MapPutRequest
    {

        public MapReplaceRequest(string name, Data key, Data value, long threadId) : base(name, key, value, threadId)
        {
        }

        public override int GetClassId()
        {
            return MapPortableHook.Replace;
        }
    }
}