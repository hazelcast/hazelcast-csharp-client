using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Multimap
{
    public class TxnMultiMapPutRequest : TxnMultiMapRequest
    {
        internal Data key;

        internal Data value;

        public TxnMultiMapPutRequest()
        {
        }

        public TxnMultiMapPutRequest(string name, Data key, Data value) : base(name)
        {
            this.key = key;
            this.value = value;
        }

        public override int GetClassId()
        {
            return MultiMapPortableHook.TxnMmPut;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            base.WritePortable(writer);
            IObjectDataOutput output = writer.GetRawDataOutput();
            key.WriteData(output);
            value.WriteData(output);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void ReadPortable(IPortableReader reader)
        {
            base.ReadPortable(reader);
            IObjectDataInput input = reader.GetRawDataInput();
            key = new Data();
            key.ReadData(input);
            value = new Data();
            value.ReadData(input);
        }
    }
}