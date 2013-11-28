using Hazelcast.Client.Request.Concurrent.Semaphore;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Client.Request.Concurrent.Semaphore
{
	
	public class ReleaseRequest : SemaphoreRequest
	{
		public ReleaseRequest()
		{
		}

		public ReleaseRequest(string name, int permitCount) : base(name, permitCount)
		{
		}

		public override int GetClassId()
		{
			return SemaphorePortableHook.Release;
		}
	}
}
