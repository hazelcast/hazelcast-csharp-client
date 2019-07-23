// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.IO;

// Client Protocol version, Since:1.0 - Update:1.5
namespace Hazelcast.Client.Protocol.Codec
{
    internal static class ClientGetPartitionsCodec
    {
        private static int CalculateRequestDataSize()
        {
            var dataSize = ClientMessage.HeaderSize;
            return dataSize;
        }

        internal static ClientMessage EncodeRequest()
        {
            var requiredDataSize = CalculateRequestDataSize();
            var clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int) ClientMessageType.ClientGetPartitions);
            clientMessage.SetRetryable(false);
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }

        internal class ResponseParameters
        {
            public IList<KeyValuePair<Address, IList<int>>> partitions;
            public int partitionStateVersion;
            public bool partitionStateVersionExist;
        }

        internal static ResponseParameters DecodeResponse(IClientMessage clientMessage)
        {
            var parameters = new ResponseParameters();
            var partitionsSize = clientMessage.GetInt();
            var partitions = new List<KeyValuePair<Address, IList<int>>>(partitionsSize);
            for (var partitionsIndex = 0; partitionsIndex < partitionsSize; partitionsIndex++)
            {
                var partitionsItemKey = AddressCodec.Decode(clientMessage);
                var partitionsItemValSize = clientMessage.GetInt();
                var partitionsItemVal = new List<int>(partitionsItemValSize);
                for (var partitionsItemValIndex = 0; partitionsItemValIndex < partitionsItemValSize; partitionsItemValIndex++)
                {
                    var partitionsItemValItem = clientMessage.GetInt();
                    partitionsItemVal.Add(partitionsItemValItem);
                }
                var partitionsItem = new KeyValuePair<Address, IList<int>>(partitionsItemKey, partitionsItemVal);
                partitions.Add(partitionsItem);
            }
            parameters.partitions = partitions;
            if (clientMessage.IsComplete())
            {
                return parameters;
            }
            var partitionStateVersion = clientMessage.GetInt();
            parameters.partitionStateVersion = partitionStateVersion;
            parameters.partitionStateVersionExist = true;
            return parameters;
        }
    }
}