using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Concurrent.Semaphore
{
    public class AcquireRequest : SemaphoreRequest
    {
        internal long timeout;

        public AcquireRequest()
        {
        }

        public AcquireRequest(string name, int permitCount, long timeout) : base(name, permitCount)
        {
            this.timeout = timeout;
        }

        public override int GetClassId()
        {
            return SemaphorePortableHook.Acquire;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            base.WritePortable(writer);
            writer.WriteLong("t", timeout);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void ReadPortable(IPortableReader reader)
        {
            base.ReadPortable(reader);
            timeout = reader.ReadLong("t");
        }
    }
}