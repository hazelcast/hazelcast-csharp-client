using System;
using Hazelcast.Client.Request.Topic;
using Hazelcast.Client.Spi;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;
using Hazelcast.Util;

namespace Hazelcast.Client.Proxy
{
    public class ClientTopicProxy<E> : ClientProxy, ITopic<E>
    {
        private readonly string name;

        private volatile Data key;

        public ClientTopicProxy(string serviceName, string objectId) : base(serviceName, objectId)
        {
            name = objectId;
        }

        public virtual void Publish(E message)
        {
            Data data = GetContext().GetSerializationService().ToData(message);
            var request = new PublishRequest(name, data);
            Invoke<object>(request);
        }

        public virtual string AddMessageListener(IMessageListener<E> listener)
        {
            var request = new AddMessageListenerRequest(name);
            return Listen<PortableMessage>(request, GetKey(), (sender, args) => HandleMessageListener(args, listener));
        }


        public virtual bool RemoveMessageListener(string registrationId)
        {
            return StopListening(registrationId);
        }

        private void HandleMessageListener(PortableMessage _event, IMessageListener<E> listener)
        {
            var messageObject = (E) GetContext().GetSerializationService().ToObject(_event.GetMessage());
            IMember member = GetContext().GetClusterService().GetMember(_event.GetUuid());
            var message = new Message<E>(name, messageObject, _event.GetPublishTime(), member);
            listener.OnMessage(message);
        }

        //    public LocalTopicStats getLocalTopicStats() {
        //        throw new UnsupportedOperationException("Locality is ambiguous for client!!!");
        //    }
        protected internal override void OnDestroy()
        {
        }

        private Data GetKey()
        {
            return key ?? (key = GetContext().GetSerializationService().ToData(name));
        }

        private T Invoke<T>(object req)
        {
            try
            {
                return GetContext().GetInvocationService().InvokeOnKeyOwner<T>(req, GetKey());
            }
            catch (Exception e)
            {
                throw ExceptionUtil.Rethrow(e);
            }
        }
    }
}