using Hazelcast.Client.Request.Base;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Collection
{
    internal class CollectionRemoveListenerRequest : ClientRequest, IRemoveRequest
    {
        private string name;
        private string serviceName;
        private string registrationId;


        public CollectionRemoveListenerRequest(string name, string serviceName, string registrationId)
        {
            this.name = name;
            this.serviceName = serviceName;
            this.registrationId = registrationId;
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
        public override void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            writer.WriteUTF("r", registrationId);
            writer.WriteUTF("s", serviceName);
        }

        public string RegistrationId
        {
            //get { return registrationId; }
            set { registrationId = value; }
        }

    }
}