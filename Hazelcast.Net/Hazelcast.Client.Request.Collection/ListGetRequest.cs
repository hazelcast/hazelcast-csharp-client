using Hazelcast.Client.Request.Collection;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Client.Request.Collection
{
	
	public class ListGetRequest : CollectionRequest
	{
		internal int index = -1;

		public ListGetRequest()
		{
		}

		public ListGetRequest(string name, int index) : base(name)
		{
			this.index = index;
		}

		public override int GetClassId()
		{
			return CollectionPortableHook.ListGet;
		}

		/// <exception cref="System.IO.IOException"></exception>
		public override void WritePortable(IPortableWriter writer)
		{
			base.WritePortable(writer);
			writer.WriteInt("i", index);
		}

		/// <exception cref="System.IO.IOException"></exception>
		public override void ReadPortable(IPortableReader reader)
		{
			base.ReadPortable(reader);
			index = reader.ReadInt("i");
		}
	}
}
