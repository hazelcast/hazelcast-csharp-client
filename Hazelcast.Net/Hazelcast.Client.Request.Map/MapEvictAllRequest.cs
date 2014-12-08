using Hazelcast.Client.Request.Base;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Map
{
	/// <summary>Evict all entries request used by clients.</summary>
	internal class MapEvictAllRequest : ClientRequest, IRetryableRequest
	{
		private string name;

		public MapEvictAllRequest(string name)
		{
			this.name = name;
		}

		public override int GetFactoryId()
		{
			return MapPortableHook.FId;
		}

		public override int GetClassId()
		{
			return MapPortableHook.EvictAll;
		}

		/// <exception cref="System.IO.IOException"></exception>
		public override void Write(IPortableWriter writer)
		{
			writer.WriteUTF("n", name);
		}
	}
}
