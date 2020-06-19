﻿// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
    internal static class EntryListUUIDLongCodec
    {
        private const int EntrySizeInBytes = BytesExtensions.SizeOfGuid + BytesExtensions.SizeOfLong;

        public static void Encode(ClientMessage clientMessage, IEnumerable<KeyValuePair<Guid, long>> collection)
        {
            var itemCount = collection.Count();
            var frame = new Frame(new byte[itemCount * EntrySizeInBytes]);
            var i = 0;
            foreach (var kvp in collection)
            {
                frame.Bytes.WriteGuid(i * EntrySizeInBytes, kvp.Key);
                frame.Bytes.WriteLong(i * EntrySizeInBytes + BytesExtensions.SizeOfGuid, kvp.Value);
                i++;
            }
            clientMessage.Append(frame);
        }

        public static IList<KeyValuePair<Guid, long>> Decode(IEnumerator<Frame> iterator)
        {
            var frame = iterator.Take();
            var itemCount = frame.Bytes.Length / EntrySizeInBytes;
            var result = new List<KeyValuePair<Guid, long>>(itemCount);
            for (var i = 0; i < itemCount; i++)
            {
                var key = frame.Bytes.ReadGuid(i * EntrySizeInBytes);
                var value = frame.Bytes.ReadLong(i * EntrySizeInBytes + BytesExtensions.SizeOfGuid);
                result.Add(new KeyValuePair<Guid, long>(key, value));
            }
            return result;
        }
    }
}
