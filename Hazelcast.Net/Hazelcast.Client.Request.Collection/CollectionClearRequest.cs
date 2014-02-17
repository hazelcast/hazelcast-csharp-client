using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Collection
{
    internal class CollectionClearRequest : CollectionRequest
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