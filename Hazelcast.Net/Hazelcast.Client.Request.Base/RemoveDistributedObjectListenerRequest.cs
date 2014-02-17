using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Base
{
    internal class RemoveDistributedObjectListenerRequest : ClientRequest, IRemoveRequest
    {
        private string registrationId;

        public RemoveDistributedObjectListenerRequest(string registrationId="")
        {
            this.registrationId = registrationId;
        }

        public override int GetFactoryId()
        {
            return ClientPortableHook.Id;
        }

        public override int GetClassId()
        {
            return ClientPortableHook.RemoveListener;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("r", registrationId);
        }

        public string RegistrationId
        {
            get { return registrationId; }
            set { registrationId = value; }
        }
    }
}