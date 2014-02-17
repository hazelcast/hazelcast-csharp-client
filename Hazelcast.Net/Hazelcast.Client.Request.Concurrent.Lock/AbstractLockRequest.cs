using Hazelcast.Client.Request.Base;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Request.Concurrent.Lock
{
    internal abstract class AbstractLockRequest : ClientRequest
    {
        protected internal Data key;

        private long threadId;

        private long timeout = -1;
        private long ttl = -1;

        protected AbstractLockRequest()
        {
        }

        protected AbstractLockRequest(Data key, long threadId, long ttl=-1, long timeout=-1)
        {
            this.key = key;
            this.threadId = threadId;
            this.ttl = ttl;
            this.timeout = timeout;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            writer.WriteLong("tid", threadId);
            writer.WriteLong("ttl", ttl);
            writer.WriteLong("timeout", timeout);
            IObjectDataOutput output = writer.GetRawDataOutput();
            key.WriteData(output);
        }


    }
}