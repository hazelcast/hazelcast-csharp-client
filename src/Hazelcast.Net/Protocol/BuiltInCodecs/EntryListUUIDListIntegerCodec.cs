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

using System;
using System.Collections.Generic;
using Hazelcast.Messaging;

namespace Hazelcast.Protocol.BuiltInCodecs
{
    internal static class EntryListUUIDListIntegerCodec
    {
        //private const int EntrySizeInBytes = GuidSizeInBytes + LongSizeInBytes;

        public static void Encode(ClientMessage clientMessage, ICollection<KeyValuePair<Guid, IList<int>>> collection)
        {
            var keyList = new List<Guid>(collection.Count);
            clientMessage.Append(Frame.CreateBeginStruct());
            foreach (var kvp in collection)
            {
                keyList.Add(kvp.Key);
                ListIntegerCodec.Encode(clientMessage, kvp.Value);
            }
            clientMessage.Append(Frame.CreateEndStruct());
            ListUUIDCodec.Encode(clientMessage, keyList);
        }

        public static void Encode(ClientMessage clientMessage, ICollection<KeyValuePair<Guid, ICollection<int>>> collection)
        {
            var keyList = new List<Guid>(collection.Count);
            clientMessage.Append(Frame.CreateBeginStruct());
            foreach (var kvp in collection)
            {
                keyList.Add(kvp.Key);
                ListIntegerCodec.Encode(clientMessage, kvp.Value);
            }
            clientMessage.Append(Frame.CreateEndStruct());
            ListUUIDCodec.Encode(clientMessage, keyList);
        }

        public static IList<KeyValuePair<Guid, IList<int>>> Decode(IEnumerator<Frame> iterator)
        {
            var listV = ListMultiFrameCodec.Decode(iterator, ListIntegerCodec.Decode);
            var listK = ListUUIDCodec.Decode(iterator);

            var result = new List<KeyValuePair<Guid, IList<int>>>(listK.Count);
            for (var i = 0; i < listK.Count; i++)
            {
                result.Add(new KeyValuePair<Guid, IList<int>>(listK[i], listV[i]));
            }
            return result;
        }
    }
}
