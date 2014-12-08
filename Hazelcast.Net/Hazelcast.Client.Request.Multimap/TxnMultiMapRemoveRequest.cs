using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Multimap
{
    internal class TxnMultiMapRemoveRequest : TxnMultiMapRequest
    {
        internal IData key;
        internal IData value;

        public TxnMultiMapRemoveRequest(string name, IData key) : base(name)
        {
            this.key = key;
        }

        public TxnMultiMapRemoveRequest(string name, IData key, IData value) : this(name, key)
        {
            this.value = value;
        }

        public override int GetClassId()
        {
            return MultiMapPortableHook.TxnMmRemove;
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