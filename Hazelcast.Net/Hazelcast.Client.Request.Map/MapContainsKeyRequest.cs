using Hazelcast.Client.Request.Base;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Map
{
	internal class MapContainsKeyRequest : ClientRequest, IRetryableRequest
	{
		private string name;
		private IData key;
		private long threadId;

		public MapContainsKeyRequest(string name, IData key)
		{
			this.name = name;
			this.key = key;
		}

		public override int GetFactoryId()
		{
			return MapPortableHook.FId;
		}

		public override int GetClassId()
		{
			return MapPortableHook.ContainsKey;
		}

		/// <exception cref="System.IO.IOException"></exception>
		public override void Write(IPortableWriter writer)
		{
			writer.WriteUTF("n", name);
			writer.WriteLong("threadId", threadId);
			IObjectDataOutput output = writer.GetRawDataOutput();
			output.WriteData(key);
		}
	}
}
