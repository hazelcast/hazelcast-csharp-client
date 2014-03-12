using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Collection
{
    internal class TxnListAddRequest : TxnCollectionRequest
    {

        public TxnListAddRequest(string name, Data value) : base(name, value)
        {
        }

        public override int GetClassId()
        {
            return CollectionPortableHook.TxnListAdd;
        }
    }
}