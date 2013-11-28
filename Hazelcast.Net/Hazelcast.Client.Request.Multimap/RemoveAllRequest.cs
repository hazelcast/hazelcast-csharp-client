using Hazelcast.Client.Request.Multimap;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Client.Request.Multimap
{
	
	public class RemoveAllRequest : MultiMapKeyBasedRequest
	{
		internal int threadId = -1;

		public RemoveAllRequest()
		{
		}

		public RemoveAllRequest(string name, Data key, int threadId) : base(name, key)
		{
			this.threadId = threadId;
		}

		public override int GetClassId()
		{
			return MultiMapPortableHook.RemoveAll;
		}

		/// <exception cref="System.IO.IOException"></exception>
		public override void WritePortable(IPortableWriter writer)
		{
			writer.WriteInt("t", threadId);
			base.WritePortable(writer);
		}

		/// <exception cref="System.IO.IOException"></exception>
		public override void ReadPortable(IPortableReader reader)
		{
			threadId = reader.ReadInt("t");
			base.ReadPortable(reader);
		}
	}
}
