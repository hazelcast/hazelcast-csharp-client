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

using Hazelcast.IO;

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class TransactionCreateCodec
    {
        public const int ResponseType = 104;
        public const bool Retryable = false;

        public static readonly TransactionMessageType RequestType = TransactionMessageType.TransactionCreate;

        public static ResponseParameters DecodeResponse(IClientMessage clientMessage)
        {
            var parameters = new ResponseParameters();
            string response = null;
            response = clientMessage.GetStringUtf8();
            parameters.response = response;
            return parameters;
        }

        public static ClientMessage EncodeRequest(long timeout, int durability, int transactionType, long threadId)
        {
            var requiredDataSize = RequestParameters.CalculateDataSize(timeout, durability, transactionType, threadId);
            var clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int) RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.Set(timeout);
            clientMessage.Set(durability);
            clientMessage.Set(transactionType);
            clientMessage.Set(threadId);
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly TransactionMessageType TYPE = RequestType;
            public int durability;
            public long threadId;
            public long timeout;
            public int transactionType;

            public static int CalculateDataSize(long timeout, int durability, int transactionType, long threadId)
            {
                var dataSize = ClientMessage.HeaderSize;
                dataSize += Bits.LongSizeInBytes;
                dataSize += Bits.IntSizeInBytes;
                dataSize += Bits.IntSizeInBytes;
                dataSize += Bits.LongSizeInBytes;
                return dataSize;
            }
        }

        //************************ RESPONSE *************************//


        public class ResponseParameters
        {
            public string response;
        }
    }
}