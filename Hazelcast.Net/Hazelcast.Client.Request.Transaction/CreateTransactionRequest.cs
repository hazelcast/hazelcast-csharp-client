using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;
using Hazelcast.Transaction;

namespace Hazelcast.Client.Request.Transaction
{
    public class CreateTransactionRequest : IPortable
    {
        internal TransactionOptions options;

        public CreateTransactionRequest()
        {
        }

        public CreateTransactionRequest(TransactionOptions options)
        {
            this.options = options;
        }

        public virtual int GetFactoryId()
        {
            return ClientTxnPortableHook.FId;
        }

        public virtual int GetClassId()
        {
            return ClientTxnPortableHook.Create;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WritePortable(IPortableWriter writer)
        {
            options.WriteData(writer.GetRawDataOutput());
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void ReadPortable(IPortableReader reader)
        {
            options = new TransactionOptions();
            options.ReadData(reader.GetRawDataInput());
        }
    }
}