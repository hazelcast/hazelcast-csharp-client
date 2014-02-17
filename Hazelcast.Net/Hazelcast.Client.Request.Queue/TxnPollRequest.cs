using Hazelcast.Client.Request.Base;
using Hazelcast.Client.Request.Transaction;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Queue
{
    internal class TxnPollRequest : BaseTransactionRequest
    {
        internal string name;

        internal long timeout;

        public TxnPollRequest()
        {
        }

        public TxnPollRequest(string name, long timeout)
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
            return QueuePortableHook.TxnPoll;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            writer.WriteLong("t", timeout);
        }

    }
}