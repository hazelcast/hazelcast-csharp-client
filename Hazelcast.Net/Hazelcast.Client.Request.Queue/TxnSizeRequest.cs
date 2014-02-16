using Hazelcast.Client.Request.Base;
using Hazelcast.Client.Request.Transaction;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Queue
{
    public class TxnSizeRequest : BaseTransactionRequest
    {
        internal string name;

        public TxnSizeRequest()
        {
        }

        public TxnSizeRequest(string name)
        {
            this.name = name;
        }

        public override int GetFactoryId()
        {
            return QueuePortableHook.FId;
        }

        public override int GetClassId()
        {
            return QueuePortableHook.TxnSize;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
        }

    }
}