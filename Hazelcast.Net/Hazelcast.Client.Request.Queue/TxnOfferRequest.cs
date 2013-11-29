using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Queue
{
    public class TxnOfferRequest : IPortable
    {
        internal Data data;
        internal string name;

        internal long timeout;

        public TxnOfferRequest()
        {
        }

        public TxnOfferRequest(string name, long timeout, Data data)
        {
            this.name = name;
            this.timeout = timeout;
            this.data = data;
        }

        public virtual int GetFactoryId()
        {
            return QueuePortableHook.FId;
        }

        public virtual int GetClassId()
        {
            return QueuePortableHook.TxnOffer;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            writer.WriteLong("t", timeout);
            data.WriteData(writer.GetRawDataOutput());
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void ReadPortable(IPortableReader reader)
        {
            name = reader.ReadUTF("n");
            timeout = reader.ReadLong("t");
            data = new Data();
            data.ReadData(reader.GetRawDataInput());
        }
    }
}