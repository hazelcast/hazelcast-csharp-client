using Hazelcast.Client.Request.Base;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Client.Request.Map
{
	public class MapContainsValueRequest : IPortable, IRetryableRequest
	{
		private string name;

		private Data value;

		public MapContainsValueRequest()
		{
		}

		public MapContainsValueRequest(string name, Data value)
		{
			this.name = name;
			this.value = value;
		}

		public virtual int GetFactoryId()
		{
			return MapPortableHook.FId;
		}

		public virtual int GetClassId()
		{
			return MapPortableHook.ContainsValue;
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual void WritePortable(IPortableWriter writer)
		{
			writer.WriteUTF("n", name);
			IObjectDataOutput output = writer.GetRawDataOutput();
			value.WriteData(output);
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual void ReadPortable(IPortableReader reader)
		{
			name = reader.ReadUTF("n");
			IObjectDataInput input = reader.GetRawDataInput();
			value = new Data();
			value.ReadData(input);
		}
	}
}
