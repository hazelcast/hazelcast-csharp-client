using Hazelcast.Client.Request.Base;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Client.Request.Map
{
	public class MapKeySetRequest : IPortable, IRetryableRequest
	{
		private string name;

		public MapKeySetRequest()
		{
		}

		public MapKeySetRequest(string name)
		{
			this.name = name;
		}

		public virtual int GetFactoryId()
		{
			return MapPortableHook.FId;
		}

		public virtual int GetClassId()
		{
			return MapPortableHook.KeySet;
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual void WritePortable(IPortableWriter writer)
		{
			writer.WriteUTF("n", name);
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual void ReadPortable(IPortableReader reader)
		{
			name = reader.ReadUTF("n");
		}
	}
}
