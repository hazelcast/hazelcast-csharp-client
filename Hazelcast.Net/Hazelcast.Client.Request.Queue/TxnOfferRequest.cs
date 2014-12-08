using Hazelcast.Client.Request.Base;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Queue
{
    internal class TxnOfferRequest : BaseTransactionRequest
    {
        internal IData data;
        internal string name;
        internal long timeout;

        public TxnOfferRequest(string name, long timeout, IData data)
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
        public override void Write(IPortableWriter writer)
        {
            base.Write(writer);
            writer.WriteUTF("n", name);
            writer.WriteLong("t", timeout);
            writer.GetRawDataOutput().WriteData(data);
        }
    }
}