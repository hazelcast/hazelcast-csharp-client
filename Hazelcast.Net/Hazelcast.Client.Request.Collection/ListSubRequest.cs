using Hazelcast.Client.Request.Collection;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Client.Request.Collection
{
	
	public class ListSubRequest : CollectionRequest
	{
		private int from;

		private int to;

		public ListSubRequest()
		{
		}

		public ListSubRequest(string name, int from, int to) : base(name)
		{
			this.from = from;
			this.to = to;
		}

		public override int GetClassId()
		{
			return CollectionPortableHook.ListSub;
		}

		/// <exception cref="System.IO.IOException"></exception>
		public override void WritePortable(IPortableWriter writer)
		{
			base.WritePortable(writer);
			writer.WriteInt("f", from);
			writer.WriteInt("t", to);
		}

		/// <exception cref="System.IO.IOException"></exception>
		public override void ReadPortable(IPortableReader reader)
		{
			base.ReadPortable(reader);
			from = reader.ReadInt("f");
			to = reader.ReadInt("t");
		}
	}
}
