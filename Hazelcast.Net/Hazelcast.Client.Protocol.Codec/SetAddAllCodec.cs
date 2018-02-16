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

using System.Collections.Generic;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;

// Client Protocol version, Since:1.0 - Update:1.0

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class SetAddAllCodec
    {
        public static readonly SetMessageType RequestType = SetMessageType.SetAddAll;
        public const int ResponseType = 101;
        public const bool Retryable = false;

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly SetMessageType TYPE = RequestType;
            public string name;
            public IList<IData> valueList;

            public static int CalculateDataSize(string name, IList<IData> valueList)
            {
                var dataSize = ClientMessage.HeaderSize;
                dataSize += ParameterUtil.CalculateDataSize(name);
                dataSize += Bits.IntSizeInBytes;
                foreach (var valueListItem in valueList)
                {
                    dataSize += ParameterUtil.CalculateDataSize(valueListItem);
                }
                return dataSize;
            }
        }

        public static ClientMessage EncodeRequest(string name, IList<IData> valueList)
        {
            var requiredDataSize = RequestParameters.CalculateDataSize(name, valueList);
            var clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int) RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.Set(name);
            clientMessage.Set(valueList.Count);
            foreach (var valueListItem in valueList)
            {
                clientMessage.Set(valueListItem);
            }
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }

        //************************ RESPONSE *************************//
        public class ResponseParameters
        {
            public bool response;
        }

        public static ResponseParameters DecodeResponse(IClientMessage clientMessage)
        {
            var parameters = new ResponseParameters();
            var response = clientMessage.GetBoolean();
            parameters.response = response;
            return parameters;
        }
    }
}