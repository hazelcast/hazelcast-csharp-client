using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Multimap
{
    internal class TxnMultiMapGetRequest : TxnMultiMapRequest
    {
        internal Data key;


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


    }
}