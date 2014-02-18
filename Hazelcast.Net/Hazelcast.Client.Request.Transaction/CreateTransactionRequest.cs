using Hazelcast.Client.Request.Base;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;
using Hazelcast.Transaction;

namespace Hazelcast.Client.Request.Transaction
{
    internal class CreateTransactionRequest : BaseTransactionRequest
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
            var objectDataOutput = writer.GetRawDataOutput();
            options.WriteData(objectDataOutput);
            //TODO if xid is implemented below permanent false should be replaced
            objectDataOutput.WriteBoolean(false);//(sXid != null);
        }

    }
}