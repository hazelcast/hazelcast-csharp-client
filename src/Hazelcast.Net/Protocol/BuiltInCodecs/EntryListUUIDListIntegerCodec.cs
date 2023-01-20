// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
using System.Linq;
using Hazelcast.Core;
using Hazelcast.Messaging;

namespace Hazelcast.Protocol.BuiltInCodecs
{
    // ReSharper disable once InconsistentNaming
    internal static class EntryListUUIDListIntegerCodec
    {
        public static void Encode(ClientMessage clientMessage, ICollection<KeyValuePair<Guid, IList<int>>> collection)
        {
            var keyList = new List<Guid>(collection.Count);
            clientMessage.Append(Frame.CreateBeginStruct());
            foreach (var (ownerId, partitionIds) in collection)
            {
                keyList.Add(ownerId);
                ListIntegerCodec.Encode(clientMessage, partitionIds);
            }
            clientMessage.Append(Frame.CreateEndStruct());
            ListUUIDCodec.Encode(clientMessage, keyList);
        }

        public static void Encode(ClientMessage clientMessage, ICollection<KeyValuePair<Guid, ICollection<int>>> collection)
        {
            var keyList = new List<Guid>(collection.Count);
            clientMessage.Append(Frame.CreateBeginStruct());
            foreach (var (ownerId, partitionIds) in collection)
            {
                keyList.Add(ownerId);
                ListIntegerCodec.Encode(clientMessage, partitionIds);
            }
            clientMessage.Append(Frame.CreateEndStruct());
            ListUUIDCodec.Encode(clientMessage, keyList);
        }

        // TODO: refactor codecs to work with IDictionary
        public static IList<KeyValuePair<Guid, IList<int>>> Decode(IEnumerator<Frame> iterator)
        {
            var ownerPartitionIds = ListMultiFrameCodec.Decode(iterator, ListIntegerCodec.Decode);
            var ownerIds = ListUUIDCodec.Decode(iterator);

            return (ownerIds, ownerPartitionIds).Combine()
                .Select(x => new KeyValuePair<Guid, IList<int>>(x.Item1, x.Item2))
                .ToList();
        }
    }
}
