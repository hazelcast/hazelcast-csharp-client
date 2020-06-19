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
using System.Linq;
using Hazelcast.Core;
using Hazelcast.Messaging;

namespace Hazelcast.Protocol.BuiltInCodecs
{
    internal static class ListLongCodec
    {
        public static void Encode(ClientMessage clientMessage, IEnumerable<long> collection)
        {
            var itemCount = collection.Count();
            var frame = new Frame(new byte[itemCount * BytesExtensions.SizeOfLong]);

            var i = 0;
            foreach (var value in collection)
            {
                frame.Bytes.WriteLong(i * BytesExtensions.SizeOfLong, value);
                i++;
            }

            clientMessage.Append(frame);
        }

        public static List<long> Decode(IEnumerator<Frame> iterator)
        {
            return Decode(iterator.Take());
        }

        public static List<long> Decode(Frame frame)
        {
            // frame.Bytes is never null
            var itemCount = frame.Bytes.Length / BytesExtensions.SizeOfLong;
            var result = new List<long>(itemCount);
            for (var i = 0; i < itemCount; i++)
            {
                result.Add(frame.Bytes.ReadLong(i * BytesExtensions.SizeOfLong));
            }
            return result;
        }
    }
}
