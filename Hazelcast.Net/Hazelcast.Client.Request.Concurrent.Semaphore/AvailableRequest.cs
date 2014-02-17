using Hazelcast.Client.Request.Base;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Concurrent.Semaphore
{
    internal class AvailableRequest : SemaphoreRequest, IRetryableRequest
    {
        public AvailableRequest()
        {
        }

        public AvailableRequest(string name) : base(name, -1)
        {
        }

        public override int GetClassId()
        {
            return SemaphorePortableHook.Available;
        }
    }
}