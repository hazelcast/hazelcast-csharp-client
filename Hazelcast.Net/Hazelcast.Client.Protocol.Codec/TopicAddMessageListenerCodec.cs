// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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

using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Logging;

// Client Protocol version, Since:1.0 - Update:1.0
namespace Hazelcast.Client.Protocol.Codec
{
    internal static class TopicAddMessageListenerCodec
    {
        private static int CalculateRequestDataSize(string name, bool localOnly)
        {
            var dataSize = ClientMessage.HeaderSize;
            dataSize += ParameterUtil.CalculateDataSize(name);
            dataSize += Bits.BooleanSizeInBytes;
            return dataSize;
        }

        internal static ClientMessage EncodeRequest(string name, bool localOnly)
        {
            var requiredDataSize = CalculateRequestDataSize(name, localOnly);
            var clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int) TopicMessageType.TopicAddMessageListener);
            clientMessage.SetRetryable(false);
            clientMessage.Set(name);
            clientMessage.Set(localOnly);
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }

        internal class ResponseParameters
        {
            public string response;
        }

        internal static ResponseParameters DecodeResponse(IClientMessage clientMessage)
        {
            var parameters = new ResponseParameters();
            var response = clientMessage.GetStringUtf8();
            parameters.response = response;
            return parameters;
        }

        internal class EventHandler
        {
            internal static void HandleEvent(IClientMessage clientMessage, HandleTopicEventV10 handleTopicEventV10)
            {
                var messageType = clientMessage.GetMessageType();
                if (messageType == EventMessageConst.EventTopic)
                {
                    var item = clientMessage.GetData();
                    var publishTime = clientMessage.GetLong();
                    var uuid = clientMessage.GetStringUtf8();
                    handleTopicEventV10(item, publishTime, uuid);
                    return;
                }
                Logger.GetLogger(typeof(EventHandler)).Warning("Unknown message type received on event handler :" + messageType);
            }

            internal delegate void HandleTopicEventV10(IData item, long publishTime, string uuid);
        }
    }
}