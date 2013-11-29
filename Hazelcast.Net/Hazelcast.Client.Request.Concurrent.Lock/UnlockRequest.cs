using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Concurrent.Lock
{
    public sealed class UnlockRequest : AbstractUnlockRequest
    {
        public UnlockRequest()
        {
        }

        public UnlockRequest(Data key, int threadId) : base(key, threadId)
        {
        }

        public UnlockRequest(Data key, int threadId, bool force) : base(key, threadId, force)
        {
        }

        public override int GetFactoryId()
        {
            return LockPortableHook.FactoryId;
        }

        public override int GetClassId()
        {
            return LockPortableHook.Unlock;
        }
    }
}