using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Map;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Client.Request.Map
{
	public class MapPutAllRequest : IPortable
	{
		protected internal string name;

		private MapEntrySet entrySet;

		public MapPutAllRequest()
		{
		}

		public MapPutAllRequest(string name, MapEntrySet entrySet)
		{
			this.name = name;
			this.entrySet = entrySet;
		}

		public virtual int GetFactoryId()
		{
			return MapPortableHook.FId;
		}

		public virtual int GetClassId()
		{
			return MapPortableHook.PutAll;
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual void WritePortable(IPortableWriter writer)
		{
			writer.WriteUTF("n", name);
			IObjectDataOutput output = writer.GetRawDataOutput();
			entrySet.WriteData(output);
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual void ReadPortable(IPortableReader reader)
		{
			name = reader.ReadUTF("n");
			IObjectDataInput input = reader.GetRawDataInput();
			entrySet = new MapEntrySet();
			entrySet.ReadData(input);
		}
	}
}
