using Hazelcast.Client.Request.Base;
using Hazelcast.Client.Request.Queue;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Client.Request.Queue
{
	
	public class SizeRequest : QueueRequest, IRetryableRequest
	{
		public SizeRequest()
		{
		}

		public SizeRequest(string name) : base(name)
		{
		}

		public override int GetClassId()
		{
			return QueuePortableHook.Size;
		}
	}
}
