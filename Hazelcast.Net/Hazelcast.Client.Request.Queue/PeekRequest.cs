using Hazelcast.Client.Request.Base;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Queue
{
    internal class PeekRequest : QueueRequest, IRetryableRequest
    {
        protected internal PeekRequest(string name) : base(name)
        {
        }

        public override int GetClassId()
        {
            return QueuePortableHook.Peek;
        }
    }
}