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
using static Hazelcast.IO.Bits;
using static Hazelcast.Client.Protocol.Codec.BuiltIn.FixedSizeTypesCodec;
using static Hazelcast.Client.Protocol.ClientMessage;

namespace Hazelcast.Client.Protocol.Codec.BuiltIn
{
    internal static class EntryListIntegerIntegerCodec
    {
        private const int EntrySizeInBytes = IntSizeInBytes + IntSizeInBytes;

        public static void Encode(ClientMessage clientMessage, ICollection<KeyValuePair<int, int>> collection)
        {
            var itemCount = collection.Count;
            var frame = new Frame(new byte[itemCount * EntrySizeInBytes]);
            var i = 0;
            foreach (var kvp in collection)
            {
                EncodeInt(frame.Content, i * EntrySizeInBytes, kvp.Key);
                EncodeInt(frame.Content, i * EntrySizeInBytes + IntSizeInBytes, kvp.Value);
                i++;
            }
            clientMessage.Add(frame);
        }

        public static IList<KeyValuePair<int, int>> Decode(FrameIterator iterator)
        {
            var frame = iterator.Next();
            var itemCount = frame.Content.Length / EntrySizeInBytes;
            var result = new List<KeyValuePair<int, int>>();
            for (int i = 0; i < itemCount; i++)
            {
                var key = DecodeInt(frame.Content, i * EntrySizeInBytes);
                var value = DecodeInt(frame.Content, i * EntrySizeInBytes + IntSizeInBytes);
                result.Add(new KeyValuePair<int, int>(key, value));
            }
            return result;
        }
    }
}