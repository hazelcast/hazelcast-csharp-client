using Hazelcast.Client.Request.Base;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Queue
{
    internal class SizeRequest : QueueRequest, IRetryableRequest
    {
        protected internal SizeRequest(string name) : base(name)
        {
        }

        public override int GetClassId()
        {
            return QueuePortableHook.Size;
        }
    }
}