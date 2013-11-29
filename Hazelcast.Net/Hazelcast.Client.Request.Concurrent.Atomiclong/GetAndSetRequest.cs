using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Concurrent.Atomiclong
{
    public class GetAndSetRequest : AtomicLongRequest
    {
        public GetAndSetRequest()
        {
        }

        public GetAndSetRequest(string name, long value) : base(name, value)
        {
        }

        public override int GetClassId()
        {
            return AtomicLongPortableHook.GetAndSet;
        }
    }
}