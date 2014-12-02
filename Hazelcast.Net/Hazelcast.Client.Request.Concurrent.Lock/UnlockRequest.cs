using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Concurrent.Lock
{
    internal sealed class UnlockRequest : AbstractUnlockRequest
    {
        public UnlockRequest(IData key, int threadId) : base(key, threadId)
        {
        }

        public UnlockRequest(IData key, int threadId, bool force) : base(key, threadId, force)
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