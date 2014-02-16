using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Multimap
{
    public class TxnMultiMapRemoveAllRequest : TxnMultiMapRequest
    {
        internal Data key;

        public TxnMultiMapRemoveAllRequest()
        {
        }

        public TxnMultiMapRemoveAllRequest(string name, Data key) : base(name)
        {
            this.key = key;
        }


        public override int GetClassId()
        {
            return MultiMapPortableHook.TxnMmRemoveAll;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            base.WritePortable(writer);
            IObjectDataOutput output = writer.GetRawDataOutput();
            key.WriteData(output);
        }


    }
}