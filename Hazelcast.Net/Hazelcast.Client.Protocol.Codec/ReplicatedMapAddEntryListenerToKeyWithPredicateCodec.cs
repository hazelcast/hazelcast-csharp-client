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

using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Logging;

// Client Protocol version, Since:1.0 - Update:1.0
namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class ReplicatedMapAddEntryListenerToKeyWithPredicateCodec
    {
        public static readonly ReplicatedMapMessageType RequestType =
            ReplicatedMapMessageType.ReplicatedMapAddEntryListenerToKeyWithPredicate;

        public const int ResponseType = 104;
        public const bool Retryable = false;

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly ReplicatedMapMessageType TYPE = RequestType;
            public string name;
            public IData key;
            public IData predicate;
            public bool localOnly;

            public static int CalculateDataSize(string name, IData key, IData predicate, bool localOnly)
            {
                var dataSize = ClientMessage.HeaderSize;
                dataSize += ParameterUtil.CalculateDataSize(name);
                dataSize += ParameterUtil.CalculateDataSize(key);
                dataSize += ParameterUtil.CalculateDataSize(predicate);
                dataSize += Bits.BooleanSizeInBytes;
                return dataSize;
            }
        }

        public static ClientMessage EncodeRequest(string name, IData key, IData predicate, bool localOnly)
        {
            var requiredDataSize = RequestParameters.CalculateDataSize(name, key, predicate, localOnly);
            var clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int) RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.Set(name);
            clientMessage.Set(key);
            clientMessage.Set(predicate);
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
            public static void Handle(IClientMessage clientMessage, HandleEntry handleEntry)
            {
                var messageType = clientMessage.GetMessageType();
                if (messageType == EventMessageConst.EventEntry)
                {
                    IData key = null;
                    var keyIsNull = clientMessage.GetBoolean();
                    if (!keyIsNull)
                    {
                        key = clientMessage.GetData();
                    }
                    IData value = null;
                    var valueIsNull = clientMessage.GetBoolean();
                    if (!valueIsNull)
                    {
                        value = clientMessage.GetData();
                    }
                    IData oldValue = null;
                    var oldValueIsNull = clientMessage.GetBoolean();
                    if (!oldValueIsNull)
                    {
                        oldValue = clientMessage.GetData();
                    }
                    IData mergingValue = null;
                    var mergingValueIsNull = clientMessage.GetBoolean();
                    if (!mergingValueIsNull)
                    {
                        mergingValue = clientMessage.GetData();
                    }
                    var eventType = clientMessage.GetInt();
                    var uuid = clientMessage.GetStringUtf8();
                    var numberOfAffectedEntries = clientMessage.GetInt();
                    handleEntry(key, value, oldValue, mergingValue, eventType, uuid, numberOfAffectedEntries);
                    return;
                }
                Logger.GetLogger(typeof(AbstractEventHandler))
                    .Warning("Unknown message type received on event handler :" + clientMessage.GetMessageType());
            }

            public delegate void HandleEntry(IData key, IData value, IData oldValue, IData mergingValue, int eventType,
                string uuid, int numberOfAffectedEntries);
        }
    }
}