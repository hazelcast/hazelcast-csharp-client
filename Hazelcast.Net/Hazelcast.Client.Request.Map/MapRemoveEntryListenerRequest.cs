using Hazelcast.Client.Request.Base;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Map
{
	internal class MapRemoveEntryListenerRequest : BaseClientRemoveListenerRequest
	{

		protected internal MapRemoveEntryListenerRequest(string name, string registrationId) : base(name, registrationId)
		{
		}

		public override int GetFactoryId()
		{
            return MapPortableHook.FId;
		}

		public override int GetClassId()
		{
            return MapPortableHook.RemoveEntryListener;
		}
	}
}
