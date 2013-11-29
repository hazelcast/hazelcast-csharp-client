using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Collection
{
    public class CollectionClearRequest : CollectionRequest
    {
        public CollectionClearRequest()
        {
        }

        public CollectionClearRequest(string name) : base(name)
        {
        }

        public override int GetClassId()
        {
            return CollectionPortableHook.CollectionClear;
        }
    }
}