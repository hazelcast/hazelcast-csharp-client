using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Multimap
{
    public class TxnMultiMapRemoveRequest : TxnMultiMapRequest
    {
        internal Data key;

        internal Data value;

        public TxnMultiMapRemoveRequest()
        {
        }

        public TxnMultiMapRemoveRequest(string name, Data key) : base(name)
        {
            this.key = key;
        }

        public TxnMultiMapRemoveRequest(string name, Data key, Data value) : this(name, key)
        {
            this.value = value;
        }

        public override int GetClassId()
        {
            return MultiMapPortableHook.TxnMmRemove;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            base.WritePortable(writer);
            IObjectDataOutput output = writer.GetRawDataOutput();
            key.WriteData(output);
            IOUtil.WriteNullableData(output, value);
        }


    }
}