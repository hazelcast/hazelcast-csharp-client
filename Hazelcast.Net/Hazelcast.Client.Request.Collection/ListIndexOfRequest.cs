using Hazelcast.Client.Request.Collection;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Client.Request.Collection
{
	
	public class ListIndexOfRequest : CollectionRequest
	{
		internal Data value;

		internal bool last;

		public ListIndexOfRequest()
		{
		}

		public ListIndexOfRequest(string name, Data value, bool last) : base(name)
		{
			this.value = value;
			this.last = last;
		}

		public override int GetClassId()
		{
			return CollectionPortableHook.ListIndexOf;
		}

		/// <exception cref="System.IO.IOException"></exception>
		public override void WritePortable(IPortableWriter writer)
		{
			base.WritePortable(writer);
			writer.WriteBoolean("l", last);
			value.WriteData(writer.GetRawDataOutput());
		}

		/// <exception cref="System.IO.IOException"></exception>
		public override void ReadPortable(IPortableReader reader)
		{
			base.ReadPortable(reader);
			last = reader.ReadBoolean("l");
			value = new Data();
			value.ReadData(reader.GetRawDataInput());
		}
	}
}
