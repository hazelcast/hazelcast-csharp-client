using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Multimap
{
    internal class TxnMultiMapPutRequest : TxnMultiMapRequest
    {
        internal Data key;

        internal Data value;


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

    }
}