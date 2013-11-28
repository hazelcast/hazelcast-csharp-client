using Hazelcast.Client.Request.Queue;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Client.Request.Queue
{
	
	public class PollRequest : QueueRequest
	{
		public PollRequest()
		{
		}

		public PollRequest(string name) : base(name)
		{
		}

		public PollRequest(string name, long timeoutMillis) : base(name, timeoutMillis)
		{
		}

		public override int GetClassId()
		{
			return QueuePortableHook.Poll;
		}
	}
}
