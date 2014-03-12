using Hazelcast.Client.Request.Base;
using Hazelcast.Client.Request.Transaction;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Queue
{
    internal class TxnOfferRequest : BaseTransactionRequest
    {
        internal Data data;
        internal string name;

        internal long timeout;

        public TxnOfferRequest(string name, long timeout, Data data)
        {
            this.name = name;
            this.timeout = timeout;
            this.data = data;
        }

        public override int GetFactoryId()
        {
            return QueuePortableHook.FId;
        }

        public override int GetClassId()
        {
            return QueuePortableHook.TxnOffer;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            writer.WriteLong("t", timeout);
            data.WriteData(writer.GetRawDataOutput());
        }


    }
}