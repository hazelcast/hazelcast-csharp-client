using Hazelcast.Client.Request.Concurrent.Lock;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Client.Request.Multimap
{
	
	public class MultiMapUnlockRequest : AbstractUnlockRequest
	{
		internal string name;

		public MultiMapUnlockRequest()
		{
		}

		public MultiMapUnlockRequest(Data key, int threadId, string name) : base(key, threadId)
		{
			this.name = name;
		}

		public MultiMapUnlockRequest(Data key, int threadId, bool force, string name) : base(key, threadId, force)
		{
			this.name = name;
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

		public override int GetFactoryId()
		{
			return MultiMapPortableHook.FId;
		}

		public override int GetClassId()
		{
			return MultiMapPortableHook.Unlock;
		}
	}
}
