using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Concurrent.Atomiclong
{
    internal class GetAndAddRequest : AtomicLongRequest
    {
        protected internal GetAndAddRequest(string name, long delta) : base(name, delta)
        {
        }

        public override int GetClassId()
        {
            return AtomicLongPortableHook.GetAndAdd;
        }
    }
}