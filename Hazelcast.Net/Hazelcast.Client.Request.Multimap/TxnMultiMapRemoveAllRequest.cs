using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Multimap
{
    public class TxnMultiMapRemoveAllRequest : TxnMultiMapRequest
    {
        internal IData key;

        public TxnMultiMapRemoveAllRequest(string name, IData key) : base(name)
        {
            this.key = key;
        }

        public override int GetClassId()
        {
            return MultiMapPortableHook.TxnMmRemoveAll;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            base.Write(writer);
            IObjectDataOutput output = writer.GetRawDataOutput();
            output.WriteData(key);
        }
    }
}