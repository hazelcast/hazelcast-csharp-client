using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Concurrent.Semaphore
{
    internal class ReleaseRequest : SemaphoreRequest
    {

        public ReleaseRequest(string name, int permitCount) : base(name, permitCount)
        {
        }

        public override int GetClassId()
        {
            return SemaphorePortableHook.Release;
        }
    }
}