using Hazelcast.Client.Request.Base;
using Hazelcast.Client.Request.Queue;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Client.Request.Queue
{
	
	public class PeekRequest : QueueRequest, IRetryableRequest
	{
		public PeekRequest()
		{
		}

		public PeekRequest(string name) : base(name)
		{
		}

		public override int GetClassId()
		{
			return QueuePortableHook.Peek;
		}
	}
}
