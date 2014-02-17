using Hazelcast.Client.Request.Base;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Queue
{
    internal abstract class QueueRequest : ClientRequest
    {
        protected internal string name;

        protected internal long timeoutMillis;

        protected internal QueueRequest()
        {
        }

        protected internal QueueRequest(string name)
        {
            this.name = name;
        }

        protected internal QueueRequest(string name, long timeoutMillis)
        {
            this.name = name;
            this.timeoutMillis = timeoutMillis;
        }

        public override int GetFactoryId()
        {
            return QueuePortableHook.FId;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            writer.WriteLong("t", timeoutMillis);
        }



    }
}