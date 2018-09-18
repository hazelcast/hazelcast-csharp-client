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

using System;
using System.Collections.Generic;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;

// Client Protocol version, Since:1.4 - Update:1.4
namespace Hazelcast.Client.Protocol.Codec
{
    internal static class MapFetchNearCacheInvalidationMetadataCodec
    {
        private static int CalculateRequestDataSize(IList<string> names, Address address)
        {
            var dataSize = ClientMessage.HeaderSize;
            dataSize += Bits.IntSizeInBytes;
            foreach (var namesItem in names)
            {
                dataSize += ParameterUtil.CalculateDataSize(namesItem);
            }
            dataSize += AddressCodec.CalculateDataSize(address);
            return dataSize;
        }

        internal static ClientMessage EncodeRequest(IList<string> names, Address address)
        {
            var requiredDataSize = CalculateRequestDataSize(names, address);
            var clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int) MapMessageType.MapFetchNearCacheInvalidationMetadata);
            clientMessage.SetRetryable(false);
            clientMessage.Set(names.Count);
            foreach (var namesItem in names)
            {
                clientMessage.Set(namesItem);
            }
            AddressCodec.Encode(address, clientMessage);
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }

        internal class ResponseParameters
        {
            public IList<KeyValuePair<string, IList<KeyValuePair<int, long>>>> namePartitionSequenceList;
            public IList<KeyValuePair<int, Guid>> partitionUuidList;
        }

        internal static ResponseParameters DecodeResponse(IClientMessage clientMessage)
        {
            var parameters = new ResponseParameters();
            var namePartitionSequenceListSize = clientMessage.GetInt();
            var namePartitionSequenceList =
                new List<KeyValuePair<string, IList<KeyValuePair<int, long>>>>(namePartitionSequenceListSize);
            for (var namePartitionSequenceListIndex = 0;
                namePartitionSequenceListIndex < namePartitionSequenceListSize;
                namePartitionSequenceListIndex++)
            {
                var namePartitionSequenceListItemKey = clientMessage.GetStringUtf8();
                var namePartitionSequenceListItemValSize = clientMessage.GetInt();
                var namePartitionSequenceListItemVal = new List<KeyValuePair<int, long>>(namePartitionSequenceListItemValSize);
                for (var namePartitionSequenceListItemValIndex = 0;
                    namePartitionSequenceListItemValIndex < namePartitionSequenceListItemValSize;
                    namePartitionSequenceListItemValIndex++)
                {
                    var namePartitionSequenceListItemValItemKey = clientMessage.GetInt();
                    var namePartitionSequenceListItemValItemVal = clientMessage.GetLong();
                    var namePartitionSequenceListItemValItem =
                        new KeyValuePair<int, long>(namePartitionSequenceListItemValItemKey,
                            namePartitionSequenceListItemValItemVal);
                    namePartitionSequenceListItemVal.Add(namePartitionSequenceListItemValItem);
                }
                var namePartitionSequenceListItem =
                    new KeyValuePair<string, IList<KeyValuePair<int, long>>>(namePartitionSequenceListItemKey,
                        namePartitionSequenceListItemVal);
                namePartitionSequenceList.Add(namePartitionSequenceListItem);
            }
            parameters.namePartitionSequenceList = namePartitionSequenceList;
            var partitionUuidListSize = clientMessage.GetInt();
            var partitionUuidList = new List<KeyValuePair<int, Guid>>(partitionUuidListSize);
            for (var partitionUuidListIndex = 0; partitionUuidListIndex < partitionUuidListSize; partitionUuidListIndex++)
            {
                var partitionUuidListItemKey = clientMessage.GetInt();
                var partitionUuidListItemVal = GuidCodec.Decode(clientMessage);
                var partitionUuidListItem = new KeyValuePair<int, Guid>(partitionUuidListItemKey, partitionUuidListItemVal);
                partitionUuidList.Add(partitionUuidListItem);
            }
            parameters.partitionUuidList = partitionUuidList;
            return parameters;
        }
    }
}