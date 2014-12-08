using Hazelcast.Client.Request.Base;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Map
{
    internal class MapTryRemoveRequest : ClientRequest
	{
		protected internal string name;

		protected internal IData key;

		protected internal long threadId;

		protected internal long timeout;

		public MapTryRemoveRequest(string name, IData key, long threadId, long timeout)
		{
			this.name = name;
			this.key = key;
			this.threadId = threadId;
			this.timeout = timeout;
		}

		public override int GetFactoryId()
		{
            return MapPortableHook.FId;
		}

		public override int GetClassId()
		{
            return MapPortableHook.TryRemove;
		}

		/// <exception cref="System.IO.IOException"></exception>
		public override void Write(IPortableWriter writer)
		{
			writer.WriteUTF("n", name);
			writer.WriteLong("t", threadId);
			writer.WriteLong("timeout", timeout);
			IObjectDataOutput output = writer.GetRawDataOutput();
			output.WriteData(key);
		}

		}
	}