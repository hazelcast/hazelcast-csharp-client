using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Concurrent.Lock
{
    public sealed class UnlockRequest : AbstractUnlockRequest
    {
        public UnlockRequest()
        {
        }

        public UnlockRequest(Data key, long threadId)
            : base(key, threadId, default(bool))
        {
        }

        public UnlockRequest(Data key, long threadId, bool force)
            : base(key, threadId, force)
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