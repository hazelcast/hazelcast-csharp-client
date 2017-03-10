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
    internal sealed class LockTryLockCodec
    {
        public const int ResponseType = 101;
        public const bool Retryable = false;

        public static readonly LockMessageType RequestType = LockMessageType.LockTryLock;

        public static ResponseParameters DecodeResponse(IClientMessage clientMessage)
        {
            var parameters = new ResponseParameters();
            bool response;
            response = clientMessage.GetBoolean();
            parameters.response = response;
            return parameters;
        }

        public static ClientMessage EncodeRequest(string name, long threadId, long lease, long timeout)
        {
            var requiredDataSize = RequestParameters.CalculateDataSize(name, threadId, lease, timeout);
            var clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int) RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.Set(name);
            clientMessage.Set(threadId);
            clientMessage.Set(lease);
            clientMessage.Set(timeout);
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly LockMessageType TYPE = RequestType;
            public long lease;
            public string name;
            public long threadId;
            public long timeout;

            public static int CalculateDataSize(string name, long threadId, long lease, long timeout)
            {
                var dataSize = ClientMessage.HeaderSize;
                dataSize += ParameterUtil.CalculateDataSize(name);
                dataSize += Bits.LongSizeInBytes;
                dataSize += Bits.LongSizeInBytes;
                dataSize += Bits.LongSizeInBytes;
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