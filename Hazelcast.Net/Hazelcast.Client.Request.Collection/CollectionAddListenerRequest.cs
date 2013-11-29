using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Collection
{
    public class CollectionAddListenerRequest : IPortable
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

        public virtual int GetFactoryId()
        {
            return CollectionPortableHook.FId;
        }

        public virtual int GetClassId()
        {
            return CollectionPortableHook.CollectionAddListener;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            writer.WriteBoolean("i", includeValue);
            writer.WriteUTF("s", serviceName);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void ReadPortable(IPortableReader reader)
        {
            name = reader.ReadUTF("n");
            includeValue = reader.ReadBoolean("i");
            serviceName = reader.ReadUTF("s");
        }

        public virtual void SetServiceName(string serviceName)
        {
            this.serviceName = serviceName;
        }
    }
}