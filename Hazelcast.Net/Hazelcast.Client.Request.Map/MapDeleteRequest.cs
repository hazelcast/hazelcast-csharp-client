using Hazelcast.Client.Request.Base;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Map
{
	internal class MapDeleteRequest : ClientRequest
	{
		protected internal string name;
		protected internal IData key;
		protected internal long threadId;

        public MapDeleteRequest(string name, IData key, long threadId)
		{
			this.name = name;
			this.key = key;
			this.threadId = threadId;
		}

		public override int GetFactoryId()
		{
            return MapPortableHook.FId;
		}

		public override int GetClassId()
		{
            return MapPortableHook.Delete;
		}

		/// <exception cref="System.IO.IOException"></exception>
		public override void Write(IPortableWriter writer)
		{
			writer.WriteUTF("n", name);
			writer.WriteLong("t", threadId);
			IObjectDataOutput output = writer.GetRawDataOutput();
			output.WriteData(key);
		}

	}
}