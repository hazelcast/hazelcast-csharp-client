using System;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Client.Spi;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;
using Hazelcast.Util;

namespace Hazelcast.Client.Proxy
{
    internal class ClientTopicProxy<E> : ClientProxy, ITopic<E>
    {
        private volatile IData key;

        public ClientTopicProxy(string serviceName, string objectId) : base(serviceName, objectId)
        {
        }

        public virtual void Publish(E message)
        {
            IData data = ToData(message);
            var request = TopicPublishCodec.EncodeRequest(GetName(), data);
            Invoke(request);
        }

        public virtual string AddMessageListener(IMessageListener<E> listener)
        {
            var request = TopicAddMessageListenerCodec.EncodeRequest(GetName());
            DistributedEventHandler handler = m => TopicAddMessageListenerCodec.AbstractEventHandler.Handle(m,
                (item, time, uuid) => HandleMessageListener(item, time, uuid, listener));
            return Listen(request, m => TopicAddMessageListenerCodec.DecodeResponse(m).response, 
                GetKey(), handler);
        }

        public virtual bool RemoveMessageListener(string registrationId)
        {
            var req = TopicRemoveMessageListenerCodec.EncodeRequest(GetName(), registrationId);
            return StopListening(req, m => TopicRemoveMessageListenerCodec.DecodeResponse(m).response, registrationId);
        }

        protected override IClientMessage Invoke(IClientMessage request)
        {
            return base.Invoke(request, GetKey());
        }

        protected override T Invoke<T>(IClientMessage request, Func<IClientMessage, T> decodeResponse)
        {
            return base.Invoke(request, GetKey(), decodeResponse);
        }

        private void HandleMessageListener(IData item, long time, string uuid, IMessageListener<E> listener)
        {
            var messageObject = ToObject<E>(item);
            IMember member = GetContext().GetClusterService().GetMember(uuid);
            var message = new Message<E>(GetName(), messageObject, time, member);
            listener.OnMessage(message);
        }

        private IData GetKey()
        {
            return key ?? (key = ToData(GetName()));
        }
    }
}