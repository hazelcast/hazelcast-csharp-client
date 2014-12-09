using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Transaction
{
    internal class RollbackTransactionRequest : BaseTransactionRequest
    {
        public override int GetFactoryId()
        {
            return ClientTxnPortableHook.FId;
        }

        public override int GetClassId()
        {
            return ClientTxnPortableHook.Rollback;
        }
    }
}