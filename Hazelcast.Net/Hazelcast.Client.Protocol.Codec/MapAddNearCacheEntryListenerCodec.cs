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

using System.Collections.Generic;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Logging;

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class MapAddNearCacheEntryListenerCodec
    {
        public const int ResponseType = 104;
        public const bool Retryable = true;

        public static readonly MapMessageType RequestType = MapMessageType.MapAddNearCacheEntryListener;

        public static ResponseParameters DecodeResponse(IClientMessage clientMessage)
        {
            var parameters = new ResponseParameters();
            string response = null;
            response = clientMessage.GetStringUtf8();
            parameters.response = response;
            return parameters;
        }

        public static ClientMessage EncodeRequest(string name, int listenerFlags, bool localOnly)
        {
            var requiredDataSize = RequestParameters.CalculateDataSize(name, listenerFlags, localOnly);
            var clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int) RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.Set(name);
            clientMessage.Set(listenerFlags);
            clientMessage.Set(localOnly);
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly MapMessageType TYPE = RequestType;
            public int listenerFlags;
            public bool localOnly;
            public string name;

            public static int CalculateDataSize(string name, int listenerFlags, bool localOnly)
            {
                var dataSize = ClientMessage.HeaderSize;
                dataSize += ParameterUtil.CalculateDataSize(name);
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
            public delegate void HandleIMapBatchInvalidation(IList<IData> keys);

            public delegate void HandleIMapInvalidation(IData key);

            public static void Handle(IClientMessage clientMessage, HandleIMapInvalidation handleIMapInvalidation,
                HandleIMapBatchInvalidation handleIMapBatchInvalidation)
            {
                var messageType = clientMessage.GetMessageType();
                if (messageType == EventMessageConst.EventIMapInvalidation)
                {
                    IData key = null;
                    var key_isNull = clientMessage.GetBoolean();
                    if (!key_isNull)
                    {
                        key = clientMessage.GetData();
                    }
                    handleIMapInvalidation(key);
                    return;
                }
                if (messageType == EventMessageConst.EventIMapBatchInvalidation)
                {
                    IList<IData> keys = null;
                    var keys_size = clientMessage.GetInt();
                    keys = new List<IData>();
                    for (var keys_index = 0; keys_index < keys_size; keys_index++)
                    {
                        IData keys_item;
                        keys_item = clientMessage.GetData();
                        keys.Add(keys_item);
                    }
                    handleIMapBatchInvalidation(keys);
                    return;
                }
                Logger.GetLogger(typeof (AbstractEventHandler))
                    .Warning("Unknown message type received on event handler :" + clientMessage.GetMessageType());
            }
        }
    }
}