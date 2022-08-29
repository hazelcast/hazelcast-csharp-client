// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Core;
using Hazelcast.Messaging;

namespace Hazelcast.Protocol.BuiltInCodecs
{
    internal static class EntryListIntegerUUIDCodec
    {
        private const int EntrySizeInBytes = BytesExtensions.SizeOfInt + BytesExtensions.SizeOfGuid;

        public static void Encode(ClientMessage clientMessage, ICollection<KeyValuePair<int, Guid>> collection)
        {
            var itemCount = collection.Count;
            var frame = new Frame(new byte[itemCount * EntrySizeInBytes]);

            var i = 0;
            foreach (var kvp in collection)
            {
                frame.Bytes.WriteIntL(i * EntrySizeInBytes, kvp.Key);
                frame.Bytes.WriteGuidL(i * EntrySizeInBytes + BytesExtensions.SizeOfInt, kvp.Value);
                i++;
            }
            clientMessage.Append(frame);
        }

        public static IList<KeyValuePair<int, Guid>> Decode(IEnumerator<Frame> iterator)
        {
            var frame = iterator.Take();
            var itemCount = frame.Bytes.Length / EntrySizeInBytes;
            var result = new List<KeyValuePair<int, Guid>>();
            for (var i = 0; i < itemCount; i++)
            {
                var key = frame.Bytes.ReadIntL(i * EntrySizeInBytes);
                var value = frame.Bytes.ReadGuidL(i * EntrySizeInBytes + BytesExtensions.SizeOfInt);
                result.Add(new KeyValuePair<int, Guid>(key, value));
            }
            return result;
        }
    }
}
