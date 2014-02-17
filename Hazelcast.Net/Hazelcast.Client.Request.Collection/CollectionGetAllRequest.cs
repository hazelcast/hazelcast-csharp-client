using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Collection
{
    internal class CollectionGetAllRequest : CollectionRequest
    {
        public CollectionGetAllRequest()
        {
        }

        public CollectionGetAllRequest(string name) : base(name)
        {
        }

        public override int GetClassId()
        {
            return CollectionPortableHook.CollectionGetAll;
        }
    }
}