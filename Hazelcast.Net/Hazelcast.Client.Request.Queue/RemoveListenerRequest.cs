using Hazelcast.Client.Request.Base;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Queue
{
    internal class RemoveListenerRequest : ClientRequest, IRemoveRequest
    {
        private string registrationId;
        private string name;

        public RemoveListenerRequest(string name, string registrationId)
        {
            this.name = name;
            this.registrationId = registrationId;
        }

        public override int GetFactoryId()
        {
            return QueuePortableHook.FId;
        }

        public override int GetClassId()
        {
            return QueuePortableHook.RemoveListener;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            writer.WriteUTF("r", registrationId);
        }

        public string RegistrationId
        {
            get { return registrationId; }
            set { registrationId = value; }
        }
    }
}