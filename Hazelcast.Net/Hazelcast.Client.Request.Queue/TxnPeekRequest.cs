using Hazelcast.Client.Request.Base;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Queue
{
    internal class TxnPeekRequest : BaseTransactionRequest
    {
        private readonly string name;

        private readonly long timeout;

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
        public override void Write(IPortableWriter writer)
        {
            base.Write(writer);
            writer.WriteUTF("n", name);
            writer.WriteLong("t", timeout);
        }
    }
}