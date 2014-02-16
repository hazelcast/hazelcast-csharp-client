using Hazelcast.Client.Request.Base;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;
using Hazelcast.Transaction;

namespace Hazelcast.Client.Request.Transaction
{
    public class CreateTransactionRequest : BaseTransactionRequest
    {
        internal TransactionOptions options;

        public CreateTransactionRequest()
        {
        }

        public CreateTransactionRequest(TransactionOptions options)
        {
            this.options = options;
        }

        public override int GetFactoryId()
        {
            return ClientTxnPortableHook.FId;
        }

        public override int GetClassId()
        {
            return ClientTxnPortableHook.Create;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            options.WriteData(writer.GetRawDataOutput());
        }

    }
}