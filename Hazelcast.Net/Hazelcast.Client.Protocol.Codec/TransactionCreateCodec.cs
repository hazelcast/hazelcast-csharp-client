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

using Hazelcast.IO;

// Client Protocol version, Since:1.0 - Update:1.0
namespace Hazelcast.Client.Protocol.Codec
{
    internal static class TransactionCreateCodec
    {
        private static int CalculateRequestDataSize(long timeout, int durability, int transactionType, long threadId)
        {
            var dataSize = ClientMessage.HeaderSize;
            dataSize += Bits.LongSizeInBytes;
            dataSize += Bits.IntSizeInBytes;
            dataSize += Bits.IntSizeInBytes;
            dataSize += Bits.LongSizeInBytes;
            return dataSize;
        }

        internal static ClientMessage EncodeRequest(long timeout, int durability, int transactionType, long threadId)
        {
            var requiredDataSize = CalculateRequestDataSize(timeout, durability, transactionType, threadId);
            var clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int) TransactionMessageType.TransactionCreate);
            clientMessage.SetRetryable(false);
            clientMessage.Set(timeout);
            clientMessage.Set(durability);
            clientMessage.Set(transactionType);
            clientMessage.Set(threadId);
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }

        internal class ResponseParameters
        {
            public string response;
        }

        internal static ResponseParameters DecodeResponse(IClientMessage clientMessage)
        {
            var parameters = new ResponseParameters();
            var response = clientMessage.GetStringUtf8();
            parameters.response = response;
            return parameters;
        }
    }
}