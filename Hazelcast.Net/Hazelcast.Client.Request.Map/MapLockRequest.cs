using Hazelcast.Client.Request.Concurrent.Lock;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Client.Request.Map
{
	public class MapLockRequest : AbstractLockRequest
	{
		private string name;

		public MapLockRequest()
		{
		}

		public MapLockRequest(string name, Data key, int threadId) : base(key, threadId)
		{
			this.name = name;
		}

		public MapLockRequest(string name, Data key, int threadId, long ttl, long timeout) : base(key, threadId, ttl, timeout)
		{
			this.name = name;
		}

		public override int GetFactoryId()
		{
			return MapPortableHook.FId;
		}

		public override int GetClassId()
		{
			return MapPortableHook.Lock;
		}

		/// <exception cref="System.IO.IOException"></exception>
		public override void WritePortable(IPortableWriter writer)
		{
			writer.WriteUTF("n", name);
			base.WritePortable(writer);
		}

		/// <exception cref="System.IO.IOException"></exception>
		public override void ReadPortable(IPortableReader reader)
		{
			name = reader.ReadUTF("n");
			base.ReadPortable(reader);
		}
	}
}
