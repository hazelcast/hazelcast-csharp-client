using Hazelcast.Client.Request.Base;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Topic
{
    internal class RemoveMessageListenerRequest : BaseClientRemoveListenerRequest
    {
        protected internal RemoveMessageListenerRequest(string name, string registrationId) : base(name, registrationId)
        {
        }

        public override int GetFactoryId()
        {
            return TopicPortableHook.FId;
        }

        public override int GetClassId()
        {
            return TopicPortableHook.RemoveListener;
        }
    }
}