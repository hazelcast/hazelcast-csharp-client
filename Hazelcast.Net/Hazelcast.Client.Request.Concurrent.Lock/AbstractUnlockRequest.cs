using Hazelcast.Client.Request.Base;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Request.Concurrent.Lock
{
    internal abstract class AbstractUnlockRequest : ClientRequest
    {
        private readonly bool force;
        private readonly long threadId;
        protected internal IData key;

        protected internal AbstractUnlockRequest(IData key, int threadId, bool force = false)
        {
            this.key = key;
            this.threadId = threadId;
            this.force = force;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            writer.WriteLong("tid", threadId);
            writer.WriteBoolean("force", force);
            IObjectDataOutput output = writer.GetRawDataOutput();
            output.WriteData(key);
        }
    }
}