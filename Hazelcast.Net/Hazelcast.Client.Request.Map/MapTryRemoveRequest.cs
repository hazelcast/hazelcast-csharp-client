using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Client.Request.Map
{
	public class MapTryRemoveRequest : IPortable
	{
		protected internal string name;

		protected internal Data key;

		protected internal int threadId;

		protected internal long timeout;

		public MapTryRemoveRequest()
		{
		}

		public MapTryRemoveRequest(string name, Data key, int threadId, long timeout)
		{
			this.name = name;
			this.key = key;
			this.threadId = threadId;
			this.timeout = timeout;
		}

		public virtual int GetFactoryId()
		{
			return MapPortableHook.FId;
		}

		public virtual int GetClassId()
		{
			return MapPortableHook.TryRemove;
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual void WritePortable(IPortableWriter writer)
		{
			writer.WriteUTF("n", name);
			writer.WriteInt("t", threadId);
			writer.WriteLong("timeout", timeout);
			IObjectDataOutput output = writer.GetRawDataOutput();
			key.WriteData(output);
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual void ReadPortable(IPortableReader reader)
		{
			name = reader.ReadUTF("n");
			threadId = reader.ReadInt("t");
			timeout = reader.ReadLong("timeout");
			IObjectDataInput input = reader.GetRawDataInput();
			key = new Data();
			key.ReadData(input);
		}
	}
}
