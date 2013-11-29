using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Queue
{
    /// <summary>
    ///     User: ahmetmircik
    ///     Date: 10/1/13
    ///     Time: 10:34 AM
    /// </summary>
    public class TxnPeekRequest : IPortable
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

        public virtual int GetFactoryId()
        {
            return QueuePortableHook.FId;
        }

        public virtual int GetClassId()
        {
            return QueuePortableHook.TxnPeek;
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