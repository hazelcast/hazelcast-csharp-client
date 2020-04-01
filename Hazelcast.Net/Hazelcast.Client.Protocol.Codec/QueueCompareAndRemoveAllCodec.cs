// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
    internal static class QueueCompareAndRemoveAllCodec
    {
        private static int CalculateRequestDataSize(string name, IList<IData> dataList)
        {
            var dataSize = ClientMessage.HeaderSize;
            dataSize += ParameterUtil.CalculateDataSize(name);
            dataSize += Bits.IntSizeInBytes;
            foreach (var dataListItem in dataList)
            {
                dataSize += ParameterUtil.CalculateDataSize(dataListItem);
            }
            return dataSize;
        }

        internal static ClientMessage EncodeRequest(string name, IList<IData> dataList)
        {
            var requiredDataSize = CalculateRequestDataSize(name, dataList);
            var clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int) QueueMessageType.QueueCompareAndRemoveAll);
            clientMessage.SetRetryable(false);
            clientMessage.Set(name);
            clientMessage.Set(dataList.Count);
            foreach (var dataListItem in dataList)
            {
                clientMessage.Set(dataListItem);
            }
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }

        internal class ResponseParameters
        {
            public bool response;
        }

        internal static ResponseParameters DecodeResponse(IClientMessage clientMessage)
        {
            var parameters = new ResponseParameters();
            var response = clientMessage.GetBoolean();
            parameters.response = response;
            return parameters;
        }
    }
}