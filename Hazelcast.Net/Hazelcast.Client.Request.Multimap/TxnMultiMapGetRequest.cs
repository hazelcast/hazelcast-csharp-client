using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Multimap
{
    public class TxnMultiMapGetRequest : TxnMultiMapRequest
    {
        internal Data key;

        public TxnMultiMapGetRequest()
        {
        }

        public TxnMultiMapGetRequest(string name, Data key) : base(name)
        {
            this.key = key;
        }

        public override int GetClassId()
        {
            return MultiMapPortableHook.TxnMmGet;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            base.WritePortable(writer);
            key.WriteData(writer.GetRawDataOutput());
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void ReadPortable(IPortableReader reader)
        {
            base.ReadPortable(reader);
            key = new Data();
            key.ReadData(reader.GetRawDataInput());
        }
    }
}