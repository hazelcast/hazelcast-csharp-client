using Hazelcast.Client.Request.Base;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Map
{
    internal class MapGetRequest : ClientRequest, IRetryableRequest
	{
		private string name;
		private IData key;
		private bool async;
		private long threadId;

		public MapGetRequest(string name, IData key)
		{
			this.name = name;
			this.key = key;
		}

		public MapGetRequest(string name, IData key, long threadId)
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
			return MapPortableHook.Get;
		}

        public void SetAsAsync()
        {
            async = true;
        }

		/// <exception cref="System.IO.IOException"></exception>
		public override void Write(IPortableWriter writer)
		{
			writer.WriteUTF("n", name);
			writer.WriteBoolean("a", async);
			writer.WriteLong("threadId", threadId);
			IObjectDataOutput output = writer.GetRawDataOutput();
			output.WriteData(key);
		}
	}
}
