using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Transaction
{
    internal class CommitTransactionRequest : BaseTransactionRequest
    {
        private readonly bool prepareAndCommit;

        public CommitTransactionRequest()
        {
        }

        public CommitTransactionRequest(bool prepareAndCommit)
        {
            this.prepareAndCommit = prepareAndCommit;
        }

        public override int GetFactoryId()
        {
            return ClientTxnPortableHook.FId;
        }

        public override int GetClassId()
        {
            return ClientTxnPortableHook.Commit;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            base.Write(writer);
            writer.WriteBoolean("pc", prepareAndCommit);
        }
    }
}