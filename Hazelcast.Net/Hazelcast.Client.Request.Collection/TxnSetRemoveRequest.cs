using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Collection
{
    internal class TxnSetRemoveRequest : TxnCollectionRequest
    {

        public TxnSetRemoveRequest(string name, Data value) : base(name, value)
        {
        }

        public override int GetClassId()
        {
            return CollectionPortableHook.TxnSetRemove;
        }
    }
}