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

using System.Collections.Generic;
using Hazelcast.Core;
using Hazelcast.Messaging;

namespace Hazelcast.Protocol.BuiltInCodecs
{
    internal static class ListIntegerCodec
    {
        public static void Encode(ClientMessage clientMessage, ICollection<int> collection)
        {
            var itemCount = collection.Count;
            var frame = new Frame(new byte[itemCount * BytesExtensions.SizeOfInt]);

            var i = 0;
            foreach (var value in collection)
            {
                frame.Bytes.WriteIntL(i* BytesExtensions.SizeOfInt, value);
                i++;
            }

            clientMessage.Append(frame);
        }

        public static IList<int> Decode(IEnumerator<Frame> iterator)
        {
            return Decode(iterator.Take());
        }

        public static IList<int> Decode(Frame frame)
        {
            // frame.Bytes is never null
            var itemCount = frame.Bytes.Length / BytesExtensions.SizeOfInt;
            var result = new List<int>(itemCount);
            for (var i = 0; i < itemCount; i++)
            {
                result.Add(frame.Bytes.ReadIntL(i * BytesExtensions.SizeOfInt));
            }
            return result;
        }
    }
}
