// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;

// Client Protocol version, Since:1.0 - Update:1.0
namespace Hazelcast.Client.Protocol.Codec
{
    internal static class MapGetAllCodec
    {
        private static int CalculateRequestDataSize(string name, ArrayList keys)
        {
            var dataSize = ClientMessage.HeaderSize;
            dataSize += ParameterUtil.CalculateDataSize(name);
            dataSize += Bits.IntSizeInBytes;
            for (int i = 0; i < keys.Count; i++)
            {
                dataSize += ParameterUtil.CalculateDataSize((IData)keys[i]);
            }
            return dataSize;
        }

        internal static ClientMessage EncodeRequest(string name, ArrayList keys)
        {
            var requiredDataSize = CalculateRequestDataSize(name, keys);
            var clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int) MapMessageType.MapGetAll);
            clientMessage.SetRetryable(false);
            clientMessage.Set(name);
            clientMessage.Set(keys.Count);
            for (int i = 0; i < keys.Count; i++)
            {
                clientMessage.Set((IData)keys[i]);
            }
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }

        internal static void DecodeResponse(IClientMessage clientMessage, ConcurrentQueue<KeyValuePair<IData, object>> result)
        {
            var responseSize = clientMessage.GetInt();
            for (var responseIndex = 0; responseIndex < responseSize; responseIndex++)
            {
                var responseItemKey = clientMessage.GetData();
                var responseItemVal = clientMessage.GetData();
                
                var responseItem = new KeyValuePair<IData, object>(responseItemKey, responseItemVal);
                result.Enqueue(responseItem);
            }
        }
    }
}