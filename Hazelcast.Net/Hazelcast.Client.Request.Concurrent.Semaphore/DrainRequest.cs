using Hazelcast.Client.Request.Concurrent.Semaphore;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Client.Request.Concurrent.Semaphore
{
	
	public class DrainRequest : SemaphoreRequest
	{
		public DrainRequest()
		{
		}

		public DrainRequest(string name) : base(name, -1)
		{
		}

		public override int GetClassId()
		{
			return SemaphorePortableHook.Drain;
		}
	}
}
