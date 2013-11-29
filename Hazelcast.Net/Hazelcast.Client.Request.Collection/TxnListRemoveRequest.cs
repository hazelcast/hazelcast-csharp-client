using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Collection
{
    public class TxnListRemoveRequest : TxnCollectionRequest
    {
        public TxnListRemoveRequest()
        {
        }

        public TxnListRemoveRequest(string name, Data value) : base(name, value)
        {
        }

        public override int GetClassId()
        {
            return CollectionPortableHook.TxnListRemove;
        }
    }
}