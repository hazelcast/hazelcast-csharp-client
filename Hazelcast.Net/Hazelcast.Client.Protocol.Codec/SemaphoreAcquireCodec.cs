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

// Client Protocol version, Since:1.0 - Update:1.0

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class SemaphoreAcquireCodec
    {
        public static readonly SemaphoreMessageType RequestType = SemaphoreMessageType.SemaphoreAcquire;
        public const int ResponseType = 100;
        public const bool Retryable = false;

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly SemaphoreMessageType TYPE = RequestType;
            public string name;
            public int permits;

            public static int CalculateDataSize(string name, int permits)
            {
                var dataSize = ClientMessage.HeaderSize;
                dataSize += ParameterUtil.CalculateDataSize(name);
                dataSize += Bits.IntSizeInBytes;
                return dataSize;
            }
        }

        public static ClientMessage EncodeRequest(string name, int permits)
        {
            var requiredDataSize = RequestParameters.CalculateDataSize(name, permits);
            var clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int) RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.Set(name);
            clientMessage.Set(permits);
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }

        //************************ RESPONSE IS EMPTY *****************//
    }
}