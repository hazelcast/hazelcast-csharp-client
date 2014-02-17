using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Concurrent.Semaphore
{
    internal class ReduceRequest : SemaphoreRequest
    {
        public ReduceRequest()
        {
        }

        public ReduceRequest(string name, int permitCount) : base(name, permitCount)
        {
        }

        public override int GetClassId()
        {
            return SemaphorePortableHook.Reduce;
        }
    }
}