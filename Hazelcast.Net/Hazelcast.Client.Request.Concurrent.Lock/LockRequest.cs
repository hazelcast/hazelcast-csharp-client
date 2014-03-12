using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Concurrent.Lock
{
    internal sealed class LockRequest : AbstractLockRequest
    {

        public LockRequest(Data key, long threadId, long ttl, long timeout)
            : base(key, threadId, ttl, timeout)
        {
        }

        public override int GetFactoryId()
        {
            return LockPortableHook.FactoryId;
        }

        public override int GetClassId()
        {
            return LockPortableHook.Lock;
        }
    }
}