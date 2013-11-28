using Hazelcast.Client.Request.Multimap;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;


namespace Hazelcast.Client.Request.Multimap
{
	
	public abstract class MultiMapKeyBasedRequest : MultiMapRequest
	{
		internal Data key;

		protected internal MultiMapKeyBasedRequest()
		{
		}

		protected internal MultiMapKeyBasedRequest(string name, Data key) : base(name)
		{
			this.key = key;
		}

		/// <exception cref="System.IO.IOException"></exception>
		public override void WritePortable(IPortableWriter writer)
		{
			base.WritePortable(writer);
			IObjectDataOutput output = writer.GetRawDataOutput();
			key.WriteData(output);
		}

		/// <exception cref="System.IO.IOException"></exception>
		public override void ReadPortable(IPortableReader reader)
		{
			base.ReadPortable(reader);
			IObjectDataInput input = reader.GetRawDataInput();
			key = new Data();
			key.ReadData(input);
		}
	}
}
