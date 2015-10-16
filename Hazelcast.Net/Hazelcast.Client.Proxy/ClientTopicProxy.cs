/*
* Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

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
            var request = TopicAddMessageListenerCodec.EncodeRequest(GetName(), false);
            DistributedEventHandler handler = m => TopicAddMessageListenerCodec.AbstractEventHandler.Handle(m,
                (item, time, uuid) => HandleMessageListener(item, time, uuid, listener));
            return Listen(request, m => TopicAddMessageListenerCodec.DecodeResponse(m).response, 
                GetKey(), handler);
        }

        public virtual bool RemoveMessageListener(string registrationId)
        {
            return StopListening(s => TopicRemoveMessageListenerCodec.EncodeRequest(GetName(), s),
                m => TopicRemoveMessageListenerCodec.DecodeResponse(m).response, registrationId);
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