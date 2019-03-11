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
    internal static class SetAddListenerCodec
    {
        private static int CalculateRequestDataSize(string name, bool includeValue, bool localOnly)
        {
            var dataSize = ClientMessage.HeaderSize;
            dataSize += ParameterUtil.CalculateDataSize(name);
            dataSize += Bits.BooleanSizeInBytes;
            dataSize += Bits.BooleanSizeInBytes;
            return dataSize;
        }

        internal static ClientMessage EncodeRequest(string name, bool includeValue, bool localOnly)
        {
            var requiredDataSize = CalculateRequestDataSize(name, includeValue, localOnly);
            var clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int) SetMessageType.SetAddListener);
            clientMessage.SetRetryable(false);
            clientMessage.Set(name);
            clientMessage.Set(includeValue);
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
            internal static void HandleEvent(IClientMessage clientMessage, HandleItemEventV10 handleItemEventV10)
            {
                var messageType = clientMessage.GetMessageType();
                if (messageType == EventMessageConst.EventItem)
                {
                    IData item = null;
                    var itemIsNull = clientMessage.GetBoolean();
                    if (!itemIsNull)
                    {
                        item = clientMessage.GetData();
                    }
                    var uuid = clientMessage.GetStringUtf8();
                    var eventType = clientMessage.GetInt();
                    handleItemEventV10(item, uuid, eventType);
                    return;
                }
                Logger.GetLogger(typeof(EventHandler)).Warning("Unknown message type received on event handler :" + messageType);
            }

            internal delegate void HandleItemEventV10(IData item, string uuid, int eventType);
        }
    }
}