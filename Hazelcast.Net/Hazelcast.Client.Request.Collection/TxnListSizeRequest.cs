using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Collection
{
    internal class TxnListSizeRequest : TxnCollectionRequest
    {
        public TxnListSizeRequest(string name) : base(name)
        {
        }

        public override int GetClassId()
        {
            return CollectionPortableHook.TxnListSize;
        }
    }
}