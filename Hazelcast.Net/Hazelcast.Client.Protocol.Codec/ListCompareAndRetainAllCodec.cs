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

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class ListCompareAndRetainAllCodec
    {
        public const int ResponseType = 101;
        public const bool Retryable = false;

        public static readonly ListMessageType RequestType = ListMessageType.ListCompareAndRetainAll;

        public static ResponseParameters DecodeResponse(IClientMessage clientMessage)
        {
            var parameters = new ResponseParameters();
            bool response;
            response = clientMessage.GetBoolean();
            parameters.response = response;
            return parameters;
        }

        public static ClientMessage EncodeRequest(string name, IList<IData> valueSet)
        {
            var requiredDataSize = RequestParameters.CalculateDataSize(name, valueSet);
            var clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int) RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.Set(name);
            clientMessage.Set(valueSet.Count);
            foreach (var valueSet_item in valueSet)
            {
                clientMessage.Set(valueSet_item);
            }
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly ListMessageType TYPE = RequestType;
            public string name;
            public IList<IData> valueSet;

            public static int CalculateDataSize(string name, IList<IData> valueSet)
            {
                var dataSize = ClientMessage.HeaderSize;
                dataSize += ParameterUtil.CalculateDataSize(name);
                dataSize += Bits.IntSizeInBytes;
                foreach (var valueSet_item in valueSet)
                {
                    dataSize += ParameterUtil.CalculateDataSize(valueSet_item);
                }
                return dataSize;
            }
        }

        //************************ RESPONSE *************************//


        public class ResponseParameters
        {
            public bool response;
        }
    }
}