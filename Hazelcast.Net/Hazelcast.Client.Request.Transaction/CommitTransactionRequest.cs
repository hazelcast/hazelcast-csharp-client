using Hazelcast.Client.Request.Base;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Transaction
{
    internal class CommitTransactionRequest : BaseTransactionRequest
    {
        public override int GetFactoryId()
        {
            return ClientTxnPortableHook.FId;
        }

        public override int GetClassId()
        {
            return ClientTxnPortableHook.Commit;
        }

        public override void WritePortable(IPortableWriter writer){}

    }
}