using Hazelcast.Client.Request.Base;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Collection
{
    internal class CollectionRemoveListenerRequest : BaseClientRemoveListenerRequest
    {
        private string serviceName;

        public CollectionRemoveListenerRequest(string name, string registrationId, string serviceName) :base(name, registrationId)
        {
            this.serviceName = serviceName;
        }

        public override int GetFactoryId()
        {
            return CollectionPortableHook.FId;
        }

        public override int GetClassId()
        {
            return CollectionPortableHook.CollectionRemoveListener;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            base.Write(writer);
            writer.WriteUTF("s", serviceName);
        }
    }
}