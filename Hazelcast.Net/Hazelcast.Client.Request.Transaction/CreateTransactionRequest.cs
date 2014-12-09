using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;
using Hazelcast.Transaction;

namespace Hazelcast.Client.Request.Transaction
{
    internal class CreateTransactionRequest : BaseTransactionRequest
    {
        private TransactionOptions options;
        private IDataSerializable sXid = null;//SerializableXID

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
        public override void Write(IPortableWriter writer)
        {
            base.Write(writer);
            IObjectDataOutput output = writer.GetRawDataOutput();
            options.WriteData(output);
            output.WriteBoolean(sXid != null);
            if (sXid != null) {
                sXid.WriteData(output);
            }
        }
    }
}