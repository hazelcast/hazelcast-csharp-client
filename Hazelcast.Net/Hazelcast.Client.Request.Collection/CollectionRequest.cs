using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Collection
{
    public abstract class CollectionRequest : IPortable
    {
        protected internal string name;
        protected internal string serviceName;

        public CollectionRequest()
        {
        }

        public CollectionRequest(string name)
        {
            this.name = name;
        }

        public virtual int GetFactoryId()
        {
            return CollectionPortableHook.FId;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("s", serviceName);
            writer.WriteUTF("n", name);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void ReadPortable(IPortableReader reader)
        {
            serviceName = reader.ReadUTF("s");
            name = reader.ReadUTF("n");
        }

        public abstract int GetClassId();

        public virtual void SetServiceName(string serviceName)
        {
            this.serviceName = serviceName;
        }
    }
}