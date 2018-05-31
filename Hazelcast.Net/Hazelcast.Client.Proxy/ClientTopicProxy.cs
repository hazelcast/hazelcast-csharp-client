// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Client.Spi;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;
using Hazelcast.Util;

namespace Hazelcast.Client.Proxy
{
    internal class ClientTopicProxy<T> : ClientProxy, ITopic<T>
    {
        private volatile IData _key;

        public ClientTopicProxy(string serviceName, string objectId) : base(serviceName, objectId)
        {
        }

        public virtual void Publish(T message)
        {
            var data = ToData(message);
            var request = TopicPublishCodec.EncodeRequest(GetName(), data);
            Invoke(request);
        }

        public virtual string AddMessageListener(IMessageListener<T> listener)
        {
            return AddMessageListener(listener.OnMessage);
        }

        public virtual string AddMessageListener(Action<Message<T>> listener)
        {
            var request = TopicAddMessageListenerCodec.EncodeRequest(GetName(), IsSmart());
            DistributedEventHandler handler = m =>
                TopicAddMessageListenerCodec.EventHandler.HandleEvent(m,
                    (item, time, uuid) =>
                    {
                        HandleMessageListener(item, time, uuid, listener);
                    });
            return RegisterListener(request, m => TopicAddMessageListenerCodec.DecodeResponse(m).response,
                id => TopicRemoveMessageListenerCodec.EncodeRequest(GetName(), id), handler);
        }

        public virtual bool RemoveMessageListener(string registrationId)
        {
            return DeregisterListener(registrationId);
        }

        protected override IClientMessage Invoke(IClientMessage request)
        {
            return base.Invoke(request, GetKey());
        }

        protected override TT Invoke<TT>(IClientMessage request, Func<IClientMessage, TT> decodeResponse)
        {
            return base.Invoke(request, GetKey(), decodeResponse);
        }

        private IData GetKey()
        {
            return _key ?? (_key = ToData(GetName()));
        }

        private void HandleMessageListener(IData item, long time, string uuid, Action<Message<T>> listener)
        {
            var messageObject = ToObject<T>(item);
            var member = GetContext().GetClusterService().GetMember(uuid);
            var message = new Message<T>(GetName(), messageObject, time, member);
            listener(message);
        }
    }
}