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
using Hazelcast.IO.Serialization;
using System.Collections.Generic;

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class MapExecuteWithPredicateCodec
    {
        public static readonly MapMessageType RequestType = MapMessageType.MapExecuteWithPredicate;
        public const int ResponseType = 117;
        public const bool Retryable = false;

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly MapMessageType TYPE = RequestType;
            public string name;
            public IData entryProcessor;
            public IData predicate;

            public static int CalculateDataSize(string name, IData entryProcessor, IData predicate)
            {
                int dataSize = ClientMessage.HeaderSize;
                dataSize += ParameterUtil.CalculateDataSize(name);
                dataSize += ParameterUtil.CalculateDataSize(entryProcessor);
                dataSize += ParameterUtil.CalculateDataSize(predicate);
                return dataSize;
            }
        }

        public static ClientMessage EncodeRequest(string name, IData entryProcessor, IData predicate)
        {
            int requiredDataSize = RequestParameters.CalculateDataSize(name, entryProcessor, predicate);
            ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int) RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.Set(name);
            clientMessage.Set(entryProcessor);
            clientMessage.Set(predicate);
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }

        //************************ RESPONSE *************************//


        public class ResponseParameters
        {
            public IList<KeyValuePair<IData, IData>> response;
        }

        public static ResponseParameters DecodeResponse(IClientMessage clientMessage)
        {
            ResponseParameters parameters = new ResponseParameters();
            IList<KeyValuePair<IData, IData>> response = null;
            int response_size = clientMessage.GetInt();
            response = new List<KeyValuePair<IData, IData>>();
            for (int response_index = 0; response_index < response_size; response_index++)
            {
                KeyValuePair<IData, IData> response_item;
                IData response_item_key;
                IData response_item_val;
                response_item_key = clientMessage.GetData();
                response_item_val = clientMessage.GetData();
                response_item = new KeyValuePair<IData, IData>(response_item_key,
                    response_item_val);
                response.Add(response_item);
            }
            parameters.response = response;
            return parameters;
        }
    }
}