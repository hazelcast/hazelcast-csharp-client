using Hazelcast.Client.Request.Topic;
using Hazelcast.Client.Spi;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Proxy
{
    internal class ClientTopicProxy<E> : ClientProxy, ITopic<E>
    {
        private readonly string name;

        private volatile IData key;

        public ClientTopicProxy(string serviceName, string objectId) : base(serviceName, objectId)
        {
            name = objectId;
        }

        public virtual void Publish(E message)
        {
            IData data = GetContext().GetSerializationService().ToData(message);
            var request = new PublishRequest(name, data);
            Invoke<object>(request);
        }

        public virtual string AddMessageListener(IMessageListener<E> listener)
        {
            var request = new AddMessageListenerRequest(name);
            return Listen(request, GetKey(), args => HandleMessageListener(args, listener));
        }

        public virtual bool RemoveMessageListener(string registrationId)
        {
            var req = new RemoveMessageListenerRequest(name, registrationId);
            return StopListening(req, registrationId);
        }

        private void HandleMessageListener(IData eventData, IMessageListener<E> listener)
        {
            var _event = GetContext().GetSerializationService().ToObject<PortableMessage>(eventData);
            var messageObject = GetContext().GetSerializationService().ToObject<E>(_event.GetMessage());
            IMember member = GetContext().GetClusterService().GetMember(_event.GetUuid());
            var message = new Message<E>(name, messageObject, _event.GetPublishTime(), member);
            listener.OnMessage(message);
        }

        protected override void OnDestroy()
        {
        }

        private IData GetKey()
        {
            return key ?? (key = GetContext().GetSerializationService().ToData(name));
        }
    }
}