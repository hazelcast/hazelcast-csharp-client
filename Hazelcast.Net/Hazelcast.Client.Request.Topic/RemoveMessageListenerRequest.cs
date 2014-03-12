using Hazelcast.Client.Request.Base;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Topic
{
    internal class RemoveMessageListenerRequest : ClientRequest, IRemoveRequest
    {
        private string registrationId;
        private string name;


        public RemoveMessageListenerRequest(string name, string registrationId)
        {
            this.name = name;
            this.registrationId = registrationId;
        }

        public override int GetFactoryId()
        {
            return TopicPortableHook.FId;
        }

        public override int GetClassId()
        {
            return TopicPortableHook.RemoveListener;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            writer.WriteUTF("r", registrationId);
        }

        public string RegistrationId
        {
            set { registrationId = value; }
        }
    }
}