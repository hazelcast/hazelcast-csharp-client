using Hazelcast.Client.Request.Concurrent.Lock;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Client.Request.Concurrent.Lock
{
	
	public sealed class LockRequest : AbstractLockRequest
	{
		public LockRequest()
		{
		}

		public LockRequest(Data key, int threadId) : base(key, threadId)
		{
		}

		public LockRequest(Data key, int threadId, long ttl, long timeout) : base(key, threadId, ttl, timeout)
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
