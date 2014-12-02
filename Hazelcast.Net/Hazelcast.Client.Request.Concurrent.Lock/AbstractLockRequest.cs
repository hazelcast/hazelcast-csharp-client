using Hazelcast.Client.Request.Base;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Request.Concurrent.Lock
{
    internal abstract class AbstractLockRequest : ClientRequest
    {
        private readonly long threadId;
        private readonly long timeout = -1;
        private readonly long ttl = -1;
        protected internal IData key;

        protected AbstractLockRequest(IData key, int threadId, long ttl = -1, long timeout = -1)
        {
            this.key = key;
            this.threadId = threadId;
            this.ttl = ttl;
            this.timeout = timeout;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            writer.WriteLong("tid", threadId);
            writer.WriteLong("ttl", ttl);
            writer.WriteLong("timeout", timeout);
            IObjectDataOutput output = writer.GetRawDataOutput();
            output.WriteData(key);
        }
    }
}