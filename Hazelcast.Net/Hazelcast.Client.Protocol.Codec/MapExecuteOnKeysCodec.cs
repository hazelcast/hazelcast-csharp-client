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

using System.Collections.Generic;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;

// Client Protocol version, Since:1.0 - Update:1.0
namespace Hazelcast.Client.Protocol.Codec
{
    internal static class MapExecuteOnKeysCodec
    {
        private static int CalculateRequestDataSize(string name, IData entryProcessor, IList<IData> keys)
        {
            var dataSize = ClientMessage.HeaderSize;
            dataSize += ParameterUtil.CalculateDataSize(name);
            dataSize += ParameterUtil.CalculateDataSize(entryProcessor);
            dataSize += Bits.IntSizeInBytes;
            foreach (var keysItem in keys)
            {
                dataSize += ParameterUtil.CalculateDataSize(keysItem);
            }
            return dataSize;
        }

        internal static ClientMessage EncodeRequest(string name, IData entryProcessor, IList<IData> keys)
        {
            var requiredDataSize = CalculateRequestDataSize(name, entryProcessor, keys);
            var clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int) MapMessageType.MapExecuteOnKeys);
            clientMessage.SetRetryable(false);
            clientMessage.Set(name);
            clientMessage.Set(entryProcessor);
            clientMessage.Set(keys.Count);
            foreach (var keysItem in keys)
            {
                clientMessage.Set(keysItem);
            }
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }

        internal class ResponseParameters
        {
            public IList<KeyValuePair<IData, IData>> response;
        }

        internal static ResponseParameters DecodeResponse(IClientMessage clientMessage)
        {
            var parameters = new ResponseParameters();
            var responseSize = clientMessage.GetInt();
            var response = new List<KeyValuePair<IData, IData>>(responseSize);
            for (var responseIndex = 0; responseIndex < responseSize; responseIndex++)
            {
                var responseItemKey = clientMessage.GetData();
                var responseItemVal = clientMessage.GetData();
                var responseItem = new KeyValuePair<IData, IData>(responseItemKey, responseItemVal);
                response.Add(responseItem);
            }
            parameters.response = response;
            return parameters;
        }
    }
}