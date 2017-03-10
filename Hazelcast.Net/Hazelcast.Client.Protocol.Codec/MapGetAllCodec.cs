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

using System.Collections.Generic;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class MapGetAllCodec
    {
        public const int ResponseType = 114;
        public const bool Retryable = false;

        public static readonly MapMessageType RequestType = MapMessageType.MapGetAll;

        public static ResponseParameters DecodeResponse(IClientMessage clientMessage)
        {
            var parameters = new ResponseParameters();
            IList<KeyValuePair<IData, IData>> entrySet = null;
            var entrySet_size = clientMessage.GetInt();
            entrySet = new List<KeyValuePair<IData, IData>>();
            for (var entrySet_index = 0; entrySet_index < entrySet_size; entrySet_index++)
            {
                KeyValuePair<IData, IData> entrySet_item;
                entrySet_item = clientMessage.GetMapEntry();
                entrySet.Add(entrySet_item);
            }
            parameters.entrySet = entrySet;
            return parameters;
        }

        public static ClientMessage EncodeRequest(string name, IList<IData> keys)
        {
            var requiredDataSize = RequestParameters.CalculateDataSize(name, keys);
            var clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int) RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.Set(name);
            clientMessage.Set(keys.Count);
            foreach (var keys_item in keys)
            {
                clientMessage.Set(keys_item);
            }
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly MapMessageType TYPE = RequestType;
            public IList<IData> keys;
            public string name;

            public static int CalculateDataSize(string name, IList<IData> keys)
            {
                var dataSize = ClientMessage.HeaderSize;
                dataSize += ParameterUtil.CalculateDataSize(name);
                dataSize += Bits.IntSizeInBytes;
                foreach (var keys_item in keys)
                {
                    dataSize += ParameterUtil.CalculateDataSize(keys_item);
                }
                return dataSize;
            }
        }

        //************************ RESPONSE *************************//


        public class ResponseParameters
        {
            public IList<KeyValuePair<IData, IData>> entrySet;
        }
    }
}