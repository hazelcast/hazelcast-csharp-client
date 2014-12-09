using Hazelcast.Client.Request.Base;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Queue
{
    /// <summary>Request to check if the Queue is empty</summary>
    internal class IsEmptyRequest : QueueRequest, IRetryableRequest
    {
        protected internal IsEmptyRequest(string name) : base(name)
        {
        }

        public override int GetClassId()
        {
            return QueuePortableHook.IsEmpty;
        }
    }
}