// Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class RingbufferAddAsyncCodec
    {
        public const int ResponseType = 103;
        public const bool Retryable = false;

        public static readonly RingbufferMessageType RequestType = RingbufferMessageType.RingbufferAddAsync;

        public static ResponseParameters DecodeResponse(IClientMessage clientMessage)
        {
            var parameters = new ResponseParameters();
            long response;
            response = clientMessage.GetLong();
            parameters.response = response;
            return parameters;
        }

        public static ClientMessage EncodeRequest(string name, int overflowPolicy, IData value)
        {
            var requiredDataSize = RequestParameters.CalculateDataSize(name, overflowPolicy, value);
            var clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int) RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.Set(name);
            clientMessage.Set(overflowPolicy);
            clientMessage.Set(value);
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly RingbufferMessageType TYPE = RequestType;
            public string name;
            public int overflowPolicy;
            public IData value;

            public static int CalculateDataSize(string name, int overflowPolicy, IData value)
            {
                var dataSize = ClientMessage.HeaderSize;
                dataSize += ParameterUtil.CalculateDataSize(name);
                dataSize += Bits.IntSizeInBytes;
                dataSize += ParameterUtil.CalculateDataSize(value);
                return dataSize;
            }
        }

        //************************ RESPONSE *************************//


        public class ResponseParameters
        {
            public long response;
        }
    }
}