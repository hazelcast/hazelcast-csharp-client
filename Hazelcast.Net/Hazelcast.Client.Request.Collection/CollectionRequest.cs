using Hazelcast.Client.Request.Base;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Collection
{
    internal abstract class CollectionRequest : ClientRequest
    {
        protected internal string name;
        protected internal string serviceName;


        protected CollectionRequest(string name)
        {
            this.name = name;
        }

        public override int GetFactoryId()
        {
            return CollectionPortableHook.FId;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("s", serviceName);
            writer.WriteUTF("n", name);
        }

        public virtual void SetServiceName(string serviceName)
        {
            this.serviceName = serviceName;
        }
    }
}