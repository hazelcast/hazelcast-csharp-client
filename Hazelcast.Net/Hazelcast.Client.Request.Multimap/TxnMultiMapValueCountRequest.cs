using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Multimap
{
    internal class TxnMultiMapValueCountRequest : TxnMultiMapRequest
    {
        internal Data key;


        public TxnMultiMapValueCountRequest(string name, Data key) : base(name)
        {
            this.key = key;
        }

        public override int GetClassId()
        {
            return MultiMapPortableHook.TxnMmValueCount;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            base.WritePortable(writer);
            key.WriteData(writer.GetRawDataOutput());
        }

    }
}