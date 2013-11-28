using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Client.Request.Map
{
	public class MapRemoveIfSameRequest : IPortable
	{
		protected internal string name;

		protected internal Data key;

		protected internal Data value;

		protected internal int threadId;

		public MapRemoveIfSameRequest()
		{
		}

		public MapRemoveIfSameRequest(string name, Data key, Data value, int threadId)
		{
			this.name = name;
			this.key = key;
			this.value = value;
			this.threadId = threadId;
		}

		public virtual int GetFactoryId()
		{
			return MapPortableHook.FId;
		}

		public virtual int GetClassId()
		{
			return MapPortableHook.RemoveIfSame;
		}

		public virtual object GetKey()
		{
			return key;
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual void WritePortable(IPortableWriter writer)
		{
			writer.WriteUTF("n", name);
			writer.WriteInt("t", threadId);
			IObjectDataOutput output = writer.GetRawDataOutput();
			key.WriteData(output);
			value.WriteData(output);
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual void ReadPortable(IPortableReader reader)
		{
			name = reader.ReadUTF("n");
			threadId = reader.ReadInt("t");
			IObjectDataInput input = reader.GetRawDataInput();
			key = new Data();
			key.ReadData(input);
			value = new Data();
			value.ReadData(input);
		}
	}
}
