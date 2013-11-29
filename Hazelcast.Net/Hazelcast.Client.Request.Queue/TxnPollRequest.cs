using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Queue
{
    public class TxnPollRequest : IPortable
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

        public virtual int GetFactoryId()
        {
            return QueuePortableHook.FId;
        }

        public virtual int GetClassId()
        {
            return QueuePortableHook.TxnPoll;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            writer.WriteLong("t", timeout);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void ReadPortable(IPortableReader reader)
        {
            name = reader.ReadUTF("n");
            timeout = reader.ReadLong("t");
        }
    }
}