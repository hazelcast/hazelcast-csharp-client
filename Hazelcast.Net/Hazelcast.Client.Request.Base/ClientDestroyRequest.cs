using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Base
{
    public class ClientDestroyRequest : ClientRequest, IRetryableRequest
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

        public override int GetFactoryId()
        {
            return ClientPortableHook.Id;
        }

        public override int GetClassId()
        {
            return ClientPortableHook.DestroyProxy;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            writer.WriteUTF("s", serviceName);
        }

    }
}