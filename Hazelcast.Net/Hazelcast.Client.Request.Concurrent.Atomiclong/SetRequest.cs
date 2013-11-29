using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Concurrent.Atomiclong
{
    public class SetRequest : AtomicLongRequest
    {
        public SetRequest()
        {
        }

        public SetRequest(string name, long value) : base(name, value)
        {
        }

        public override int GetClassId()
        {
            return AtomicLongPortableHook.Set;
        }
    }
}