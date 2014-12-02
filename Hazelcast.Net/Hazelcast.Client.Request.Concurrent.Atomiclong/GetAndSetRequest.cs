using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Concurrent.Atomiclong
{
    internal class GetAndSetRequest : AtomicLongRequest
    {
        protected internal GetAndSetRequest(string name, long value) : base(name, value)
        {
        }

        public override int GetClassId()
        {
            return AtomicLongPortableHook.GetAndSet;
        }
    }
}