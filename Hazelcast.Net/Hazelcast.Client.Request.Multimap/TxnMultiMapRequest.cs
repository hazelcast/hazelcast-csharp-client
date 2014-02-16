using Hazelcast.Client.Request.Base;
using Hazelcast.Client.Request.Transaction;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Multimap
{
    public abstract class TxnMultiMapRequest : BaseTransactionRequest
    {
        internal string name;

        protected internal TxnMultiMapRequest()
        {
        }

        protected internal TxnMultiMapRequest(string name)
        {
            this.name = name;
        }

        public override int GetFactoryId()
        {
            return MultiMapPortableHook.FId;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
        }


    }
}