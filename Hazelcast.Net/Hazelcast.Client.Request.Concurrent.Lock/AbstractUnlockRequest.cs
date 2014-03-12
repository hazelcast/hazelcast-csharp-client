using Hazelcast.Client.Request.Base;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Request.Concurrent.Lock
{
    internal abstract class AbstractUnlockRequest : ClientRequest
    {
        private bool force;
        protected internal Data key;

        private long threadId;

        protected internal AbstractUnlockRequest(Data key, long threadId, bool force = false)
        {
            this.key = key;
            this.threadId = threadId;
            this.force = force;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            writer.WriteLong("tid", threadId);
            writer.WriteBoolean("force", force);
            IObjectDataOutput output = writer.GetRawDataOutput();
            key.WriteData(output);
        }


    }
}