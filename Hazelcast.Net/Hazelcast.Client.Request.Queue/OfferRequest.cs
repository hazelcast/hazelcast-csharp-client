using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Queue
{
    internal class OfferRequest : QueueRequest
    {
        internal Data data;


        public OfferRequest(string name, long timeoutMillis, Data data) : base(name, timeoutMillis)
        {
            this.data = data;
        }

        public override int GetClassId()
        {
            return QueuePortableHook.Offer;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            base.WritePortable(writer);
            IObjectDataOutput output = writer.GetRawDataOutput();
            data.WriteData(output);
        }

    }
}