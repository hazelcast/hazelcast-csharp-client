using Hazelcast.Client.Request.Base;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Queue
{
    internal class IteratorRequest : QueueRequest, IRetryableRequest
    {
        public IteratorRequest()
        {
        }

        public IteratorRequest(string name) : base(name)
        {
        }

        public override int GetClassId()
        {
            return QueuePortableHook.Iterator;
        }
    }
}