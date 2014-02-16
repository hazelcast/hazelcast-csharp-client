using Hazelcast.Client.Request.Base;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Map
{
    public class RemoveEntryListenerRequest : ClientRequest, IRemoveRequest
    {
        private string name;
        private string registrationId;

        public RemoveEntryListenerRequest()
        {
        }

        public RemoveEntryListenerRequest(string name, string registrationId)
        {
            this.name = name;
            this.registrationId = registrationId;
        }

        public override int GetFactoryId()
        {
            return MultiMapPortableHook.FId;
        }

        public override int GetClassId()
        {
            return MultiMapPortableHook.RemoveEntryListener;
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