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
using Hazelcast.IO.Serialization;

// Client Protocol version, Since:1.0 - Update:1.2

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class MapTryLockCodec
    {
        public static readonly MapMessageType RequestType = MapMessageType.MapTryLock;
        public const int ResponseType = 101;
        public const bool Retryable = true;

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly MapMessageType TYPE = RequestType;
            public string name;
            public IData key;
            public long threadId;
            public long lease;
            public long timeout;
            public long referenceId;

            public static int CalculateDataSize(string name, IData key, long threadId, long lease, long timeout,
                long referenceId)
            {
                var dataSize = ClientMessage.HeaderSize;
                dataSize += ParameterUtil.CalculateDataSize(name);
                dataSize += ParameterUtil.CalculateDataSize(key);
                dataSize += Bits.LongSizeInBytes;
                dataSize += Bits.LongSizeInBytes;
                dataSize += Bits.LongSizeInBytes;
                dataSize += Bits.LongSizeInBytes;
                return dataSize;
            }
        }

        public static ClientMessage EncodeRequest(string name, IData key, long threadId, long lease, long timeout,
            long referenceId)
        {
            var requiredDataSize = RequestParameters.CalculateDataSize(name, key, threadId, lease, timeout, referenceId);
            var clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int) RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.Set(name);
            clientMessage.Set(key);
            clientMessage.Set(threadId);
            clientMessage.Set(lease);
            clientMessage.Set(timeout);
            clientMessage.Set(referenceId);
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