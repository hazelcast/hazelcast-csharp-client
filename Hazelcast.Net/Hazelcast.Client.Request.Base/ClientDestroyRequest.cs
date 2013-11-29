using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Base
{
    public class ClientDestroyRequest : IPortable, IRetryableRequest
    {
        private string name;

        private string serviceName;

        public ClientDestroyRequest()
        {
        }

        public ClientDestroyRequest(string name, string serviceName)
        {
            this.name = name;
            this.serviceName = serviceName;
        }

        public virtual int GetFactoryId()
        {
            return ClientPortableHook.Id;
        }

        public virtual int GetClassId()
        {
            return ClientPortableHook.DestroyProxy;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            writer.WriteUTF("s", serviceName);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void ReadPortable(IPortableReader reader)
        {
            name = reader.ReadUTF("n");
            serviceName = reader.ReadUTF("s");
        }
    }
}