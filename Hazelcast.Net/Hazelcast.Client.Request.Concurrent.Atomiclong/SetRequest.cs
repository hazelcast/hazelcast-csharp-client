using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Concurrent.Atomiclong
{
    internal class SetRequest : AtomicLongRequest
    {
        protected internal SetRequest(string name, long value) : base(name, value)
        {
        }

        public override int GetClassId()
        {
            return AtomicLongPortableHook.Set;
        }
    }
}