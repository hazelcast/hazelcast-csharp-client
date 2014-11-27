using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Collection
{
    internal class TxnSetSizeRequest : TxnCollectionRequest
    {
        public TxnSetSizeRequest(string name) : base(name)
        {
        }

        public override int GetClassId()
        {
            return CollectionPortableHook.TxnSetSize;
        }
    }
}