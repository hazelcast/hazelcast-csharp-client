using Hazelcast.Client.Request.Base;
using Hazelcast.Client.Request.Transaction;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Queue
{
    public class TxnPeekRequest : BaseTransactionRequest
    {
        private string name;

        private long timeout;

        public TxnPeekRequest()
        {
        }

        public TxnPeekRequest(string name, long timeout)
        {
            this.name = name;
            this.timeout = timeout;
        }

        public override int GetFactoryId()
        {
            return QueuePortableHook.FId;
        }

        public override int GetClassId()
        {
            return QueuePortableHook.TxnPeek;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            writer.WriteLong("t", timeout);
        }

    }
}