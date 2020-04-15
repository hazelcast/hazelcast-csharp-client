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
using Hazelcast.Protocol.Portability;
using static Hazelcast.Protocol.Portability.Temp;

namespace Hazelcast.Protocol.BuiltInCodecs
{
    internal static class EntryListIntegerUUIDCodec
    {
        private const int EntrySizeInBytes = IntSizeInBytes + GuidSizeInBytes;

        public static void Encode(ClientMessage clientMessage, ICollection<KeyValuePair<int, Guid>> collection)
        {
            var itemCount = collection.Count;
            var frame = new Frame(new byte[itemCount * EntrySizeInBytes]);

            var i = 0;
            foreach (var kvp in collection)
            {
                EncodeInt(frame, i * EntrySizeInBytes, kvp.Key);
                EncodeGuid(frame, i * EntrySizeInBytes + IntSizeInBytes, kvp.Value);
                i++;
            }
            clientMessage.Add(frame);
        }

        public static IList<KeyValuePair<int, Guid>> Decode(FrameIterator iterator)
        {
            var frame = iterator.Take();
            var itemCount = frame.Bytes.Length / EntrySizeInBytes;
            var result = new List<KeyValuePair<int, Guid>>();
            for (var i = 0; i < itemCount; i++)
            {
                var key = DecodeInt(frame, i * EntrySizeInBytes);
                var value = DecodeGuid(frame, i * EntrySizeInBytes + IntSizeInBytes);
                result.Add(new KeyValuePair<int, Guid>(key, value));
            }
            return result;
        }
    }
}