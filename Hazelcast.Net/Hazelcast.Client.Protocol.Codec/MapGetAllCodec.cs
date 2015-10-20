/*
* Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using System.Collections.Generic;

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class MapGetAllCodec
    {

        public static readonly MapMessageType RequestType = MapMessageType.MapGetAll;
        public const int ResponseType = 114;
        public const bool Retryable = false;

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly MapMessageType TYPE = RequestType;
            public string name;
            public ISet<IData> keys;

            public static int CalculateDataSize(string name, ISet<IData> keys)
            {
                int dataSize = ClientMessage.HeaderSize;
                dataSize += ParameterUtil.CalculateDataSize(name);
                dataSize += Bits.IntSizeInBytes;
                foreach (var keys_item in keys )
                {
                dataSize += ParameterUtil.CalculateDataSize(keys_item);
                }
                return dataSize;
            }
        }

        public static ClientMessage EncodeRequest(string name, ISet<IData> keys)
        {
            int requiredDataSize = RequestParameters.CalculateDataSize(name, keys);
            ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int)RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.Set(name);
            clientMessage.Set(keys.Count);
            foreach (var keys_item in keys) {
            clientMessage.Set(keys_item);
            }
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }

        //************************ RESPONSE *************************//


        public class ResponseParameters
        {
            public ISet<KeyValuePair<IData,IData>> entrySet;
        }

        public static ResponseParameters DecodeResponse(IClientMessage clientMessage)
        {
            ResponseParameters parameters = new ResponseParameters();
            ISet<KeyValuePair<IData,IData>> entrySet = null;
            int entrySet_size = clientMessage.GetInt();
            entrySet = new HashSet<KeyValuePair<IData,IData>>();
            for (int entrySet_index = 0; entrySet_index<entrySet_size; entrySet_index++) {
                KeyValuePair<IData,IData> entrySet_item;
            entrySet_item = clientMessage.GetMapEntry();
                entrySet.Add(entrySet_item);
            }
            parameters.entrySet = entrySet;
            return parameters;
        }

    }
}
