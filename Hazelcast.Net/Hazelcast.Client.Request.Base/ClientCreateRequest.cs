using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Base
{
    internal class ClientCreateRequest : ClientRequest, IRetryableRequest
    {
        private string name;

        private string serviceName;


        public ClientCreateRequest(string name, string serviceName)
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
            return ClientPortableHook.CreateProxy;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            writer.WriteUTF("s", serviceName);
        }

    }
}