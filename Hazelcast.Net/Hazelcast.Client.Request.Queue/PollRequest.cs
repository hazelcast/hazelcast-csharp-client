using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Queue
{
    internal class PollRequest : QueueRequest
    {
        protected internal PollRequest(string name) : base(name)
        {
        }

        protected internal PollRequest(string name, long timeoutMillis) : base(name, timeoutMillis)
        {
        }

        public override int GetClassId()
        {
            return QueuePortableHook.Poll;
        }
    }
}