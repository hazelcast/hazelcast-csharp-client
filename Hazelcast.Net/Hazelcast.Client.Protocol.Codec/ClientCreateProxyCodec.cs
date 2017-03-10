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

using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class ClientCreateProxyCodec
    {
        public const int ResponseType = 100;
        public const bool Retryable = false;

        public static readonly ClientMessageType RequestType = ClientMessageType.ClientCreateProxy;

        public static ResponseParameters DecodeResponse(IClientMessage clientMessage)
        {
            var parameters = new ResponseParameters();
            return parameters;
        }

        public static ClientMessage EncodeRequest(string name, string serviceName, Address target)
        {
            var requiredDataSize = RequestParameters.CalculateDataSize(name, serviceName, target);
            var clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int) RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.Set(name);
            clientMessage.Set(serviceName);
            AddressCodec.Encode(target, clientMessage);
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly ClientMessageType TYPE = RequestType;
            public string name;
            public string serviceName;
            public Address target;

            public static int CalculateDataSize(string name, string serviceName, Address target)
            {
                var dataSize = ClientMessage.HeaderSize;
                dataSize += ParameterUtil.CalculateDataSize(name);
                dataSize += ParameterUtil.CalculateDataSize(serviceName);
                dataSize += AddressCodec.CalculateDataSize(target);
                return dataSize;
            }
        }

        //************************ RESPONSE *************************//


        public class ResponseParameters
        {
        }
    }
}