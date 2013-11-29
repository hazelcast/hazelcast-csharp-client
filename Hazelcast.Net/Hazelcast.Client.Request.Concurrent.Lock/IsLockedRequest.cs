using Hazelcast.Client.Request.Base;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Concurrent.Lock
{
    public sealed class IsLockedRequest : AbstractIsLockedRequest, IRetryableRequest
    {
        public IsLockedRequest()
        {
        }

        public IsLockedRequest(Data key) : base(key)
        {
        }

        public IsLockedRequest(Data key, int threadId) : base(key, threadId)
        {
        }

        public override int GetFactoryId()
        {
            return LockPortableHook.FactoryId;
        }

        public override int GetClassId()
        {
            return LockPortableHook.IsLocked;
        }
    }
}