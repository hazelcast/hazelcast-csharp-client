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
    internal sealed class LockLockCodec
    {
        public const int ResponseType = 100;
        public const bool Retryable = false;

        public static readonly LockMessageType RequestType = LockMessageType.LockLock;

        public static ClientMessage EncodeRequest(string name, long leaseTime, long threadId)
        {
            var requiredDataSize = RequestParameters.CalculateDataSize(name, leaseTime, threadId);
            var clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int) RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.Set(name);
            clientMessage.Set(leaseTime);
            clientMessage.Set(threadId);
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly LockMessageType TYPE = RequestType;
            public long leaseTime;
            public string name;
            public long threadId;

            public static int CalculateDataSize(string name, long leaseTime, long threadId)
            {
                var dataSize = ClientMessage.HeaderSize;
                dataSize += ParameterUtil.CalculateDataSize(name);
                dataSize += Bits.LongSizeInBytes;
                dataSize += Bits.LongSizeInBytes;
                return dataSize;
            }
        }
    }
}