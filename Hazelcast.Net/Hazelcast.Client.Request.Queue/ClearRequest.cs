using Hazelcast.Client.Request.Base;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Queue
{
    internal class ClearRequest : QueueRequest, IRetryableRequest
    {
        public ClearRequest()
        {
        }

        public ClearRequest(string name) : base(name)
        {
        }

        public override int GetClassId()
        {
            return QueuePortableHook.Clear;
        }
    }
}