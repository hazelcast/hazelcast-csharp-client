using Hazelcast.Client.Request.Base;
using Hazelcast.Client.Request.Concurrent.Semaphore;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Client.Request.Concurrent.Semaphore
{
	
	public class AvailableRequest : SemaphoreRequest, IRetryableRequest
	{
		public AvailableRequest()
		{
		}

		public AvailableRequest(string name) : base(name, -1)
		{
		}

		public override int GetClassId()
		{
			return SemaphorePortableHook.Available;
		}
	}
}
