using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Collection
{
    internal class TxnListRemoveRequest : TxnCollectionRequest
    {
        public TxnListRemoveRequest(string name, IData value) : base(name, value)
        {
        }

        public override int GetClassId()
        {
            return CollectionPortableHook.TxnListRemove;
        }
    }
}