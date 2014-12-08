using Hazelcast.Client.Request.Base;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Queue
{
    internal class RemainingCapacityRequest : QueueRequest, IRetryableRequest
    {
        protected internal RemainingCapacityRequest(string name) : base(name)
        {
        }

        public override int GetFactoryId()
        {
            return QueuePortableHook.FId;
        }

        public override int GetClassId()
        {
            return QueuePortableHook.RemainingCapacity;
        }
    }
}