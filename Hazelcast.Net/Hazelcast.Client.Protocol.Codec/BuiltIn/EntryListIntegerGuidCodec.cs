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

using System;
using System.Collections.Generic;
using System.Linq;
using static Hazelcast.IO.Bits;
using static Hazelcast.Client.Protocol.Codec.BuiltIn.FixedSizeTypesCodec;
using static Hazelcast.Client.Protocol.ClientMessage;

namespace Hazelcast.Client.Protocol.Codec.BuiltIn
{
    internal static class EntryListIntegerUUIDCodec
    {
        private const int EntrySizeInBytes = IntSizeInBytes + GuidSizeInBytes;

        public static void Encode(ClientMessage clientMessage, IEnumerable<KeyValuePair<int, Guid>> collection)
        {
            var itemCount = collection.Count();
            var frame = new Frame(new byte[itemCount * EntrySizeInBytes]);

            var i = 0;
            foreach (var kvp in collection)
            {
                EncodeInt(frame.Content, i * EntrySizeInBytes, kvp.Key);
                EncodeGuid(frame.Content, i * EntrySizeInBytes + IntSizeInBytes, kvp.Value);
                i++;
            }
            clientMessage.Add(frame);
        }

        public static IList<KeyValuePair<int, Guid>> Decode(ref FrameIterator iterator)
        {
            ref var frame = ref iterator.Next();
            var itemCount = frame.Content.Length / EntrySizeInBytes;
            var result = new List<KeyValuePair<int, Guid>>();
            for (var i = 0; i < itemCount; i++)
            {
                var key = DecodeInt(frame.Content, i * EntrySizeInBytes);
                var value = DecodeGuid(frame.Content, i * EntrySizeInBytes + IntSizeInBytes);
                result.Add(new KeyValuePair<int, Guid>(key, value));
            }
            return result;
        }
    }
}