using Hazelcast.Client.Request.Base;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Concurrent.Lock
{
    internal sealed class IsLockedRequest : AbstractIsLockedRequest, IRetryableRequest
    {
        public IsLockedRequest(IData key) : base(key)
        {
        }

        public IsLockedRequest(IData key, long threadId) : base(key, threadId)
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