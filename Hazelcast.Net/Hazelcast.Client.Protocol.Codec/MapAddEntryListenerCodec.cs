// Copyright (c) 2008, Hazelcast, Inc. All Rights Reserved.
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
    internal sealed class MapAddEntryListenerCodec
    {
        public const int ResponseType = 104;
        public const bool Retryable = true;

        public static readonly MapMessageType RequestType = MapMessageType.MapAddEntryListener;

        public static ResponseParameters DecodeResponse(IClientMessage clientMessage)
        {
            var parameters = new ResponseParameters();
            string response = null;
            response = clientMessage.GetStringUtf8();
            parameters.response = response;
            return parameters;
        }

        public static ClientMessage EncodeRequest(string name, bool includeValue, int listenerFlags, bool localOnly)
        {
            var requiredDataSize = RequestParameters.CalculateDataSize(name, includeValue, listenerFlags, localOnly);
            var clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int) RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.Set(name);
            clientMessage.Set(includeValue);
            clientMessage.Set(listenerFlags);
            clientMessage.Set(localOnly);
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly MapMessageType TYPE = RequestType;
            public bool includeValue;
            public int listenerFlags;
            public bool localOnly;
            public string name;

            public static int CalculateDataSize(string name, bool includeValue, int listenerFlags, bool localOnly)
            {
                var dataSize = ClientMessage.HeaderSize;
                dataSize += ParameterUtil.CalculateDataSize(name);
                dataSize += Bits.BooleanSizeInBytes;
                dataSize += Bits.IntSizeInBytes;
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
            public delegate void HandleEntry(
                IData key, IData value, IData oldValue, IData mergingValue, int eventType, string uuid,
                int numberOfAffectedEntries);

            public static void Handle(IClientMessage clientMessage, HandleEntry handleEntry)
            {
                var messageType = clientMessage.GetMessageType();
                if (messageType == EventMessageConst.EventEntry)
                {
                    IData key = null;
                    var key_isNull = clientMessage.GetBoolean();
                    if (!key_isNull)
                    {
                        key = clientMessage.GetData();
                    }
                    IData value = null;
                    var value_isNull = clientMessage.GetBoolean();
                    if (!value_isNull)
                    {
                        value = clientMessage.GetData();
                    }
                    IData oldValue = null;
                    var oldValue_isNull = clientMessage.GetBoolean();
                    if (!oldValue_isNull)
                    {
                        oldValue = clientMessage.GetData();
                    }
                    IData mergingValue = null;
                    var mergingValue_isNull = clientMessage.GetBoolean();
                    if (!mergingValue_isNull)
                    {
                        mergingValue = clientMessage.GetData();
                    }
                    int eventType;
                    eventType = clientMessage.GetInt();
                    string uuid = null;
                    uuid = clientMessage.GetStringUtf8();
                    int numberOfAffectedEntries;
                    numberOfAffectedEntries = clientMessage.GetInt();
                    handleEntry(key, value, oldValue, mergingValue, eventType, uuid, numberOfAffectedEntries);
                    return;
                }
                Logger.GetLogger(typeof (AbstractEventHandler))
                    .Warning("Unknown message type received on event handler :" + clientMessage.GetMessageType());
            }
        }
    }
}