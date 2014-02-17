using Hazelcast.Client.Request.Base;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Collection
{
    internal class CollectionAddListenerRequest : ClientRequest
    {
        private bool includeValue;
        private string name;

        private string serviceName;

        public CollectionAddListenerRequest()
        {
        }

        public CollectionAddListenerRequest(string name, bool includeValue)
        {
            this.name = name;
            this.includeValue = includeValue;
        }

        public override int GetFactoryId()
        {
            return CollectionPortableHook.FId;
        }

        public override int GetClassId()
        {
            return CollectionPortableHook.CollectionAddListener;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            writer.WriteBoolean("i", includeValue);
            writer.WriteUTF("s", serviceName);
        }

        public virtual void SetServiceName(string serviceName)
        {
            this.serviceName = serviceName;
        }
    }
}