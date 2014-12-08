using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Multimap
{
    internal class TxnMultiMapPutRequest : TxnMultiMapRequest
    {
        internal IData key;
        internal IData value;

        public TxnMultiMapPutRequest(string name, IData key, IData value) : base(name)
        {
            this.key = key;
            this.value = value;
        }

        public override int GetClassId()
        {
            return MultiMapPortableHook.TxnMmPut;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            base.Write(writer);
            IObjectDataOutput output = writer.GetRawDataOutput();
            output.WriteData(key);
            output.WriteData(value);
        }
    }
}