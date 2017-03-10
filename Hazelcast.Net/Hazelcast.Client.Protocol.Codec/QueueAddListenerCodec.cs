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
using Hazelcast.Logging;

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class QueueAddListenerCodec
    {
        public const int ResponseType = 104;
        public const bool Retryable = true;

        public static readonly QueueMessageType RequestType = QueueMessageType.QueueAddListener;

        public static ResponseParameters DecodeResponse(IClientMessage clientMessage)
        {
            var parameters = new ResponseParameters();
            string response = null;
            response = clientMessage.GetStringUtf8();
            parameters.response = response;
            return parameters;
        }

        public static ClientMessage EncodeRequest(string name, bool includeValue, bool localOnly)
        {
            var requiredDataSize = RequestParameters.CalculateDataSize(name, includeValue, localOnly);
            var clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int) RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.Set(name);
            clientMessage.Set(includeValue);
            clientMessage.Set(localOnly);
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly QueueMessageType TYPE = RequestType;
            public bool includeValue;
            public bool localOnly;
            public string name;

            public static int CalculateDataSize(string name, bool includeValue, bool localOnly)
            {
                var dataSize = ClientMessage.HeaderSize;
                dataSize += ParameterUtil.CalculateDataSize(name);
                dataSize += Bits.BooleanSizeInBytes;
                dataSize += Bits.BooleanSizeInBytes;
                return dataSize;
            }
        }

        //************************ RESPONSE *************************//


        public class ResponseParameters
        {
            public string response;
        }


        //************************ EVENTS *************************//
        public abstract class AbstractEventHandler
        {
            public delegate void HandleItem(IData item, string uuid, int eventType);

            public static void Handle(IClientMessage clientMessage, HandleItem handleItem)
            {
                var messageType = clientMessage.GetMessageType();
                if (messageType == EventMessageConst.EventItem)
                {
                    IData item = null;
                    var item_isNull = clientMessage.GetBoolean();
                    if (!item_isNull)
                    {
                        item = clientMessage.GetData();
                    }
                    string uuid = null;
                    uuid = clientMessage.GetStringUtf8();
                    int eventType;
                    eventType = clientMessage.GetInt();
                    handleItem(item, uuid, eventType);
                    return;
                }
                Logger.GetLogger(typeof (AbstractEventHandler))
                    .Warning("Unknown message type received on event handler :" + clientMessage.GetMessageType());
            }
        }
    }
}