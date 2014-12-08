using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Queue
{
    internal class OfferRequest : QueueRequest
    {
        internal IData data;

        public OfferRequest(string name, IData data) : base(name)
        {
            this.data = data;
        }

        public OfferRequest(string name, long timeoutMillis, IData data) : base(name, timeoutMillis)
        {
            this.data = data;
        }

        public override int GetClassId()
        {
            return QueuePortableHook.Offer;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            base.Write(writer);
            IObjectDataOutput output = writer.GetRawDataOutput();
            output.WriteData(data);
        }
    }
}