using Hazelcast.Client.Request.Collection;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Client.Request.Collection
{
	
	public class CollectionAddRequest : CollectionRequest
	{
		protected internal Data value;

		public CollectionAddRequest()
		{
		}

		public CollectionAddRequest(string name, Data value) : base(name)
		{
			this.value = value;
		}

		public override int GetClassId()
		{
			return CollectionPortableHook.CollectionAdd;
		}

		/// <exception cref="System.IO.IOException"></exception>
		public override void WritePortable(IPortableWriter writer)
		{
			base.WritePortable(writer);
			value.WriteData(writer.GetRawDataOutput());
		}

		/// <exception cref="System.IO.IOException"></exception>
		public override void ReadPortable(IPortableReader reader)
		{
			base.ReadPortable(reader);
			value = new Data();
			value.ReadData(reader.GetRawDataInput());
		}
	}
}
