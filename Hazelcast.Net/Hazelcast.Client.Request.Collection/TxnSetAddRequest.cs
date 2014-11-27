using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Collection
{
    internal class TxnSetAddRequest : TxnCollectionRequest
    {
        public TxnSetAddRequest(string name, IData value) : base(name, value)
        {
        }

        public override int GetClassId()
        {
            return CollectionPortableHook.TxnSetAdd;
        }
    }
}