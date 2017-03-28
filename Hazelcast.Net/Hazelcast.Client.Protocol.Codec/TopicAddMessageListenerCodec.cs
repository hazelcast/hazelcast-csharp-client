// Copyright (c) 2008-2017, Hazelcast, Inc. All Rights Reserved.
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

// Client Protocol version, Since:1.0 - Update:1.0

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class TopicAddMessageListenerCodec
    {
        public static readonly TopicMessageType RequestType = TopicMessageType.TopicAddMessageListener;
        public const int ResponseType = 104;
        public const bool Retryable = false;

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly TopicMessageType TYPE = RequestType;
            public string name;
            public bool localOnly;

            public static int CalculateDataSize(string name, bool localOnly)
            {
                var dataSize = ClientMessage.HeaderSize;
                dataSize += ParameterUtil.CalculateDataSize(name);
                dataSize += Bits.BooleanSizeInBytes;
                return dataSize;
            }
        }

        public static ClientMessage EncodeRequest(string name, bool localOnly)
        {
            var requiredDataSize = RequestParameters.CalculateDataSize(name, localOnly);
            var clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int) RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.Set(name);
            clientMessage.Set(localOnly);
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }

        //************************ RESPONSE *************************//
        public class ResponseParameters
        {
            public string response;
        }

        public static ResponseParameters DecodeResponse(IClientMessage clientMessage)
        {
            var parameters = new ResponseParameters();
            var response = clientMessage.GetStringUtf8();
            parameters.response = response;
            return parameters;
        }

//************************ EVENTS *************************//
        public abstract class AbstractEventHandler
        {
            public static void Handle(IClientMessage clientMessage, HandleTopic handleTopic)
            {
                var messageType = clientMessage.GetMessageType();
                if (messageType == EventMessageConst.EventTopic)
                {
                    var item = clientMessage.GetData();
                    var publishTime = clientMessage.GetLong();
                    var uuid = clientMessage.GetStringUtf8();
                    handleTopic(item, publishTime, uuid);
                    return;
                }
                Hazelcast.Logging.Logger.GetLogger(typeof(AbstractEventHandler))
                    .Warning("Unknown message type received on event handler :" + clientMessage.GetMessageType());
            }

            public delegate void HandleTopic(IData item, long publishTime, string uuid);
        }
    }
}