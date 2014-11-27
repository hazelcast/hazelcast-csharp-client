using Hazelcast.Client.Request.Base;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Collection
{
    internal abstract class TxnCollectionRequest : BaseTransactionRequest
    {
        internal string name;
        internal IData value;

        protected TxnCollectionRequest(string name)
        {
            this.name = name;
        }

        protected TxnCollectionRequest(string name, IData value) : this(name)
        {
            this.value = value;
        }

        public override int GetFactoryId()
        {
            return CollectionPortableHook.FId;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            base.Write(writer);
            writer.WriteUTF("n", name);
            writer.GetRawDataOutput().WriteData(value);
        }
    }
}