using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Multimap
{
    internal class TxnMultiMapGetRequest : TxnMultiMapRequest
    {
        internal IData key;


        public TxnMultiMapGetRequest(string name, IData key) : base(name)
        {
            this.key = key;
        }

        public override int GetClassId()
        {
            return MultiMapPortableHook.TxnMmGet;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            base.Write(writer);
            writer.GetRawDataOutput().WriteData(key);
        }
    }
}