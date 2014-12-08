using Hazelcast.Client.Request.Base;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Multimap
{
    internal class RemoveEntryListenerRequest : BaseClientRemoveListenerRequest
    {
        public RemoveEntryListenerRequest(string name, string registrationId)
            : base(name, registrationId)
        {
        }

        public override int GetFactoryId()
        {
            return MultiMapPortableHook.FId;
        }

        public override int GetClassId()
        {
            return MultiMapPortableHook.RemoveEntryListener;
        }
    }
}